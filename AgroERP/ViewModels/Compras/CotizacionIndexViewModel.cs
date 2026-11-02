namespace AgroERP.ViewModels.Compras;

public class CotizacionIndexViewModel
{
    public int RequerimientoId { get; set; }
    public string Estado { get; set; } = null!;
    public List<OfertaViewModel> Ofertas { get; set; } = new();
}

public class OfertaViewModel
{
    public int Id { get; set; }
    public string Proveedor { get; set; } = null!;
    public decimal Precio { get; set; }
    public int Dias { get; set; }
    public bool EsGanador { get; set; }
}