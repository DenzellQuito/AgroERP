using AgroERP.Models.ActivosFijos;
using AgroERP.Models.Contabilidad;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Lavado. Escribe ActivosFijos, DepreciacionesActivo.
public class DAActivos
{
    private readonly ApplicationDbContext _db;
    private readonly DAPagos _pagos;
    private readonly DAContabilidad _contabilidad;

    public DAActivos(ApplicationDbContext db, DAPagos pagos, DAContabilidad contabilidad)
    {
        _db = db;
        _pagos = pagos;
        _contabilidad = contabilidad;
    }

    /// Contrato llamado por Méndez al transferir un activo fijo. No llama SaveChangesAsync.
    public async Task ActualizarUbicacionYCustodio(int activoId, int zonaUbicacionId, string usuarioResponsableId)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null) throw new Exception("Activo no encontrado.");

        activo.ZonaUbicacionId = zonaUbicacionId;
        activo.UsuarioResponsableId = usuarioResponsableId;

        _db.Set<ActivoFijo>().Update(activo);
    }

    /// Contrato llamado por Méndez al registrar una mejora capitalizable. No llama SaveChangesAsync.
    public async Task CapitalizarMejora(int activoId, decimal montoMejora)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null) throw new Exception("Activo no encontrado.");

        // La mejora suma valor al costo histórico y al valor neto según NIC 16
        activo.CostoAdquisicion += montoMejora;
        activo.ValorNetoLibros += montoMejora;

        _db.Set<ActivoFijo>().Update(activo);
    }

    /// Contrato llamado por Méndez al dar de baja un activo fijo. No llama SaveChangesAsync.
    public async Task MarcarActivoDadoDeBaja(int activoId)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null) throw new Exception("Activo no encontrado.");

        // Texto exacto de estado según regla 8 del README
        activo.Estado = "DadoDeBaja";

        _db.Set<ActivoFijo>().Update(activo);
    }

    // Método para listar los activos en la pantalla principal
    public async Task<List<ActivoFijo>> ObtenerActivosAsync()
    {
        return await _db.Set<ActivoFijo>()
            .Include(a => a.ZonaUbicacion) // Traemos los datos de la zona cruzando tablas
            .ToListAsync();
    }

    // Método para crear el activo en la BD
    public async Task CrearActivoAsync(ActivoFijo nuevoActivo)
    {
        nuevoActivo.Codigo = nuevoActivo.Codigo.Trim().ToUpper();

        if (await _db.Set<ActivoFijo>().AnyAsync(a => a.Codigo == nuevoActivo.Codigo))
            throw new Exception($"Ya existe un activo fijo registrado con el código '{nuevoActivo.Codigo}'.");

        if (nuevoActivo.ValorResidual >= nuevoActivo.CostoAdquisicion)
            throw new Exception("El valor residual debe ser menor al costo de adquisición.");

        // Regla 8: El activo nace en estado "EnProceso"
        nuevoActivo.Estado = "EnProceso";
        nuevoActivo.FechaAdquisicion = DateTime.Now;
        nuevoActivo.DepreciacionAcumulada = 0;
        nuevoActivo.ValorNetoLibros = nuevoActivo.CostoAdquisicion;

        await _db.Set<ActivoFijo>().AddAsync(nuevoActivo);
        await _db.SaveChangesAsync();
    }

    // Método de la Fase 2 del Bizagi: Capitalización, CxP y Contabilidad
    public async Task CapitalizarActivoAsync(int activoId, string nroFactura, string usuarioId)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null || activo.Estado != "EnProceso")
            throw new Exception("El activo no existe o ya fue capitalizado.");

        if (!activo.ProveedorId.HasValue)
            throw new Exception("El activo debe tener un Proveedor asignado para generar la deuda.");

        // 1. Cambiar estado a Activo
        activo.Estado = "Activo";
        _db.Set<ActivoFijo>().Update(activo);

        // 2. Cálculos Financieros (Base + IGV)
        decimal igv = Math.Round(activo.CostoAdquisicion * 0.18m, 2, MidpointRounding.AwayFromZero);
        decimal totalFactura = activo.CostoAdquisicion + igv;

        // 3. Generar Obligación: Contrato con DAPagos (Changa)
        await _pagos.CrearCuentaPorPagar(
            proveedorId: activo.ProveedorId.Value,
            ordenCompraId: null,
            tipoObligacion: "ActivoFijo",
            nroFactura: nroFactura,
            montoTotal: totalFactura,
            diasVencimiento: 30
        );

        // 4. Detonar Asiento: Contrato con DAContabilidad (Quito)
        await _contabilidad.RegistrarAsiento(
            "AltaActivo",
            activo.Id,
            $"Capitalización de Activo Fijo {activo.Codigo}",
            usuarioId,
            (Cuentas.MaquinariaEquipo, activo.CostoAdquisicion, 0m), // Debe: Costo sin IGV
            (Cuentas.IgvPorPagar, igv, 0m),                          // Debe: IGV
            (Cuentas.PasivoActivoFijo, 0m, totalFactura)             // Haber: Total a pagar
        );

        // 5. Un solo guardado para sellar la transacción de los 3 módulos
        await _db.SaveChangesAsync();
    }

    // Método de la Fase 3 del Bizagi: Motor de Depreciación Mensual
    public async Task DepreciarActivoAsync(int activoId, string usuarioId)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null || activo.Estado != "Activo")
            throw new Exception("Solo se pueden depreciar bienes en estado 'Activo'.");

        if (activo.ValorNetoLibros <= activo.ValorResidual)
            throw new Exception("El activo ya alcanzó su valor residual mínimo. No se puede depreciar más.");

        // 1. Cálculo de depreciación lineal mensual: (Costo - ValorResidual) / VidaUtil
        decimal cuotaMensual = (activo.CostoAdquisicion - activo.ValorResidual) / activo.VidaUtilMeses;
        cuotaMensual = Math.Round(cuotaMensual, 2, MidpointRounding.AwayFromZero);

        // Tope: Asegurar que la última cuota no lo baje más allá del valor residual
        if (activo.ValorNetoLibros - cuotaMensual < activo.ValorResidual)
        {
            cuotaMensual = activo.ValorNetoLibros - activo.ValorResidual;
        }

        // 2. Actualizar los saldos del maestro
        activo.DepreciacionAcumulada += cuotaMensual;
        activo.ValorNetoLibros -= cuotaMensual;
        _db.Set<ActivoFijo>().Update(activo);

        // 3. Registrar el historial (insertar en DepreciacionesActivo)
        var historial = new DepreciacionActivo
        {
            ActivoFijoId = activo.Id,
            Anio = DateTime.Now.Year,
            Mes = DateTime.Now.Month,
            FechaRegistro = DateTime.Now,
            MontoDepreciado = cuotaMensual,
            DepreciacionAcumuladaAlCierre = activo.DepreciacionAcumulada,
            ValorNetoResultante = activo.ValorNetoLibros
        };
        await _db.Set<DepreciacionActivo>().AddAsync(historial);

        // 4. Contabilizar: Regla del README (6814 vs 3913)
        await _contabilidad.RegistrarAsiento(
            "Depreciacion",
            activo.Id,
            $"Depreciación mensual {DateTime.Now.ToString("MM-yyyy")} - Activo {activo.Codigo}",
            usuarioId,
            (Cuentas.DepreciacionGasto, cuotaMensual, 0m),       // Debe: 6814 (Gasto)
            (Cuentas.DepreciacionAcumulada, 0m, cuotaMensual)    // Haber: 3913 (Acumulada)
        );

        // Un solo guardado para sellar el mes
        await _db.SaveChangesAsync();
    }

}