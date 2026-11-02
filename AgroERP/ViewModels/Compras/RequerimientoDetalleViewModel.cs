namespace AgroERP.ViewModels.Compras;

public class RequerimientoDetalleViewModel
{
    public int Id { get; set; }
    public string FechaSolicitud { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public List<ItemRequerimientoViewModel> Articulos { get; set; } = new();
}

public class ItemRequerimientoViewModel
{
    public string CodigoArticulo { get; set; } = null!;
    public string NombreArticulo { get; set; } = null!;
    public decimal Cantidad { get; set; }
}