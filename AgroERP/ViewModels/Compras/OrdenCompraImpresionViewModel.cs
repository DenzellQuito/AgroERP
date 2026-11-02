namespace AgroERP.ViewModels.Compras;

public class OrdenCompraImpresionViewModel
{
    public int Id { get; set; }
    public string FechaEmision { get; set; } = null!;
    public string ProveedorRuc { get; set; } = null!;
    public string ProveedorRazonSocial { get; set; } = null!;
    public decimal Total { get; set; }
    public List<ItemImpresionViewModel> Detalles { get; set; } = new();
}

public class ItemImpresionViewModel
{
    public string Codigo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}