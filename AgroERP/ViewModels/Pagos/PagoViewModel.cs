using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Pagos;

public class PagoViewModel
{
    public int CuentaPorPagarId { get; set; }
    public string Proveedor { get; set; } = null!;
    public string NumeroFactura { get; set; } = null!;
    public decimal SaldoPendiente { get; set; }

    [Required(ErrorMessage = "Seleccione una cuenta bancaria")]
    public int CuentaBancariaId { get; set; }

    [Required]
    public string MedioPago { get; set; } = "Transferencia";

    [Required(ErrorMessage = "Ingrese el Nro de Operación")]
    public string NumeroOperacion { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoAPagar { get; set; }
}