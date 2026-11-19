namespace AgroERP.ViewModels.Activos;

public class ActivoFijoItemVM
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Categoria { get; set; } = null!;
    public decimal CostoAdquisicion { get; set; }
    public decimal ValorNetoLibros { get; set; }
    public string Estado { get; set; } = null!;
}

public class DepreciacionHistorialVM
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public DateTime FechaRegistro { get; set; }
    public decimal MontoDepreciado { get; set; }
    public decimal DepreciacionAcumuladaAlCierre { get; set; }
    public decimal ValorNetoResultante { get; set; }
}

public class TransferenciaHistorialActivoVM
{
    public DateTime Fecha { get; set; }
    public string Origen { get; set; } = "";
    public string Destino { get; set; } = "";
    public string ResponsableAnterior { get; set; } = "";
    public string ResponsableNuevo { get; set; } = "";
    public string Motivo { get; set; } = "";
}

public class MantenimientoHistorialActivoVM
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Costo { get; set; }
}

public class FichaActivoPdfVM
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string MarcaSerie { get; set; } = "";
    public string Proveedor { get; set; } = "-";
    public DateTime FechaAdquisicion { get; set; }
    public string UbicacionActual { get; set; } = "";
    public string CustodioActual { get; set; } = "";
    public decimal CostoAdquisicion { get; set; }
    public decimal ValorResidual { get; set; }
    public int VidaUtilMeses { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public decimal ValorNetoLibros { get; set; }
    public string Estado { get; set; } = "";
    public List<DepreciacionHistorialVM> Depreciaciones { get; set; } = new();
    public List<TransferenciaHistorialActivoVM> Transferencias { get; set; } = new();
    public List<MantenimientoHistorialActivoVM> Mantenimientos { get; set; } = new();
}