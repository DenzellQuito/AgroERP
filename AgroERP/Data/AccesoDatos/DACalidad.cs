using AgroERP.Models.Calidad_WMS;
using AgroERP.Models.Compras;
using AgroERP.Models.Contabilidad;
using AgroERP.ViewModels.Calidad;
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

    // 1. REGISTRAR RECEPCIÓN (3-Way Matching Manual)
    public async Task<int> RegistrarRecepcion(RegistrarRecepcionViewModel model, string usuarioId)
    {
        var oc = await _db.OrdenesCompra.Include(o => o.Detalles).FirstOrDefaultAsync(o => o.Id == model.OrdenCompraId);
        if (oc == null) throw new Exception("Orden de Compra no encontrada.");

        if (oc.Estado != "Emitida") throw new Exception("Esta Orden de Compra ya fue procesada o no está disponible para recepción.");

        // 🔥 VALIDACIÓN EXTREMA: Evita que una OC ya recibida (aprobada o rechazada) vuelva a entrar
        var yaTieneRecepcion = await _db.RecepcionesMercaderia.AnyAsync(r => r.OrdenCompraId == model.OrdenCompraId);
        if (yaTieneRecepcion)
            throw new Exception("Esta Orden de Compra ya tiene una recepción registrada. Debe generar una nueva OC para recibir más mercadería.");

        var guiaExistente = await _db.RecepcionesMercaderia.AnyAsync(r => r.GuiaProveedor == model.GuiaProveedor);
        if (guiaExistente) throw new Exception($"La guía '{model.GuiaProveedor}' ya fue registrada en el sistema. Verifique el número.");

        var recepcion = new RecepcionMercaderia
        {
            OrdenCompraId = model.OrdenCompraId,
            GuiaProveedor = model.GuiaProveedor,
            FechaRecepcion = DateTime.Now,
            UsuarioReceptorId = usuarioId,
            Estado = "EnProceso",
            Detalles = new List<RecepcionDetalle>()
        };

        foreach (var detModel in model.Detalles)
        {
            var nuevoLote = new Lote
            {
                ArticuloId = detModel.ArticuloId,
                NumeroLote = detModel.NumeroLote,
                FechaVencimiento = detModel.FechaVencimiento,
                EstadoCalidad = "Cuarentena"
            };

            recepcion.Detalles.Add(new RecepcionDetalle
            {
                ArticuloId = detModel.ArticuloId,
                Lote = nuevoLote, // Asignación por navegación
                CantidadPedida = detModel.CantidadPedida,
                CantidadGuia = detModel.CantidadGuia,
                CantidadRecibida = detModel.CantidadRecibida
            });
        }

        _db.RecepcionesMercaderia.Add(recepcion);

        await _db.SaveChangesAsync(); // UN SOLO SaveChanges
        return recepcion.Id;
    }

    // 2. INSPECCIÓN DE CALIDAD (Liberar / Rechazar)
    public async Task InspeccionarLote(int recepcionDetalleId, bool esConforme, string? motivoRechazo, int zonaId, string usuarioId)
    {
        var detalle = await _db.DetallesRecepcion
            .Include(d => d.Lote)
            .Include(d => d.Recepcion)
            .FirstOrDefaultAsync(d => d.Id == recepcionDetalleId);

        if (detalle == null) throw new Exception("Detalle de recepción no encontrado.");
        if (detalle.Lote.EstadoCalidad != "Cuarentena") throw new Exception("Este lote ya fue inspeccionado.");

        _db.InspeccionesCalidad.Add(new InspeccionCalidad
        {
            LoteId = detalle.LoteId,
            UsuarioInspectorId = usuarioId,
            EsConforme = esConforme,
            MotivoRechazo = motivoRechazo
        });

        if (esConforme)
        {
            detalle.Lote.EstadoCalidad = "Liberado";

            await _almacen.RegistrarIngreso(detalle.LoteId, zonaId, detalle.CantidadRecibida, $"OC-{detalle.Recepcion.OrdenCompraId}");

            var precio = await _db.DetallesOrdenCompra
                .Where(d => d.OrdenCompraId == detalle.Recepcion.OrdenCompraId && d.ArticuloId == detalle.ArticuloId)
                .Select(d => d.PrecioUnitario).FirstOrDefaultAsync();

            decimal monto = Math.Round(detalle.CantidadRecibida * precio, 2, MidpointRounding.AwayFromZero);

            var articulo = await _db.Articulos.FindAsync(detalle.ArticuloId);
            string cuentaDebe = articulo?.Tipo == "Empaque" ? Cuentas.Envases : Cuentas.MaterialesAux;

            await _contabilidad.RegistrarAsiento(
                modulo: "Recepcion",
                referenciaId: detalle.RecepcionId,
                glosa: $"Recepción conforme Lote {detalle.Lote.NumeroLote}",
                usuarioId: usuarioId,
                (cuentaDebe, monto, 0m),
                (Cuentas.FacturasPorPagar, 0m, monto)
            );
        }
        else
        {
            detalle.Lote.EstadoCalidad = "Rechazado";
        }

        await _db.SaveChangesAsync();

        var recepcion = detalle.Recepcion;
        var todosDetalles = await _db.DetallesRecepcion.Include(d => d.Lote).Where(d => d.RecepcionId == recepcion.Id).ToListAsync();

        if (!todosDetalles.Any(d => d.Lote.EstadoCalidad == "Cuarentena"))
        {
            recepcion.Estado = "Cerrada";

            bool parcial = todosDetalles.Any(d => d.Lote.EstadoCalidad == "Rechazado" || d.CantidadRecibida < d.CantidadPedida);

            await _compras.MarcarOrdenRecepcionada(recepcion.OrdenCompraId, parcial);
            await _db.SaveChangesAsync();
        }
    }
}