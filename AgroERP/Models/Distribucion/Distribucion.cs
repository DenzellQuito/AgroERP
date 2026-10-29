namespace AgroERP.Models.Distribucion;

using Maestros;
using Calidad_WMS;

public class PedidoVenta
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaPedido { get; set; }
    public DateTime FechaPromesaEntrega { get; set; }    // OTIF
    public DateTime? FechaEntregaReal { get; set; }      // OTIF
    public decimal Total { get; set; }                   // (18,2)
    public string EstadoLogistico { get; set; } = null!;
    public List<PedidoVentaDetalle> Detalles { get; set; } = new();
}

public class PedidoVentaDetalle
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public PedidoVenta Pedido { get; set; } = null!;
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;
    public decimal Cantidad { get; set; }                // (18,4)
    public decimal PrecioUnitario { get; set; }          // (18,4)
}

public class OrdenPicking
{
    public int Id { get; set; }
    public int PedidoVentaId { get; set; }
    public PedidoVenta PedidoVenta { get; set; } = null!;
    public string UsuarioOperarioId { get; set; } = null!;      // FK AspNetUsers
    public List<OrdenPickingDetalle> Detalles { get; set; } = new();
}

public class OrdenPickingDetalle
{
    public int Id { get; set; }
    public int OrdenPickingId { get; set; }
    public OrdenPicking OrdenPicking { get; set; } = null!;
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public decimal CantidadRecolectada { get; set; }     // (18,4)
}

public class GuiaRemisionRemitente
{
    public int Id { get; set; }
    public int PedidoVentaId { get; set; }
    public PedidoVenta PedidoVenta { get; set; } = null!;
    public string PlacaVehiculo { get; set; } = null!;
    public string Chofer { get; set; } = null!;
    public string MotivoTraslado { get; set; } = null!;
    public string Estado { get; set; } = null!;
}

public class ComprobantePago
{
    public int Id { get; set; }
    public int PedidoVentaId { get; set; }
    public PedidoVenta PedidoVenta { get; set; } = null!;
    public string TipoCPE { get; set; } = null!;
    public decimal MontoTotal { get; set; }              // (18,2)
}

public class DevolucionCliente
{
    public int Id { get; set; }
    public int PedidoVentaId { get; set; }
    public PedidoVenta PedidoVenta { get; set; } = null!;
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public decimal CantidadRechazada { get; set; }       // (18,4)
    public string Motivo { get; set; } = null!;
}

public class NotaCredito
{
    public int Id { get; set; }
    public int DevolucionId { get; set; }
    public DevolucionCliente Devolucion { get; set; } = null!;
    public int ComprobantePagoId { get; set; }
    public ComprobantePago ComprobantePago { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }                   // (18,2)
}
