using System;

namespace AgroERP.ViewModels.Pagos;

public class CuentaPorPagarItemVM
{
    public int Id { get; set; }
    public int? OrdenCompraId { get; set; }
    public string Proveedor { get; set; } = null!;
    public string TipoObligacion { get; set; } = null!;
    public string NumeroFactura { get; set; } = null!; // Agregado
    public DateTime FechaEmision { get; set; }         // Agregado
    public DateTime FechaVencimiento { get; set; }
    public string Prioridad { get; set; } = null!;     // Agregado
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = null!;
}

public class PagoEjecutadoItemVM
{
    public int PagoId { get; set; }
    public int CuentaPorPagarId { get; set; }
    public int? OrdenCompraId { get; set; }
    public string Proveedor { get; set; } = "";
    public string TipoObligacion { get; set; } = "";
    public string NumeroFactura { get; set; } = "";
    public string Banco { get; set; } = "";
    public DateTime FechaPago { get; set; }
    public string MedioPago { get; set; } = "";
    public string NumeroOperacion { get; set; } = "";
    public decimal MontoPagado { get; set; }
}

public class VoucherPagoPdfVM
{
    public int PagoId { get; set; }
    public int CuentaPorPagarId { get; set; }
    public int? OrdenCompraId { get; set; }
    public string Proveedor { get; set; } = "";
    public string ProveedorRuc { get; set; } = "";
    public string TipoObligacion { get; set; } = "";
    public string NumeroFactura { get; set; } = "";
    public string CuentaBancaria { get; set; } = "";
    public DateTime FechaPago { get; set; }
    public string MedioPago { get; set; } = "";
    public string NumeroOperacion { get; set; } = "";
    public decimal MontoTotalObligacion { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal SaldoRestante { get; set; }
    public string CuentaContableDebe { get; set; } = "";
    public string Usuario { get; set; } = "";
}

public class PropuestaPagosPdfVM
{
    public DateTime FechaEmision { get; set; }
    public decimal TotalPendiente { get; set; }
    public decimal TotalProgramado { get; set; }
    public decimal TotalAutorizado { get; set; }
    public decimal SaldoDisponibleBancos { get; set; }
    public List<CuentaPorPagarItemVM> Obligaciones { get; set; } = new();
}

public class OrdenCompraPendienteCxPVM
{
    public int OrdenCompraId { get; set; }
    public int ProveedorId { get; set; }
    public string Proveedor { get; set; } = null!;
    public string EstadoOrden { get; set; } = null!;
    public decimal MontoConforme { get; set; }
}

public class RegistrarFacturaOcVM
{
    public int OrdenCompraId { get; set; }
    public string NumeroFactura { get; set; } = null!;
    public string Prioridad { get; set; } = "Media";
    public int DiasVencimiento { get; set; } = 30;
}