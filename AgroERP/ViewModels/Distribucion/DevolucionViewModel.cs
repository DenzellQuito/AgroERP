using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroERP.ViewModels.Distribucion;

public class DevolucionViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un pedido")]
    public int PedidoId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un artículo")]
    public int ArticuloId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un lote")]
    public int LoteId { get; set; }

    [Range(0.01, 10000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal CantidadRechazada { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio")]
    public string Motivo { get; set; } = null!;

    // Listas para llenar los combos en la vista
    public IEnumerable<SelectListItem>? PedidosDisponibles { get; set; }
    public IEnumerable<SelectListItem>? ArticulosDisponibles { get; set; }
    public IEnumerable<SelectListItem>? LotesDisponibles { get; set; }
}