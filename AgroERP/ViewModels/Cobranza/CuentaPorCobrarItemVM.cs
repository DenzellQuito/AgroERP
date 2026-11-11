namespace AgroERP.ViewModels.Cobranzas;

public class CuentaPorCobrarItemVM
{
    public int Id { get; set; }
    public string Cliente { get; set; } = null!;
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = null!;
}