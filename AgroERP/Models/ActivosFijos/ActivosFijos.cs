namespace AgroERP.Models.ActivosFijos;

using Maestros;
using Calidad_WMS;

// ===================== LAVADO: ALTA, CAPITALIZACIÓN Y DEPRECIACIÓN =====================
public class ActivoFijo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;              // único: "AF-001"
    public string Nombre { get; set; } = null!;              // "Tractor Agrícola John Deere"
    public string Categoria { get; set; } = null!;           // MaquinariaAgricola | EquiposComputo | InfraestructuraRiego | Vehiculos
    public string MarcaSerie { get; set; } = null!;
    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public DateTime FechaAdquisicion { get; set; }
    public int ZonaUbicacionId { get; set; }
    public ZonaUbicacion ZonaUbicacion { get; set; } = null!;
    public string UsuarioResponsableId { get; set; } = null!; // Custodio (FK AspNetUsers)
    public decimal CostoAdquisicion { get; set; }            // (18,2)
    public decimal ValorResidual { get; set; }               // (18,2)
    public int VidaUtilMeses { get; set; }                   // ej. 120 meses (10 años)
    public decimal DepreciacionAcumulada { get; set; }       // (18,2)
    public decimal ValorNetoLibros { get; set; }             // (18,2)
    public string Estado { get; set; } = null!;              // EnProceso | Activo | DadoDeBaja
    public List<DepreciacionActivo> Depreciaciones { get; set; } = new();
}

public class DepreciacionActivo
{
    public int Id { get; set; }
    public int ActivoFijoId { get; set; }
    public ActivoFijo ActivoFijo { get; set; } = null!;
    public int Anio { get; set; }
    public int Mes { get; set; }                             // 1..12
    public DateTime FechaRegistro { get; set; }
    public decimal MontoDepreciado { get; set; }             // (18,2)
    public decimal DepreciacionAcumuladaAlCierre { get; set; } // (18,2)
    public decimal ValorNetoResultante { get; set; }         // (18,2)
}

// ===================== MÉNDEZ: MANTENIMIENTO, TRANSFERENCIAS Y BAJA =====================
public class TransferenciaActivo
{
    public int Id { get; set; }
    public int ActivoFijoId { get; set; }
    public ActivoFijo ActivoFijo { get; set; } = null!;
    public DateTime FechaTransferencia { get; set; }
    public int ZonaOrigenId { get; set; }
    public ZonaUbicacion ZonaOrigen { get; set; } = null!;
    public int ZonaDestinoId { get; set; }
    public ZonaUbicacion ZonaDestino { get; set; } = null!;
    public string ResponsableAnteriorId { get; set; } = null!;
    public string ResponsableNuevoId { get; set; } = null!;
    public string Motivo { get; set; } = null!;
}

public class MantenimientoActivo
{
    public int Id { get; set; }
    public int ActivoFijoId { get; set; }
    public ActivoFijo ActivoFijo { get; set; } = null!;
    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = null!;                // Preventivo | Correctivo | MejoraCapitalizable
    public string Descripcion { get; set; } = null!;
    public decimal Costo { get; set; }                       // (18,2)
    public string UsuarioId { get; set; } = null!;
}

public class BajaActivo
{
    public int Id { get; set; }
    public int ActivoFijoId { get; set; }
    public ActivoFijo ActivoFijo { get; set; } = null!;
    public DateTime FechaBaja { get; set; }
    public string MotivoBaja { get; set; } = null!;          // Desuso | Retiro | Siniestro
    public decimal CostoHistorico { get; set; }              // (18,2)
    public decimal DepreciacionAcumuladaAlRetiro { get; set; } // (18,2)
    public decimal ValorNetoEnBaja { get; set; }             // (18,2)
    public string UsuarioId { get; set; } = null!;
}