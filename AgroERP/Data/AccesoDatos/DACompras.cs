using AgroERP.Models.Compras;
using AgroERP.Models.MRP_IA; // Agregado para poder leer la clase NecesidadMrp de Quito
using Microsoft.EntityFrameworkCore;
using AgroERP.Models.Maestros;
using AgroERP.Models.Contabilidad;
using AgroERP.Models.BI_Gestion;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Changa. Tablas que escribe: Proveedor, RequerimientoCompra, RequerimientoDetalle,
/// CotizacionProveedor, OrdenCompra, OrdenCompraDetalle, GastoLogistico.
public class DACompras
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    private readonly DAPagos _pagos;

    public DACompras(ApplicationDbContext db, DAContabilidad contabilidad, DAPagos pagos)
    {
        _db = db;
        _contabilidad = contabilidad;
        _pagos = pagos;
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
        var orden = await _db.Set<OrdenCompra>()
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.Id == ordenCompraId);

        if (orden != null)
        {
            // Cambiar el estado en texto exacto (Regla 8)
            orden.Estado = parcial ? "RecepcionadaParcial" : "Cerrada";

            if (!parcial)
            {
                var articuloIds = orden.Detalles.Select(d => d.ArticuloId).Distinct().ToList();
                var necesidades = await _db.Set<NecesidadMrp>()
                    .Where(n => n.Estado == "ConRequerimiento" && articuloIds.Contains(n.ArticuloId))
                    .ToListAsync();

                foreach (var n in necesidades)
                {
                    n.Estado = "Atendida";
                }
            }
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

    public async Task AprobarRequerimientoAsync(int requerimientoId)
    {
        var requerimiento = await _db.Set<RequerimientoCompra>().FindAsync(requerimientoId)
            ?? throw new InvalidOperationException("Requerimiento no encontrado.");

        if (requerimiento.Estado != "PendienteAprobacion")
            throw new InvalidOperationException("Solo se pueden aprobar requerimientos en estado PendienteAprobacion.");

        requerimiento.Estado = "Aprobado";
        await _db.SaveChangesAsync();
    }

    public async Task RechazarRequerimientoAsync(int requerimientoId)
    {
        var requerimiento = await _db.Set<RequerimientoCompra>().FindAsync(requerimientoId)
            ?? throw new InvalidOperationException("Requerimiento no encontrado.");

        if (requerimiento.Estado != "PendienteAprobacion")
            throw new InvalidOperationException("Solo se pueden rechazar requerimientos en estado PendienteAprobacion.");

        requerimiento.Estado = "Rechazado";
        await _db.SaveChangesAsync();
    }

    public async Task DevolverABorradorAsync(int requerimientoId)
    {
        var requerimiento = await _db.Set<RequerimientoCompra>().FindAsync(requerimientoId)
            ?? throw new InvalidOperationException("Requerimiento no encontrado.");

        if (requerimiento.Estado != "PendienteAprobacion")
            throw new InvalidOperationException("Solo se pueden devolver a borrador requerimientos en estado PendienteAprobacion.");

        requerimiento.Estado = "Borrador";
        await _db.SaveChangesAsync();
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
        var rucLimpio = (proveedor.Ruc ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(rucLimpio))
            throw new InvalidOperationException("El RUC del proveedor es obligatorio.");

        if (await _db.Set<Proveedor>().AnyAsync(p => p.Ruc == rucLimpio))
            throw new InvalidOperationException($"Ya existe un proveedor registrado con el RUC '{rucLimpio}'.");

        proveedor.Ruc = rucLimpio;
        proveedor.RazonSocial = (proveedor.RazonSocial ?? string.Empty).Trim();

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

    public async Task<AgroERP.Models.Compras.RequerimientoCompra?> ObtenerRequerimientoCompletoAsync(int requerimientoId)
    {
        return await _db.Set<AgroERP.Models.Compras.RequerimientoCompra>()
            .Include(r => r.Detalles).ThenInclude(d => d.Articulo)
            .Include(r => r.Cotizaciones).ThenInclude(c => c.Proveedor)
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
            .FirstOrDefaultAsync(r => r.Id == requerimientoId)
            ?? throw new InvalidOperationException("Requerimiento no encontrado.");

        if (req.Estado == "Rechazado")
            throw new InvalidOperationException("No se puede generar una orden de compra para un requerimiento rechazado.");

        if (!req.Detalles.Any())
            throw new InvalidOperationException("El requerimiento no tiene artículos solicitados.");

        var cantidadTotal = req.Detalles.Sum(x => x.Cantidad);
        if (cantidadTotal <= 0)
            throw new InvalidOperationException("La cantidad total del requerimiento debe ser mayor a cero.");

        var ganadora = req.Cotizaciones.FirstOrDefault(c => c.EsGanador)
            ?? throw new InvalidOperationException("Debe adjudicar una cotización ganadora antes de generar la orden de compra.");

        var precioProrrateado = Math.Round(ganadora.PrecioOfertado / cantidadTotal, 4, MidpointRounding.AwayFromZero);

        var orden = new AgroERP.Models.Compras.OrdenCompra
        {
            ProveedorId = ganadora.ProveedorId,
            FechaEmision = DateTime.Now,
            Total = Math.Round(ganadora.PrecioOfertado, 2, MidpointRounding.AwayFromZero),
            Estado = "Emitida", // Regla 8 de tu equipo: texto exacto
            Detalles = req.Detalles.Select(d => new AgroERP.Models.Compras.OrdenCompraDetalle
            {
                ArticuloId = d.ArticuloId,
                CantidadPedida = d.Cantidad,
                PrecioUnitario = precioProrrateado
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
        var orden = await _db.Set<OrdenCompra>().FindAsync(ordenId)
            ?? throw new InvalidOperationException("Orden de compra no encontrada.");

        costo = Math.Round(costo, 2, MidpointRounding.AwayFromZero);
        if (costo <= 0)
            throw new InvalidOperationException("El costo del flete debe ser mayor a cero.");

        await _contabilidad.RegistrarAsiento(
            "Compras",
            ordenId,
            string.IsNullOrWhiteSpace(glosa) ? $"Flete OC-{ordenId:D4}" : glosa.Trim(),
            usuarioId,
            (Cuentas.FletesGasto, costo, 0m),
            (Cuentas.FacturasPorPagar, 0m, costo)
        );

        _db.Set<GastoLogistico>().Add(new GastoLogistico
        {
            Concepto = $"Flete OC-{ordenId:D4}",
            Monto = costo,
            FechaRegistro = DateTime.Now
        });

        await _pagos.CrearCuentaPorPagar(orden.ProveedorId, orden.Id, "Flete", $"FLETE-OC-{orden.Id:D4}", costo, 15);

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