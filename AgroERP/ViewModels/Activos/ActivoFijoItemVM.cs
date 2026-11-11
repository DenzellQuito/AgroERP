namespace AgroERP.ViewModels.Activos;

public class ActivoFijoItemVM
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Categoria { get; set; } = null!;
    public decimal CostoAdquisicion { get; set; }
    public decimal ValorNetoLibros { get; set; }
    public string Estado { get; set; } = null!;
}