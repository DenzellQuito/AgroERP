using Microsoft.EntityFrameworkCore;
using AgroERP.Models.Distribucion;
using AgroERP.Models.Contabilidad;

namespace AgroERP.Data.AccesoDatos;

public class DADistribucion
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    private readonly DAAlmacen _almacen;

    public DADistribucion(ApplicationDbContext db, DAContabilidad contabilidad, DAAlmacen almacen)
    {
        _db = db;
        _contabilidad = contabilidad;
        _almacen = almacen;
    }

    // =========================================================
    // 1. FACTURACIÓN 
    // =========================================================
    public async Task<int> FacturarPedido(int pedidoId, string usuarioId)
    {
        var pedido = await _db.PedidosVenta
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null) throw new Exception("Pedido no encontrado.");
        if (pedido.EstadoLogistico != "Entregado") throw new Exception("Solo se puede facturar un pedido entregado.");

        // Cálculos redondeados a 2 decimales (céntimos)
        decimal totalFactura = Math.Round(pedido.Total, 2, MidpointRounding.AwayFromZero);
        decimal neto = Math.Round(totalFactura / 1.18m, 2, MidpointRounding.AwayFromZero);
        decimal igv = totalFactura - neto; // La resta asegura que no haya descuadres por milésimas

        // 1. Crear el Comprobante y GUARDARLO
        var comprobante = new ComprobantePago
        {
            PedidoVentaId = pedido.Id,
            TipoCPE = "Factura",
            MontoTotal = totalFactura
        };

        _db.ComprobantesPago.Add(comprobante);
        await _db.SaveChangesAsync();

        // FORZAMOS LA LECTURA DEL ID REAL DESDE LA BASE DE DATOS
        var idRealComprobante = await _db.ComprobantesPago
            .Where(c => c.PedidoVentaId == pedido.Id)
            .OrderByDescending(c => c.Id)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        // 2. Registrar el Asiento Contable
        await _contabilidad.RegistrarAsiento(
            modulo: "Factura",
            referenciaId: idRealComprobante,
            glosa: $"Facturación Pedido {pedido.Id}",
            usuarioId: usuarioId,
            (Cuentas.FacturasPorCobrar, totalFactura, 0m), // Debe
            (Cuentas.VentasProductos, 0m, neto),           // Haber
            (Cuentas.IgvPorPagar, 0m, igv)                 // Haber
        );

        await _db.SaveChangesAsync();
        return comprobante.Id;
    }

    // =========================================================
    // 2. DEVOLUCIÓN Y NOTA DE CRÉDITO
    // =========================================================
    public async Task<int> RegistrarDevolucion(int pedidoId, int articuloId, int loteId, decimal cantidadRechazada, string motivo, string usuarioId)
    {
        var pedido = await _db.PedidosVenta.FindAsync(pedidoId);
        if (pedido == null) throw new Exception("Pedido no encontrado.");

        var comprobante = await _db.ComprobantesPago.FirstOrDefaultAsync(c => c.PedidoVentaId == pedidoId);
        if (comprobante == null) throw new Exception("El pedido no tiene comprobante de pago para acreditar.");

        var detallePedido = await _db.DetallesPedidoVenta
            .FirstOrDefaultAsync(d => d.PedidoId == pedidoId && d.ArticuloId == articuloId);

        if (detallePedido == null) throw new Exception("El artículo no pertenece al pedido original.");

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var devolucion = new DevolucionCliente
            {
                PedidoVentaId = pedidoId,
                ArticuloId = articuloId,
                LoteId = loteId,
                CantidadRechazada = cantidadRechazada,
                Motivo = motivo
            };
            _db.DevolucionesCliente.Add(devolucion);
            await _db.SaveChangesAsync();

            await _almacen.ReingresarDevolucion(devolucion.Id);

            // Cálculos redondeados a 2 decimales
            decimal totalDevolucion = Math.Round(cantidadRechazada * detallePedido.PrecioUnitario, 2, MidpointRounding.AwayFromZero);
            decimal netoDevolucion = Math.Round(totalDevolucion / 1.18m, 2, MidpointRounding.AwayFromZero);
            decimal igvDevolucion = totalDevolucion - netoDevolucion;

            var notaCredito = new NotaCredito
            {
                DevolucionId = devolucion.Id,
                ComprobantePagoId = comprobante.Id,
                Fecha = DateTime.Now,
                Monto = totalDevolucion
            };

            _db.NotaCredito.Add(notaCredito);
            await _db.SaveChangesAsync();

            // Forzamos lectura del ID real de la nota de crédito
            var idRealNota = await _db.NotaCredito
                .Where(n => n.DevolucionId == devolucion.Id)
                .OrderByDescending(n => n.Id)
                .Select(n => n.Id)
                .FirstOrDefaultAsync();

            await _contabilidad.RegistrarAsiento(
                modulo: "NotaCredito",
                referenciaId: idRealNota,
                glosa: $"Nota de Crédito por Devolución {devolucion.Id} - Pedido {pedido.Id}",
                usuarioId: usuarioId,
                (Cuentas.VentasProductos, netoDevolucion, 0m),
                (Cuentas.IgvPorPagar, igvDevolucion, 0m),
                (Cuentas.FacturasPorCobrar, 0m, totalDevolucion)
            );

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return notaCredito.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Error al registrar la devolución: " + ex.Message, ex);
        }
    }

    // =========================================================
    // 3. RESERVAR PEDIDO
    // =========================================================
    public async Task<bool> IntentarReservarPedido(int pedidoId)
    {
        var detalles = await _db.DetallesPedidoVenta
            .Where(d => d.PedidoId == pedidoId)
            .ToListAsync();

        foreach (var detalle in detalles)
        {
            bool exito = await _almacen.ReservarStock(detalle.Id, detalle.ArticuloId, detalle.Cantidad);
            if (!exito) return false;
        }

        await _db.SaveChangesAsync();
        return true;
    }
}