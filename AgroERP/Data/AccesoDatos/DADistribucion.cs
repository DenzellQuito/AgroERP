using Microsoft.EntityFrameworkCore;
using AgroERP.Models.Distribucion;
using AgroERP.Models.Contabilidad;

namespace AgroERP.Data.AccesoDatos;

public class DADistribucion
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    private readonly DAAlmacen _almacen;
    private readonly DACobranzas _cobranzas;

    public DADistribucion(ApplicationDbContext db, DAContabilidad contabilidad, DAAlmacen almacen, DACobranzas cobranzas)
    {
        _db = db;
        _contabilidad = contabilidad;
        _almacen = almacen;
        _cobranzas = cobranzas;
    }

    // 1. FACTURACIÓN 
    public async Task<int> FacturarPedido(int pedidoId, string usuarioId)
    {
        var pedido = await _db.PedidosVenta
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null) throw new Exception("Pedido no encontrado.");
        if (pedido.EstadoLogistico != "Entregado") throw new Exception("Solo se puede facturar un pedido entregado.");

        decimal totalFactura = Math.Round(pedido.Total, 2, MidpointRounding.AwayFromZero);
        decimal neto = Math.Round(totalFactura / 1.18m, 2, MidpointRounding.AwayFromZero);
        decimal igv = totalFactura - neto;

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var comprobante = new ComprobantePago
            {
                PedidoVentaId = pedido.Id,
                TipoCPE = "Factura",
                MontoTotal = totalFactura
            };

            _db.ComprobantesPago.Add(comprobante);
            await _db.SaveChangesAsync();

            await _contabilidad.RegistrarAsiento(
                modulo: "Factura",
                referenciaId: comprobante.Id,
                glosa: $"Facturación Pedido {pedido.Id}",
                usuarioId: usuarioId,
                (Cuentas.FacturasPorCobrar, totalFactura, 0m),
                (Cuentas.VentasProductos, 0m, neto),
                (Cuentas.IgvPorPagar, 0m, igv)
            );

            // Creamos la Cuenta por Cobrar en el módulo de Cobranzas
            await _cobranzas.CrearCuentaPorCobrar(comprobante.Id, 30);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return comprobante.Id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // 2. DEVOLUCIÓN Y NOTA DE CRÉDITO (CON VALIDACIÓN FINANCIERA)
    public async Task<int> RegistrarDevolucion(int pedidoId, int articuloId, int loteId, decimal cantidadRechazada, string motivo, string usuarioId)
    {
        var pedido = await _db.PedidosVenta.FindAsync(pedidoId);
        if (pedido == null) throw new Exception("Pedido no encontrado.");

        var comprobante = await _db.ComprobantesPago.FirstOrDefaultAsync(c => c.PedidoVentaId == pedidoId);
        if (comprobante == null) throw new Exception("El pedido no tiene comprobante de pago para acreditar.");

        // 🔥 VALIDACIÓN DE SEGURIDAD FINANCIERA
        var cxc = await _db.CuentasPorCobrar.FirstOrDefaultAsync(c => c.ComprobantePagoId == comprobante.Id);
        if (cxc == null) throw new Exception("No se encontró una Cuenta por Cobrar asociada a esta factura.");

        if (cxc.Estado == "Cobrado")
            throw new Exception("No se puede registrar la devolución porque esta factura ya fue pagada en su totalidad. Debe gestionar la devolución en efectivo (Tesorería).");

        var detallePedido = await _db.DetallesPedidoVenta
            .FirstOrDefaultAsync(d => d.PedidoId == pedidoId && d.ArticuloId == articuloId);

        if (detallePedido == null) throw new Exception("El artículo no pertenece al pedido original.");

        decimal totalDevolucion = Math.Round(cantidadRechazada * detallePedido.PrecioUnitario, 2, MidpointRounding.AwayFromZero);

        // Validación extra: No devolver más de lo que debe
        if (totalDevolucion > cxc.SaldoPendiente)
            throw new Exception($"El monto a devolver (S/ {totalDevolucion:N2}) supera el saldo pendiente de la deuda (S/ {cxc.SaldoPendiente:N2}).");

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

            // Aplicamos el descuento a la Cuenta por Cobrar
            await _cobranzas.AplicarNotaCredito(comprobante.Id, totalDevolucion);

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

    // 3. RESERVAR PEDIDO
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