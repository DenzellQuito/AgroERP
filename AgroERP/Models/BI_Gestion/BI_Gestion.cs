namespace AgroERP.Models.BI_Gestion;

public class IndicadorKPI
{
    public int Id { get; set; }
    public string NombreMetrica { get; set; } = null!;   // OTIF, Costo Logístico...
    public decimal MetaObjetivo { get; set; }            // (10,2)
    public decimal ToleranciaAlarma { get; set; }        // (10,2) semáforo amarillo/rojo
}

public class PlanAccionMejora
{
    public int Id { get; set; }
    public int IndicadorId { get; set; }
    public IndicadorKPI Indicador { get; set; } = null!;
    public string AnalisisCausaRaiz { get; set; } = null!;
    public string? ResponsableId { get; set; }           // FK AspNetUsers
    public string Estado { get; set; } = null!;
}

public class GastoLogistico
{
    public int Id { get; set; }
    public string Concepto { get; set; } = null!;        // Flete, Estiba...
    public decimal Monto { get; set; }                   // (18,2)
    public DateTime FechaRegistro { get; set; }
}
