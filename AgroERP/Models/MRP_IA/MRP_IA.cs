namespace AgroERP.Models.MRP_IA;

using Maestros;
public class CampaniaAgricola
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public decimal HectareasActivas { get; set; }        // (10,2) variable X para Python
    public int MesProyectado { get; set; }               // 1..12 estacionalidad
}

public class PronosticoDemanda
{
    public int Id { get; set; }
    public int CampaniaId { get; set; }
    public CampaniaAgricola Campania { get; set; } = null!;
    public DateTime FechaCalculo { get; set; }
    public int VolumenCosechaEstimado { get; set; }      // jabas, variable Y (XGBoost)
}

// Tabla: MRP_IA.RecetaAgricola_BOM
public class RecetaAgricolaBOM
{
    public int Id { get; set; }
    public int ArticuloFinalId { get; set; }             // fresa
    public Articulo ArticuloFinal { get; set; } = null!;
    public int ArticuloInsumoId { get; set; }            // agroquímico / empaque
    public Articulo ArticuloInsumo { get; set; } = null!;
    public int MesAplicacion { get; set; }               // 1..12
    public decimal CantidadPorJaba { get; set; }         // (10,4)
}

// Tabla: MRP_IA.Necesidad_MRP
public class NecesidadMrp
{
    public int Id { get; set; }
    public int PronosticoId { get; set; }
    public PronosticoDemanda Pronostico { get; set; } = null!;
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;
    public decimal CantidadNetaSugerida { get; set; }    // (18,4)
    public string Estado { get; set; } = null!;
}
