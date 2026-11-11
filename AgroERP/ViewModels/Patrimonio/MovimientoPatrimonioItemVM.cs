namespace AgroERP.ViewModels.Patrimonio;

public class MovimientoPatrimonioItemVM
{
    public int Id { get; set; }
    public string CodigoActivo { get; set; } = null!;
    public string TipoMovimiento { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string Detalle { get; set; } = null!;
}