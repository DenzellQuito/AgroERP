namespace AgroERP.ViewModels.Compras;

public class OrdenCompraViewModel
{
    public int Id { get; set; }
    public string Proveedor { get; set; } = null!;
    public string FechaEmision { get; set; } = null!;
    public decimal Total { get; set; }
    public string Estado { get; set; } = null!;
}