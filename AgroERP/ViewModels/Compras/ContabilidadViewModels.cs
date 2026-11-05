namespace AgroERP.ViewModels.Compras;

public class AsientoDiarioViewModel
{
    public int Id { get; set; }
    public string Fecha { get; set; } = null!;
    public string Glosa { get; set; } = null!;
    public string Referencia { get; set; } = null!;
    public List<DetalleAsientoViewModel> Detalles { get; set; } = new();
}

public class DetalleAsientoViewModel
{
    public string Cuenta { get; set; } = null!;
    public decimal Debe { get; set; }
    public decimal Haber { get; set; }
}