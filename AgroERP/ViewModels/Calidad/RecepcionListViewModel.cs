namespace AgroERP.ViewModels.Calidad;

public class RecepcionListViewModel
{
    public int OrdenCompraId { get; set; }
    public string Proveedor { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
}

public class RecepcionHistorialItemVM
{
    public int RecepcionId { get; set; }
    public int OrdenCompraId { get; set; }
    public string GuiaProveedor { get; set; } = "";
    public string Proveedor { get; set; } = "";
    public DateTime FechaRecepcion { get; set; }
    public string Estado { get; set; } = "";
}

public class DetalleReporteCalidadVM
{
    public string Articulo { get; set; } = "";
    public string NumeroLote { get; set; } = "";
    public DateTime FechaVencimiento { get; set; }
    public decimal CantidadPedida { get; set; }
    public decimal CantidadGuia { get; set; }
    public decimal CantidadRecibida { get; set; }
    public string EstadoCalidad { get; set; } = "";
    public string? MotivoRechazo { get; set; }
    public string? Inspector { get; set; }
}

public class ReporteRecepcionCalidadPdfVM
{
    public int RecepcionId { get; set; }
    public int OrdenCompraId { get; set; }
    public string Proveedor { get; set; } = "";
    public string ProveedorRuc { get; set; } = "";
    public string GuiaProveedor { get; set; } = "";
    public DateTime FechaRecepcion { get; set; }
    public string Receptor { get; set; } = "";
    public string EstadoRecepcion { get; set; } = "";
    public string EstadoOrden { get; set; } = "";
    public List<DetalleReporteCalidadVM> Detalles { get; set; } = new();
}