using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Calidad;

public class DetalleRecepcionViewModel
{
    public int ArticuloId { get; set; }
    public string NombreArticulo { get; set; } = null!;
    public decimal CantidadPedida { get; set; }

    [Required(ErrorMessage = "La cantidad de guía es obligatoria")]
    public decimal CantidadGuia { get; set; }

    [Required(ErrorMessage = "La cantidad contada es obligatoria")]
    public decimal CantidadRecibida { get; set; }

    [Required(ErrorMessage = "El número de lote es obligatorio")]
    public string NumeroLote { get; set; } = null!;

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
    public DateTime FechaVencimiento { get; set; }
}

public class RegistrarRecepcionViewModel
{
    public int OrdenCompraId { get; set; }

    [Required(ErrorMessage = "El número de guía es obligatorio")]
    public string GuiaProveedor { get; set; } = null!;

    public List<DetalleRecepcionViewModel> Detalles { get; set; } = new();
}