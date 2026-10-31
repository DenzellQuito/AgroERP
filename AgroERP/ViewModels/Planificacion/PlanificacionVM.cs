namespace AgroERP.ViewModels.Planificacion;

public class CampaniaItemVM
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public decimal HectareasActivas { get; set; }
    public int MesProyectado { get; set; }
    public string Estado { get; set; } = "";
}

public class NecesidadItemVM
{
    public int Id { get; set; }
    public string Articulo { get; set; } = "";
    public decimal CantidadNetaSugerida { get; set; }
    public string Estado { get; set; } = "";
}

public class ResultadoMrpVM
{
    public int PronosticoId { get; set; }
    public string Campania { get; set; } = "";
    public int VolumenCosechaEstimado { get; set; }
    public DateTime FechaCalculo { get; set; }
    public List<NecesidadItemVM> Necesidades { get; set; } = new();
}
