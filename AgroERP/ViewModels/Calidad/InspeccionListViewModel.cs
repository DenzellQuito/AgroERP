namespace AgroERP.ViewModels.Calidad;

public class InspeccionListViewModel
{
    public int DetalleId { get; set; }
    public string NumeroLote { get; set; } = null!;
    public string Articulo { get; set; } = null!;
    public decimal CantidadRecibida { get; set; }
    public string Proveedor { get; set; } = null!;
    public string GuiaProveedor { get; set; } = null!; // Nueva propiedad para la trazabilidad
}