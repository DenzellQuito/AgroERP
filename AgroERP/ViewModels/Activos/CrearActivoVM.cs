using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Activos;

public class CrearActivoVM
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    public string Codigo { get; set; } = null!; // Ej: AF-001

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "Debe seleccionar una categoría.")]
    public string Categoria { get; set; } = null!; // MaquinariaAgricola, EquiposComputo, etc.

    [Required(ErrorMessage = "La marca/serie es obligatoria.")]
    public string MarcaSerie { get; set; } = null!;

    public int? ProveedorId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar la ubicación inicial.")]
    public int ZonaUbicacionId { get; set; }

    [Required(ErrorMessage = "Debe asignar un custodio responsable.")]
    public string UsuarioResponsableId { get; set; } = null!;

    [Required(ErrorMessage = "El costo de adquisición es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0.")]
    public decimal CostoAdquisicion { get; set; }

    [Required(ErrorMessage = "El valor residual es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "No puede ser negativo.")]
    public decimal ValorResidual { get; set; }

    [Required(ErrorMessage = "La vida útil es obligatoria.")]
    [Range(1, 1200, ErrorMessage = "Vida útil no válida.")]
    public int VidaUtilMeses { get; set; }
}