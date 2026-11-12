using AgroERP.Models;
using AgroERP.Models.Contabilidad;
using AgroERP.Models.Tesoreria;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Changa. Escribe CuentasPorPagar, PagosProveedor.
public class DAPagos
{
    private readonly ApplicationDbContext _db;
    private readonly DATesoreria _tesoreria;
    private readonly DAContabilidad _contabilidad;

    public DAPagos(ApplicationDbContext db, DATesoreria tesoreria, DAContabilidad contabilidad)
    {
        _db = db;
        _tesoreria = tesoreria;
        _contabilidad = contabilidad;
    }

    /// Contrato llamado por Lavado (alta activo), Méndez (mantenimiento/mejora) y Changa (OC/fletes). No llama SaveChangesAsync.
    public async Task CrearCuentaPorPagar(int proveedorId, int? ordenCompraId, string tipoObligacion, string nroFactura, decimal montoTotal, int diasVencimiento = 30)
    {
        var cuentaPorPagar = new CuentaPorPagar
        {
            ProveedorId = proveedorId,
            OrdenCompraId = ordenCompraId, // null si es mantenimiento o activo sin OC
            TipoObligacion = tipoObligacion,
            NumeroFacturaProveedor = nroFactura, // <-- ¡Corregido!
            MontoTotal = montoTotal,
            SaldoPendiente = montoTotal, // Nace con toda la deuda pendiente
            FechaEmision = DateTime.Now,
            FechaVencimiento = DateTime.Now.AddDays(diasVencimiento),
            Prioridad = "Media",
            Estado = "Pendiente"
        };

        await _db.Set<CuentaPorPagar>().AddAsync(cuentaPorPagar);
    }

    // =========================================================================
    // BANDEJA PRINCIPAL: Lista las deudas para programar y pagar
    // =========================================================================
    public async Task<List<ViewModels.Pagos.CuentaPorPagarItemVM>> ObtenerBandejaPagosAsync()
    {
        return await _db.Set<CuentaPorPagar>()
            .Include(c => c.Proveedor) // Traemos los datos del proveedor
            .Where(c => c.Estado != "Pagado") // No mostramos los que ya se pagaron por completo
            .OrderBy(c => c.FechaVencimiento) // Ordenamos para saber qué se vence primero
            .Select(c => new ViewModels.Pagos.CuentaPorPagarItemVM
            {
                Id = c.Id,
                Proveedor = c.Proveedor.RazonSocial,
                TipoObligacion = c.TipoObligacion,
                NumeroFactura = c.NumeroFacturaProveedor, // El campo que corregimos antes
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                Prioridad = c.Prioridad,
                MontoTotal = c.MontoTotal,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado
            })
            .ToListAsync();
    }

    // =========================================================================
    // TAREA BIZAGI (A02): Seleccionar obligaciones y cambiar estado a Programado
    // =========================================================================
    public async Task ProgramarPagoAsync(int cuentaPorPagarId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta != null && cuenta.Estado == "Pendiente")
        {
            cuenta.Estado = "Programado";
            await _db.SaveChangesAsync();
        }
    }

    // =========================================================================
    // TAREA BIZAGI (A03): Validar datos y autorizar pago
    // =========================================================================
    public async Task AutorizarPagoAsync(int cuentaPorPagarId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta != null && cuenta.Estado == "Programado")
        {
            cuenta.Estado = "Autorizado";
            await _db.SaveChangesAsync();
        }
    }
    // =========================================================================
    // TAREA BIZAGI (A04): Ejecutar el pago y contabilizar (JEFE FINAL)
    // =========================================================================
    public async Task ProcesarPagoAsync(int cuentaPorPagarId, int cuentaBancariaId, string medioPago, string nroOperacion, decimal montoPagado, string usuarioId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta == null) throw new Exception("Cuenta por pagar no encontrada.");

        if (montoPagado > cuenta.SaldoPendiente)
            throw new Exception("El monto a pagar supera el saldo pendiente.");

        // 1. Escribir en tu tabla PagosProveedor
        var pago = new PagoProveedor
        {
            CuentaPorPagarId = cuentaPorPagarId,
            CuentaBancariaId = cuentaBancariaId,
            FechaPago = DateTime.Now,
            MedioPago = medioPago, // "Transferencia", "Deposito" o "POS"
            NumeroOperacion = nroOperacion,
            MontoPagado = montoPagado,
            UsuarioId = usuarioId
        };
        await _db.Set<PagoProveedor>().AddAsync(pago);

        // 2. Rebajar saldo de la deuda y actualizar estado
        cuenta.SaldoPendiente -= montoPagado;
        cuenta.Estado = cuenta.SaldoPendiente == 0 ? "Pagado" : "PagadoParcial";

        // 3. CONTRATO TESORERÍA: Descontar el dinero de la cuenta bancaria (Paso B)
        await _tesoreria.RegistrarMovimientoBancario(cuentaBancariaId, "Egreso", montoPagado, medioPago, nroOperacion, $"PAG-FACT-{cuenta.NumeroFacturaProveedor}");

        // 4. CONTRATO CONTABILIDAD: Asiento Doble Automático (Paso C y D)
        string cuentaDebe = cuenta.TipoObligacion == "ActivoFijo" ? Cuentas.PasivoActivoFijo : Cuentas.FacturasPorPagar;

        await _contabilidad.RegistrarAsiento(
            "Pagos",
            cuentaPorPagarId,
            $"Pago de factura {cuenta.NumeroFacturaProveedor}",
            usuarioId,
            (cuentaDebe, montoPagado, 0m),     // DEBE: 4212 o 4654
            (Cuentas.Bancos, 0m, montoPagado)  // HABER: 1041
        );

        // 5. UN SOLO GUARDADO POR OPERACIÓN (Regla 4)
        await _db.SaveChangesAsync();
    }
}