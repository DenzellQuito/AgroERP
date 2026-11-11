using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Tesoreria;

// ===================== PANTALLA PRINCIPAL (DASHBOARD TESORERÍA) =====================
public class TesoreriaDashboardVM
{
    // Flujo de Caja del mes (Saldo inicial + Cobros - Pagos = Saldo final)
    public decimal SaldoInicialCaja { get; set; }
    public decimal CobrosMes { get; set; }
    public decimal PagosMes { get; set; }
    public decimal SaldoFinalDisponible { get; set; }
    public decimal SaldoFinalContable { get; set; }

    // Posición de Liquidez Proyectada (cruzada con CxC de Díaz y CxP de Changa)
    public decimal TotalPorCobrarPendiente { get; set; }
    public decimal TotalPorPagarPendiente { get; set; }
    public decimal LiquidezProyectada { get; set; }

    public List<CuentaBancariaItemVM> Cuentas { get; set; } = new();
    public List<MovimientoBancarioItemVM> UltimosMovimientos { get; set; } = new();
    public List<ConciliacionItemVM> UltimasConciliaciones { get; set; } = new();
}

public class CuentaBancariaItemVM
{
    public int Id { get; set; }
    public string Banco { get; set; } = null!;
    public string NumeroCuenta { get; set; } = null!;
    public string Moneda { get; set; } = "PEN";
    public string Titular { get; set; } = null!;
    public decimal SaldoContable { get; set; }
    public decimal SaldoDisponible { get; set; }
    public bool Activa { get; set; }
    public int MovimientosSinConciliar { get; set; }
}

public class CrearCuentaBancariaVM
{
    [Required(ErrorMessage = "El nombre del banco es obligatorio.")]
    public string Banco { get; set; } = null!;

    [Required(ErrorMessage = "El número de cuenta es obligatorio.")]
    public string NumeroCuenta { get; set; } = null!;

    public string Moneda { get; set; } = "PEN";

    [Required(ErrorMessage = "El titular es obligatorio.")]
    public string Titular { get; set; } = "Mi Paraíso Gramadal S.A.C.";

    [Range(0, 999999999, ErrorMessage = "El saldo inicial no puede ser negativo.")]
    public decimal SaldoInicial { get; set; }
}

public class RegistrarMovimientoManualVM
{
    [Required]
    public int CuentaBancariaId { get; set; }

    [Required]
    public string TipoMovimiento { get; set; } = "Ingreso"; // Ingreso | Egreso

    [Required]
    public string MedioPago { get; set; } = "Transferencia"; // Transferencia | Deposito | POS

    [Required(ErrorMessage = "Ingrese el número de operación.")]
    public string NumeroOperacion { get; set; } = null!;

    [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a cero.")]
    public decimal Monto { get; set; }

    public string? ReferenciaDocumento { get; set; }
}

public class MovimientoBancarioItemVM
{
    public int Id { get; set; }
    public string CuentaBanco { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = null!;
    public string MedioPago { get; set; } = null!;
    public string NumeroOperacion { get; set; } = null!;
    public decimal Monto { get; set; }
    public string? ReferenciaDocumento { get; set; }
    public bool Conciliado { get; set; }
}

// ===================== PANTALLA DE CONCILIACIÓN BANCARIA =====================
public class ConciliarCuentaVM
{
    public int CuentaBancariaId { get; set; }
    public string Banco { get; set; } = null!;
    public string NumeroCuenta { get; set; } = null!;
    public string Moneda { get; set; } = "PEN";
    public decimal SaldoContableActual { get; set; }
    public decimal SaldoDisponibleActual { get; set; }

    // Datos de entrada del extracto bancario
    public DateTime FechaCorte { get; set; } = DateTime.Today;
    public decimal SaldoExtractoBanco { get; set; }
    public decimal DepositosEnTransito { get; set; }
    public decimal PagosPendientes { get; set; }
    public decimal ComisionesBancarias { get; set; }

    // Movimientos seleccionados para marcar como conciliados en este corte
    public List<int> MovimientosSeleccionadosIds { get; set; } = new();

    // Listas informativas para la vista
    public List<MovimientoBancarioItemVM> MovimientosPendientes { get; set; } = new();
    public List<MovimientoBancarioItemVM> MovimientosConciliados { get; set; } = new();
    public List<ConciliacionItemVM> HistorialConciliaciones { get; set; } = new();
}

public class ConciliacionItemVM
{
    public int Id { get; set; }
    public string CuentaBanco { get; set; } = null!;
    public DateTime FechaCorte { get; set; }
    public decimal SaldoExtractoBanco { get; set; }
    public decimal DepositosEnTransito { get; set; }
    public decimal PagosPendientes { get; set; }
    public decimal ComisionesBancarias { get; set; }
    public decimal SaldoContableAjustado { get; set; }
    public string Estado { get; set; } = null!;
    public string UsuarioId { get; set; } = null!;
}