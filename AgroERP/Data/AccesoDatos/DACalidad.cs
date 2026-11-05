using AgroERP.Models.Calidad_WMS;
using AgroERP.Models.Compras;
using AgroERP.Models.Contabilidad;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: George Lavado. Tablas que escribe: RecepcionMercaderia, RecepcionDetalle, Lote, InspeccionCalidad.
/// REGLA: solo este DAO cambia Lote.EstadoCalidad (Cuarentena -> Liberado / Rechazado).
public class DACalidad
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    private readonly DAAlmacen _almacen;
    private readonly DACompras _compras;

    public DACalidad(ApplicationDbContext db, DAContabilidad contabilidad, DAAlmacen almacen, DACompras compras)
    {
        _db = db;
        _contabilidad = contabilidad;
        _almacen = almacen;
        _compras = compras;
    }

    // 1. REGISTRAR RECEPCIÓN
    public async Task<int> RegistrarRecepcion(int ordenCompraId, string guiaProveedor, string usuarioId)
    {
        var oc = await _db.OrdenesCompra.Include(o => o.Detalles).FirstOrDefaultAsync(o => o.Id == ordenCompraId);
        if (oc == null) throw new Exception("Orden de Compra no encontrada.");

        // VALIDACIÓN DE SEGURIDAD: Evita doble recepción de la misma orden
        if (oc.Estado != "Emitida") throw new Exception("Esta Orden de Compra ya fue procesada o no está disponible para recepción.");

        // VALIDACIÓN NUEVA: Evita guías duplicadas
        var guiaExistente = await _db.RecepcionesMercaderia.AnyAsync(r => r.GuiaProveedor == guiaProveedor);
        if (guiaExistente) throw new Exception($"La guía '{guiaProveedor}' ya fue registrada en el sistema. Verifique el número.");

        var recepcion = new RecepcionMercaderia
        {
            OrdenCompraId = ordenCompraId,
            GuiaProveedor = guiaProveedor,
            FechaRecepcion = DateTime.Now,
            UsuarioReceptorId = usuarioId,
            Estado = "EnProceso",
            Detalles = new List<RecepcionDetalle>()
        };

        foreach (var detOc in oc.Detalles)
        {
            var lote = new Lote
            {
                ArticuloId = detOc.ArticuloId,
                NumeroLote = $"LOT-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
                FechaVencimiento = DateTime.Now.AddMonths(6),
                EstadoCalidad = "Cuarentena"
            };
            _db.Lotes.Add(lote);
            await _db.SaveChangesAsync(); // Guardamos para obtener el LoteId

            recepcion.Detalles.Add(new RecepcionDetalle
            {
                ArticuloId = detOc.ArticuloId,
                LoteId = lote.Id,
                CantidadPedida = detOc.CantidadPedida,
                CantidadGuia = detOc.CantidadPedida,
                CantidadRecibida = detOc.CantidadPedida
            });
        }

        _db.RecepcionesMercaderia.Add(recepcion);

        // Cerramos la Orden de Compra
        oc.Estado = "Cerrada";

        await _db.SaveChangesAsync();
        return recepcion.Id;
    }

    // 2. INSPECCIÓN DE CALIDAD (Liberar o Rechazar)
    public async Task InspeccionarLote(int recepcionDetalleId, bool esConforme, string? motivoRechazo, string usuarioId)
    {
        var detalle = await _db.DetallesRecepcion
            .Include(d => d.Lote)
            .Include(d => d.Recepcion)
            .FirstOrDefaultAsync(d => d.Id == recepcionDetalleId);

        if (detalle == null) throw new Exception("Detalle de recepción no encontrado.");
        if (detalle.Lote.EstadoCalidad != "Cuarentena") throw new Exception("Este lote ya fue inspeccionado.");

        var inspeccion = new InspeccionCalidad
        {
            LoteId = detalle.LoteId,
            UsuarioInspectorId = usuarioId,
            EsConforme = esConforme,
            MotivoRechazo = motivoRechazo
        };
        _db.InspeccionesCalidad.Add(inspeccion);

        if (esConforme)
        {
            detalle.Lote.EstadoCalidad = "Liberado";

            var zonaId = await _db.ZonasUbicacion.Select(z => z.Id).FirstOrDefaultAsync();
            if (zonaId == 0) throw new Exception("No hay zonas de ubicación creadas. Crea una en Almacén primero.");

            // Avisamos a Méndez para que ingrese el stock
            await _almacen.RegistrarIngreso(detalle.LoteId, zonaId, detalle.CantidadRecibida, $"OC-{detalle.Recepcion.OrdenCompraId}");

            // Avisamos a Quito para el asiento contable
            decimal monto = detalle.CantidadRecibida * 10m;
            await _contabilidad.RegistrarAsiento(
                modulo: "Recepcion",
                referenciaId: detalle.RecepcionId,
                glosa: $"Recepción conforme Lote {detalle.Lote.NumeroLote}",
                usuarioId: usuarioId,
                (Cuentas.MaterialesAux, monto, 0m),
                (Cuentas.FacturasPorPagar, 0m, monto)
            );
        }
        else
        {
            detalle.Lote.EstadoCalidad = "Rechazado";
        }

        await _db.SaveChangesAsync();
    }
}