namespace AgroERP.ViewModels.Tesoreria;

public class CuentaBancariaItemVM
{
    public int Id { get; set; }
    public string Banco { get; set; } = null!;
    public string NumeroCuenta { get; set; } = null!;
    public string Moneda { get; set; } = "PEN";
    public decimal SaldoContable { get; set; }
    public decimal SaldoDisponible { get; set; }
}