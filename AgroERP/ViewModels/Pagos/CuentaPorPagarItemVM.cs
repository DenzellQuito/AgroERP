namespace AgroERP.ViewModels.Pagos;

public class CuentaPorPagarItemVM
{
    public int Id { get; set; }
    public string Proveedor { get; set; } = null!;
    public string TipoObligacion { get; set; } = null!;
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = null!;
}