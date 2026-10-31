using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroERP.ViewModels.Planificacion;

public class ArticuloVM
{
    public int Id { get; set; }
    [Required, StringLength(30)] public string Codigo { get; set; } = "";
    [Required, StringLength(150)] public string Nombre { get; set; } = "";
    [Required] public string Tipo { get; set; } = "Insumo";          // Fresa | Insumo | Empaque
    [Required, StringLength(20), Display(Name = "Unidad de medida")] public string UnidadMedida { get; set; } = "";
    [Range(0, 999999999), Display(Name = "Stock de seguridad")] public decimal StockSeguridad { get; set; }
    [Range(0, 999999999), Display(Name = "Lote económico")] public decimal LoteEconomico { get; set; }
}

public class CampaniaVM
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required] public string Estado { get; set; } = "Planificada";   // Planificada | Activa | Cerrada
    [Range(0.01, 100000, ErrorMessage = "Debe ser mayor que 0")]
    [Display(Name = "Hectáreas activas")] public decimal HectareasActivas { get; set; }
    [Range(1, 12, ErrorMessage = "Mes entre 1 y 12")]
    [Display(Name = "Mes proyectado")] public int MesProyectado { get; set; } = 1;
}

public class RecetaVM
{
    public int Id { get; set; }
    [Display(Name = "Producto final (fresa)")] public int ArticuloFinalId { get; set; }
    [Display(Name = "Insumo / empaque")] public int ArticuloInsumoId { get; set; }
    [Range(1, 12, ErrorMessage = "Mes entre 1 y 12")]
    [Display(Name = "Mes de aplicación")] public int MesAplicacion { get; set; } = 1;
    [Range(0.0001, 100000, ErrorMessage = "Debe ser mayor que 0")]
    [Display(Name = "Cantidad por jaba")] public decimal CantidadPorJaba { get; set; }

    // Solo lectura / apoyo de pantalla
    public string? ArticuloFinal { get; set; }
    public string? ArticuloInsumo { get; set; }
    public List<SelectListItem> Articulos { get; set; } = new();
}
