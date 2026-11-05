using AgroERP.Models.Compras;
using AgroERP.Models.MRP_IA; // Agregado para poder leer la clase NecesidadMrp de Quito
using Microsoft.EntityFrameworkCore;
using AgroERP.Models.Maestros;
using AgroERP.Models.Contabilidad;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Changa. Tablas que escribe: Proveedor, RequerimientoCompra, RequerimientoDetalle,
/// CotizacionProveedor, OrdenCompra, OrdenCompraDetalle, GastoLogistico.
public class DACompras
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;

    public DACompras(ApplicationDbContext db, DAContabilidad contabilidad)
    {
        _db = db;
        _contabilidad = contabilidad;
    }

    // ===================== CONTRATOS (los llaman otros módulos) =====================

    /// LLAMA: Quito (MRP). Crea un RequerimientoCompra en estado "Borrador" con un detalle por cada
    /// Necesidad_MRP (guardando NecesidadMrpId) y marca esas necesidades como "ConRequerimiento".
    public async Task<int> CrearRequerimientoDesdeMrp(List<int> necesidadIds, string usuarioId)
    {
        // 1. Validar que las necesidades existan
        var necesidades = await _db.Set<AgroERP.Models.MRP_IA.NecesidadMrp>()
            .Where(n => necesidadIds.Contains(n.Id))
            .ToListAsync();

        if (necesidades.Count != necesidadIds.Distinct().Count())
            throw new InvalidOperationException("Alguna necesidad no existe.");

        // 2. Validar que no se dupliquen (solo permitir estado Pendiente)
        if (necesidades.Any(n => n.Estado != "Pendiente"))
            throw new InvalidOperationException("Solo se pueden enviar necesidades Pendientes.");

        // 3. Crear el requerimiento
        var req = new AgroERP.Models.Compras.RequerimientoCompra
        {
            UsuarioSolicitanteId = usuarioId,
            FechaSolicitud = DateTime.Now,
            Estado = "Borrador",
            Detalles = new List<AgroERP.Models.Compras.RequerimientoDetalle>()
        };

        foreach (var n in necesidades)
        {
            req.Detalles.Add(new AgroERP.Models.Compras.RequerimientoDetalle
            {
                ArticuloId = n.ArticuloId,
                NecesidadMrpId = n.Id,
                Cantidad = n.CantidadNetaSugerida // Campo indicado por Denzell
            });

            n.Estado = "ConRequerimiento"; // Actualiza el estado para que no vuelva a jalarse
        }

        _db.Set<AgroERP.Models.Compras.RequerimientoCompra>().Add(req);

        // 4. Excepción a la Regla: Aquí SÍ llamamos a SaveChangesAsync para obtener el Id generado
        await _db.SaveChangesAsync();

        return req.Id;
    }

    /// LLAMA: Lavado (al cerrar una recepción). Cambia OrdenCompra.Estado a
    /// "RecepcionadaParcial" si parcial = true, o "Cerrada" si parcial = false.
    public async Task MarcarOrdenRecepcionada(int ordenCompraId, bool parcial)
    {
        // Buscar la orden de compra que indica Lavado
        var orden = await _db.Set<OrdenCompra>().FindAsync(ordenCompraId);

        if (orden != null)
        {
            // Cambiar el estado en texto exacto (Regla 8)
            orden.Estado = parcial ? "RecepcionadaParcial" : "Cerrada";
        }
    }

    // ===================== TU CÓDIGO AQUÍ =====================

    // 1. Obtener todos los requerimientos (Para listar en tu pantalla principal)
    public async Task<List<RequerimientoCompra>> ObtenerRequerimientosAsync()
    {
        return await _db.Set<RequerimientoCompra>()
                        .Include(r => r.Detalles)
                            .ThenInclude(d => d.Articulo)
                        .OrderByDescending(r => r.FechaSolicitud)
                        .ToListAsync();
    }

    // 2. Obtener un requerimiento específico con sus detalles
    public async Task<RequerimientoCompra?> ObtenerRequerimientoPorIdAsync(int id)
    {
        return await _db.Set<RequerimientoCompra>()
                        .Include(r => r.Detalles)
                            .ThenInclude(d => d.Articulo)
                        .FirstOrDefaultAsync(r => r.Id == id);
    }

    // 3. Crear un requerimiento manual (El que hace el Agrónomo/Capataz)
    public async Task<int> CrearRequerimientoManualAsync(string usuarioId, List<RequerimientoDetalle> detalles)
    {
        var requerimiento = new RequerimientoCompra
        {
            UsuarioSolicitanteId = usuarioId,
            FechaSolicitud = DateTime.Now,
            Estado = "Borrador", // Cumpliendo la Regla 8: Texto exacto
            Detalles = detalles
        };

        _db.Set<RequerimientoCompra>().Add(requerimiento);

        // Aquí SÍ usamos SaveChangesAsync porque esta operación nace y termina en tu módulo
        await _db.SaveChangesAsync();

        return requerimiento.Id;
    }

    // 4. Enviar a aprobación (Avanzar el flujo)
    public async Task EnviarAprobacionAsync(int requerimientoId)
    {
        var requerimiento = await _db.Set<RequerimientoCompra>().FindAsync(requerimientoId);

        if (requerimiento != null && requerimiento.Estado == "Borrador")
        {
            requerimiento.Estado = "PendienteAprobacion"; // Cumpliendo la Regla 8
            await _db.SaveChangesAsync();
        }
    }

    // 5. Obtener lista de artículos (Lectura permitida por Regla 1)
    public async Task<List<AgroERP.Models.Maestros.Articulo>> ObtenerArticulosAsync()
    {
        return await _db.Set<AgroERP.Models.Maestros.Articulo>().ToListAsync();
    }

    // ===================== CRUD PROVEEDORES =====================
    public async Task<List<Proveedor>> ObtenerProveedoresAsync()
    {
        return await _db.Set<Proveedor>().ToListAsync();
    }

    public async Task<int> CrearProveedorAsync(Proveedor proveedor)
    {
        _db.Set<Proveedor>().Add(proveedor);
        await _db.SaveChangesAsync(); // Operación interna de tu módulo (Regla 4)
        return proveedor.Id;
    }

    // ===================== COTIZACIONES =====================
    public async Task<AgroERP.Models.Compras.RequerimientoCompra?> ObtenerRequerimientoConCotizacionesAsync(int requerimientoId)
    {
        return await _db.Set<AgroERP.Models.Compras.RequerimientoCompra>()
            .Include(r => r.Cotizaciones)
            .ThenInclude(c => c.Proveedor)
            .FirstOrDefaultAsync(r => r.Id == requerimientoId);
    }

    public async Task RegistrarCotizacionAsync(int requerimientoId, int proveedorId, decimal precio, int dias)
    {
        var cotizacion = new AgroERP.Models.Compras.CotizacionProveedor
        {
            RequerimientoId = requerimientoId,
            ProveedorId = proveedorId,
            PrecioOfertado = precio,
            DiasEntrega = dias,
            EsGanador = false // Inicia sin ser ganador
        };
        _db.Set<AgroERP.Models.Compras.CotizacionProveedor>().Add(cotizacion);
        await _db.SaveChangesAsync();
    }

    public async Task AdjudicarCotizacionAsync(int cotizacionId, int requerimientoId)
    {
        var cotizaciones = await _db.Set<AgroERP.Models.Compras.CotizacionProveedor>()
            .Where(c => c.RequerimientoId == requerimientoId)
            .ToListAsync();

        foreach (var c in cotizaciones)
        {
            c.EsGanador = (c.Id == cotizacionId);
        }

        await _db.SaveChangesAsync();
    }

    // ===================== ÓRDENES DE COMPRA =====================
    public async Task<int> GenerarOrdenDesdeCotizacionAsync(int requerimientoId)
    {
        var req = await _db.Set<AgroERP.Models.Compras.RequerimientoCompra>()
            .Include(r => r.Detalles)
            .Include(r => r.Cotizaciones)
            .FirstOrDefaultAsync(r => r.Id == requerimientoId);

        var ganadora = req!.Cotizaciones.FirstOrDefault(c => c.EsGanador);

        var orden = new AgroERP.Models.Compras.OrdenCompra
        {
            ProveedorId = ganadora!.ProveedorId,
            FechaEmision = DateTime.Now,
            Total = ganadora.PrecioOfertado,
            Estado = "Emitida", // Regla 8 de tu equipo: texto exacto
            Detalles = req.Detalles.Select(d => new AgroERP.Models.Compras.OrdenCompraDetalle
            {
                ArticuloId = d.ArticuloId,
                CantidadPedida = d.Cantidad,
                PrecioUnitario = ganadora.PrecioOfertado / req.Detalles.Sum(x => x.Cantidad) // Prorrateo del precio total
            }).ToList()
        };

        req.Estado = "Aprobado"; // Actualizamos el requerimiento inicial

        _db.Set<AgroERP.Models.Compras.OrdenCompra>().Add(orden);
        await _db.SaveChangesAsync();

        return orden.Id;
    }

    public async Task<List<AgroERP.Models.Compras.OrdenCompra>> ObtenerOrdenesAsync()
    {
        return await _db.Set<AgroERP.Models.Compras.OrdenCompra>()
            .Include(o => o.Proveedor)
            .ToListAsync();
    }

    public async Task<AgroERP.Models.Compras.OrdenCompra?> ObtenerOrdenImpresionAsync(int id)
    {
        return await _db.Set<AgroERP.Models.Compras.OrdenCompra>()
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles)
            .ThenInclude(d => d.Articulo)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    // ===================== GASTOS LOGÍSTICOS (FLETES) =====================
    public async Task RegistrarFleteContableAsync(int ordenId, decimal costo, string glosa, string usuarioId)
    {
        var cuentaGasto = await _db.Set<PlanCuenta>().FirstOrDefaultAsync(c => c.Codigo == Cuentas.FletesGasto);
        if (cuentaGasto == null) throw new InvalidOperationException("La cuenta contable de fletes no está configurada.");

        var cuentaPasivo = await _db.Set<PlanCuenta>().FirstOrDefaultAsync(c => c.Codigo == Cuentas.FacturasPorPagar);
        if (cuentaPasivo == null) throw new InvalidOperationException("La cuenta contable de pasivo no está configurada.");

        var asiento = new AsientoContable
        {
            FechaContable = DateTime.Now,
            Glosa = glosa,
            ReferenciaModulo = "Compras",
            ReferenciaId = ordenId,
            UsuarioId = usuarioId, // Ahora usa el parámetro dinámico
            Detalles = new List<AsientoDetalle>
            {
                new AsientoDetalle { CuentaId = cuentaGasto.Id, Debe = costo, Haber = 0 },
                new AsientoDetalle { CuentaId = cuentaPasivo.Id, Debe = 0, Haber = costo }
            }
        };

        _db.Set<AsientoContable>().Add(asiento);
        await _db.SaveChangesAsync();
    }

    // ===================== VISTAS CONTABLES =====================
    public async Task<List<PlanCuenta>> ObtenerPlanCuentasAsync()
    {
        return await _db.Set<PlanCuenta>().ToListAsync();
    }

    public async Task<List<AsientoContable>> ObtenerLibroDiarioAsync()
    {
        return await _db.Set<AsientoContable>()
            .Include(a => a.Detalles)
            .ThenInclude(d => d.Cuenta)
            .OrderByDescending(a => a.FechaContable)
            .ToListAsync();
    }
}