namespace AgroERP.Models.Compras;

using Maestros;
using MRP_IA;

public class RequerimientoCompra
{
    public int Id { get; set; }
    public string UsuarioSolicitanteId { get; set; } = null!;   // FK AspNetUsers
    public DateTime FechaSolicitud { get; set; }
    public string Estado { get; set; } = null!;
    public List<RequerimientoDetalle> Detalles { get; set; } = new();
    public List<CotizacionProveedor> Cotizaciones { get; set; } = new();
}

public class RequerimientoDetalle
{
    public int Id { get; set; }
    public int RequerimientoId { get; set; }
    public RequerimientoCompra Requerimiento { get; set; } = null!;
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;
    public int? NecesidadMrpId { get; set; }                    // trazabilidad MRP -> compra
    public NecesidadMrp? NecesidadMrp { get; set; }
    public decimal Cantidad { get; set; }                       // (18,4)
}

public class CotizacionProveedor
{
    public int Id { get; set; }
    public int RequerimientoId { get; set; }
    public RequerimientoCompra Requerimiento { get; set; } = null!;
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public decimal PrecioOfertado { get; set; }                 // (18,2)
    public int DiasEntrega { get; set; }
    public bool EsGanador { get; set; }                         // índice filtrado: 1 por requerimiento
}

public class OrdenCompra
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }                          // (18,2)
    public string Estado { get; set; } = null!;
    public List<OrdenCompraDetalle> Detalles { get; set; } = new();
}

public class OrdenCompraDetalle
{
    public int Id { get; set; }
    public int OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;
    public decimal CantidadPedida { get; set; }                 // (18,4)
    public decimal PrecioUnitario { get; set; }                 // (18,4)
}
