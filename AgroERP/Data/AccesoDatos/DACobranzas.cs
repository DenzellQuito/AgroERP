using AgroERP.Models.Tesoreria;
using AgroERP.Models.Contabilidad;
using AgroERP.Models.Distribucion;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Díaz. Escribe CuentasPorCobrar, CobranzasCliente.
public class DACobranzas
{
    private readonly ApplicationDbContext _db;
    private readonly DATesoreria _tesoreria;
    private readonly DAContabilidad _contabilidad;

    public DACobranzas(ApplicationDbContext db, DATesoreria tesoreria, DAContabilidad contabilidad)
    {
        _db = db;
        _tesoreria = tesoreria;
        _contabilidad = contabilidad;
    }

    // ===================== CONTRATOS =====================

    /// Contrato para abrir la CuentaPorCobrar al emitir un ComprobantePago. No llama SaveChangesAsync.
    public async Task CrearCuentaPorCobrar(int comprobantePagoId, int diasCredito = 30)
    {
        var comprobante = await _db.ComprobantesPago
            .Include(c => c.PedidoVenta)
            .FirstOrDefaultAsync(c => c.Id == comprobantePagoId);

        if (comprobante == null) throw new Exception("Comprobante de pago no encontrado.");

        var cuentaPorCobrar = new CuentaPorCobrar
        {
            ComprobantePagoId = comprobantePagoId,
            ClienteId = comprobante.PedidoVenta.ClienteId,
            FechaEmision = DateTime.Now,
            FechaVencimiento = DateTime.Now.AddDays(diasCredito),
            MontoTotal = comprobante.MontoTotal,
            SaldoPendiente = comprobante.MontoTotal,
            Estado = "Pendiente"
        };

        _db.CuentasPorCobrar.Add(cuentaPorCobrar);
    }

    // ===================== FLUJO DE COBRANZA =====================

    public async Task RegistrarCobranza(int cuentaPorCobrarId, int cuentaBancariaId, decimal monto, string medioPago, string nroOperacion, string usuarioId)
    {
        var cxc = await _db.CuentasPorCobrar.FindAsync(cuentaPorCobrarId);
        if (cxc == null) throw new Exception("Cuenta por cobrar no encontrada.");
        if (cxc.Estado == "Cobrado") throw new Exception("Esta cuenta ya está totalmente cobrada.");
        if (monto <= 0) throw new Exception("El monto debe ser mayor a cero.");
        if (monto > cxc.SaldoPendiente) throw new Exception("El monto a cobrar supera el saldo pendiente.");

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // 1. Movimiento en el banco (Contrato de Quito)
            await _tesoreria.RegistrarMovimientoBancario(
                cuentaBancariaId,
                "Ingreso",
                monto,
                medioPago,
                nroOperacion,
                $"COB-{cxc.Id}"
            );

            // 2. Registrar el detalle de la cobranza
            var cobranza = new CobranzaCliente
            {
                CuentaPorCobrarId = cuentaPorCobrarId,
                CuentaBancariaId = cuentaBancariaId,
                FechaCobro = DateTime.Now,
                MedioPago = medioPago,
                NumeroOperacion = nroOperacion,
                MontoCobrado = monto,
                UsuarioId = usuarioId
            };
            _db.CobranzasCliente.Add(cobranza);

            // 3. Actualizar saldo y estado
            cxc.SaldoPendiente -= monto;
            cxc.Estado = cxc.SaldoPendiente == 0 ? "Cobrado" : "CobradoParcial";

            // 4. Asiento Contable (Bancos 1041 vs Facturas por Cobrar 1212)
            await _contabilidad.RegistrarAsiento(
                modulo: "Cobranza",
                referenciaId: cobranza.Id,
                glosa: $"Cobro de CxC {cxc.Id} vía {medioPago}",
                usuarioId: usuarioId,
                (Cuentas.Bancos, monto, 0m),
                (Cuentas.FacturasPorCobrar, 0m, monto)
            );

            // 5. Guardar todo
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Error al registrar la cobranza: " + ex.Message);
        }
    }

    // ===================== APLICACIÓN DE NOTA DE CRÉDITO =====================

    /// Rebaja el saldo pendiente de la CuentaPorCobrar asociada al comprobante original de la Nota de Crédito.
    /// No llama a SaveChangesAsync (lo hace el DAO que lo invoca).
    public async Task AplicarNotaCredito(int comprobantePagoId, decimal montoNotaCredito)
    {
        var cxc = await _db.CuentasPorCobrar
            .FirstOrDefaultAsync(c => c.ComprobantePagoId == comprobantePagoId);

        if (cxc == null) throw new Exception("No se encontró una Cuenta por Cobrar asociada a este comprobante.");

        cxc.SaldoPendiente -= montoNotaCredito;

        if (cxc.SaldoPendiente < 0) cxc.SaldoPendiente = 0;

        if (cxc.SaldoPendiente == 0) cxc.Estado = "Cobrado";
        else cxc.Estado = "CobradoParcial";
    }
}