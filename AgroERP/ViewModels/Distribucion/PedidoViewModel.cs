using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Distribucion;

public class PedidoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Seleccione un cliente")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Seleccione un artículo")]
    public int ArticuloId { get; set; }

    [Range(0.01, 10000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }

    [Required]
    public DateTime FechaPromesaEntrega { get; set; } = DateTime.Now.AddDays(2);

    public decimal PrecioUnitario { get; set; }
}

public class PedidoListViewModel
{
    public int Id { get; set; }
    public string Cliente { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = null!;

    public bool TieneStockReservado { get; set; }
}