namespace AgroERP.ViewModels.Calidad;

public class RecepcionListViewModel
{
    public int OrdenCompraId { get; set; }
    public string Proveedor { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
}