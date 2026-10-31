using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroERP.ViewModels.BI;

public class KpiResultadoVM
{
    public string Nombre { get; set; } = "";
    public decimal? Valor { get; set; }
    public decimal Meta { get; set; }
    public decimal Tolerancia { get; set; }
    public string Unidad { get; set; } = "";
    public bool MayorEsMejor { get; set; }
    public string Semaforo { get; set; } = "SinDatos";   // Verde | Amarillo | Rojo | SinDatos
}

public class PuntoMesVM
{
    public string Mes { get; set; } = "";
    public decimal? Valor { get; set; }
}

public class ParetoItemVM
{
    public string Articulo { get; set; } = "";
    public decimal Total { get; set; }
    public decimal AcumuladoPct { get; set; }
}

public class DashboardVM
{
    public int Dias { get; set; } = 90;
    public List<KpiResultadoVM> Kpis { get; set; } = new();
    public List<PuntoMesVM> OtifMensual { get; set; } = new();
    public List<PuntoMesVM> CostoMensual { get; set; } = new();
    public List<ParetoItemVM> Pareto { get; set; } = new();
}

public class IndicadorVM
{
    public int Id { get; set; }
    [Required, Display(Name = "Indicador")] public string NombreMetrica { get; set; } = "";
    [Range(0, 1000000), Display(Name = "Meta objetivo")] public decimal MetaObjetivo { get; set; }
    [Range(0, 1000000), Display(Name = "Tolerancia de alarma")] public decimal ToleranciaAlarma { get; set; }
    public List<SelectListItem> Nombres { get; set; } = new();
}
