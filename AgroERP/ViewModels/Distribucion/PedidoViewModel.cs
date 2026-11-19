using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Distribucion;

public class PedidoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Seleccione un cliente")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Seleccione un artículo")]
    public int ArticuloId { get; set; }

    [Range(0.01, 10000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }

    [Required]
    public DateTime FechaPromesaEntrega { get; set; } = DateTime.Now.AddDays(2);

    public decimal PrecioUnitario { get; set; }
}

public class PedidoListViewModel
{
    public int Id { get; set; }
    public string Cliente { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = null!;

    public bool TieneStockReservado { get; set; }
    public bool TieneFactura { get; set; }
    public int? OrdenPickingId { get; set; }
    public int? GuiaRemisionId { get; set; }
    public int? ComprobantePagoId { get; set; }
    public int? UltimaNotaCreditoId { get; set; }
}

public class ItemPickingPdfVM
{
    public string Articulo { get; set; } = "";
    public string Unidad { get; set; } = "";
    public string NumeroLote { get; set; } = "";
    public DateTime FechaVencimiento { get; set; }
    public string AlmacenZona { get; set; } = "";
    public decimal Cantidad { get; set; }
}

public class HojaPickingPdfVM
{
    public int OrdenPickingId { get; set; }
    public int PedidoVentaId { get; set; }
    public string Cliente { get; set; } = "";
    public string ClienteRuc { get; set; } = "";
    public DateTime FechaPedido { get; set; }
    public DateTime FechaPromesaEntrega { get; set; }
    public string EstadoPedido { get; set; } = "";
    public string Operario { get; set; } = "";
    public List<ItemPickingPdfVM> Items { get; set; } = new();
}

public class ItemGuiaPdfVM
{
    public string CodigoArticulo { get; set; } = "";
    public string Articulo { get; set; } = "";
    public string Unidad { get; set; } = "";
    public decimal Cantidad { get; set; }
    public string Lotes { get; set; } = "";
}

public class GuiaRemisionPdfVM
{
    public int GuiaId { get; set; }
    public int PedidoVentaId { get; set; }
    public string EstadoGuia { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string ClienteRuc { get; set; } = "";
    public DateTime FechaPedido { get; set; }
    public DateTime FechaPromesaEntrega { get; set; }
    public string PlacaVehiculo { get; set; } = "";
    public string Chofer { get; set; } = "";
    public string MotivoTraslado { get; set; } = "";
    public List<ItemGuiaPdfVM> Items { get; set; } = new();
}

public class ItemFacturaPdfVM
{
    public string CodigoArticulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Unidad { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class FacturaPdfVM
{
    public int ComprobanteId { get; set; }
    public string TipoCPE { get; set; } = "Factura";
    public int PedidoVentaId { get; set; }
    public int? GuiaRemisionId { get; set; }
    public string Cliente { get; set; } = "";
    public string ClienteRuc { get; set; } = "";
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimientoCxC { get; set; }
    public string EstadoCxC { get; set; } = "";
    public decimal SaldoPendienteCxC { get; set; }
    public decimal SubtotalNeto { get; set; }
    public decimal Igv { get; set; }
    public decimal MontoTotal { get; set; }
    public List<ItemFacturaPdfVM> Items { get; set; } = new();
    public List<(int NotaId, DateTime Fecha, decimal Monto)> NotasCredito { get; set; } = new();
}

public class NotaCreditoPdfVM
{
    public int NotaCreditoId { get; set; }
    public int ComprobantePagoId { get; set; }
    public int PedidoVentaId { get; set; }
    public DateTime FechaEmision { get; set; }
    public string Cliente { get; set; } = "";
    public string ClienteRuc { get; set; } = "";
    public string Articulo { get; set; } = "";
    public string NumeroLote { get; set; } = "";
    public decimal CantidadDevuelta { get; set; }
    public string Motivo { get; set; } = "";
    public decimal SubtotalNeto { get; set; }
    public decimal Igv { get; set; }
    public decimal MontoTotal { get; set; }
}