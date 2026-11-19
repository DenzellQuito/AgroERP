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

public class ActaItemReqVM
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Unidad { get; set; } = "";
    public decimal Cantidad { get; set; }
}

public class ActaOfertaVM
{
    public string Proveedor { get; set; } = "";
    public string Ruc { get; set; } = "";
    public int Confiabilidad { get; set; }
    public decimal Precio { get; set; }
    public int DiasEntrega { get; set; }
    public bool EsGanador { get; set; }
}

public class ActaAdjudicacionPdfVM
{
    public int RequerimientoId { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public string Estado { get; set; } = "";
    public string Solicitante { get; set; } = "";
    public List<ActaItemReqVM> Articulos { get; set; } = new();
    public List<ActaOfertaVM> Ofertas { get; set; } = new();
}