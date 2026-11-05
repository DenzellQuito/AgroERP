namespace AgroERP.ViewModels.Compras;
public class RequerimientoIndexViewModel
{
    public int Id { get; set; }
    public string FechaSolicitud { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public int CantidadArticulos { get; set; }
}