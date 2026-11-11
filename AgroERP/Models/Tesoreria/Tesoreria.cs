namespace AgroERP.Models.Tesoreria;

using Maestros;
using Compras;
using Distribucion;

// ===================== QUITO: TESORERÍA Y BANCOS =====================
public class CuentaBancaria
{
    public int Id { get; set; }
    public string Banco { get; set; } = null!;               // BCP, BBVA, Interbank...
    public string NumeroCuenta { get; set; } = null!;        // único
    public string Moneda { get; set; } = "PEN";              // PEN
    public string Titular { get; set; } = null!;             // Mi Paraíso Gramadal S.A.C.
    public decimal SaldoContable { get; set; }               // (18,2)
    public decimal SaldoDisponible { get; set; }             // (18,2)
    public bool Activa { get; set; } = true;
    public List<MovimientoBancario> Movimientos { get; set; } = new();
}

public class MovimientoBancario
{
    public int Id { get; set; }
    public int CuentaBancariaId { get; set; }
    public CuentaBancaria CuentaBancaria { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = null!;      // Ingreso | Egreso
    public string MedioPago { get; set; } = null!;           // Transferencia | Deposito | POS
    public string NumeroOperacion { get; set; } = null!;
    public decimal Monto { get; set; }                       // (18,2)
    public string? ReferenciaDocumento { get; set; }         // "COB-10", "PAG-05", "COM-BCO"
    public bool Conciliado { get; set; }
}

public class ConciliacionBancaria
{
    public int Id { get; set; }
    public int CuentaBancariaId { get; set; }
    public CuentaBancaria CuentaBancaria { get; set; } = null!;
    public DateTime FechaCorte { get; set; }
    public decimal SaldoExtractoBanco { get; set; }          // (18,2)
    public decimal DepositosEnTransito { get; set; }         // (18,2)
    public decimal PagosPendientes { get; set; }             // (18,2)
    public decimal ComisionesBancarias { get; set; }         // (18,2)
    public decimal SaldoContableAjustado { get; set; }       // (18,2)
    public string Estado { get; set; } = null!;              // EnProceso | Cerrada
    public string UsuarioId { get; set; } = null!;           // FK AspNetUsers
}

// ===================== DÍAZ: CUENTAS POR COBRAR Y COBRANZAS =====================
public class CuentaPorCobrar
{
    public int Id { get; set; }
    public int ComprobantePagoId { get; set; }
    public ComprobantePago ComprobantePago { get; set; } = null!;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoTotal { get; set; }                  // (18,2)
    public decimal SaldoPendiente { get; set; }              // (18,2)
    public string Estado { get; set; } = null!;              // Pendiente | CobradoParcial | Cobrado
    public List<CobranzaCliente> Cobranzas { get; set; } = new();
}

public class CobranzaCliente
{
    public int Id { get; set; }
    public int CuentaPorCobrarId { get; set; }
    public CuentaPorCobrar CuentaPorCobrar { get; set; } = null!;
    public int CuentaBancariaId { get; set; }
    public CuentaBancaria CuentaBancaria { get; set; } = null!;
    public DateTime FechaCobro { get; set; }
    public string MedioPago { get; set; } = null!;           // Transferencia | Deposito | POS
    public string NumeroOperacion { get; set; } = null!;
    public decimal MontoCobrado { get; set; }                // (18,2)
    public string UsuarioId { get; set; } = null!;           // FK AspNetUsers
}

// ===================== CHANGA: CUENTAS POR PAGAR Y PAGOS =====================
public class CuentaPorPagar
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public int? OrdenCompraId { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }
    public string TipoObligacion { get; set; } = null!;      // Insumos | Flete | Mantenimiento | ActivoFijo
    public string NumeroFacturaProveedor { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Prioridad { get; set; } = "Media";         // Alta | Media | Baja
    public decimal MontoTotal { get; set; }                  // (18,2)
    public decimal SaldoPendiente { get; set; }              // (18,2)
    public string Estado { get; set; } = null!;              // Pendiente | Programado | Autorizado | PagadoParcial | Pagado
    public List<PagoProveedor> Pagos { get; set; } = new();
}

public class PagoProveedor
{
    public int Id { get; set; }
    public int CuentaPorPagarId { get; set; }
    public CuentaPorPagar CuentaPorPagar { get; set; } = null!;
    public int CuentaBancariaId { get; set; }
    public CuentaBancaria CuentaBancaria { get; set; } = null!;
    public DateTime FechaPago { get; set; }
    public string MedioPago { get; set; } = null!;           // Transferencia | Deposito | POS
    public string NumeroOperacion { get; set; } = null!;
    public decimal MontoPagado { get; set; }                 // (18,2)
    public string UsuarioId { get; set; } = null!;           // FK AspNetUsers
}