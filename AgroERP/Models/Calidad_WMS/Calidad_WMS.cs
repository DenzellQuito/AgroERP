namespace AgroERP.Models.Calidad_WMS;
using Maestros;
using Distribucion;
using Compras;
public class Almacen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;            // Insumos | AcopioFresa
    public List<ZonaUbicacion> Zonas { get; set; } = new();
}

public class ZonaUbicacion
{
    public int Id { get; set; }
    public int AlmacenId { get; set; }
    public Almacen Almacen { get; set; } = null!;
    public string CodigoUbicacion { get; set; } = null!; // ZN-01
}

public class Lote
{
    public int Id { get; set; }
    public int ArticuloId { get; set; }                  // único (ArticuloId, NumeroLote)
    public Articulo Articulo { get; set; } = null!;
    public string NumeroLote { get; set; } = null!;
    public DateTime FechaVencimiento { get; set; }       // columna tipo date (FEFO)
    public string EstadoCalidad { get; set; } = null!;  // Cuarentena | Aceptado | Rechazado
}

public class RecepcionMercaderia
{
    public int Id { get; set; }
    public int OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;
    public string GuiaProveedor { get; set; } = null!;
    public DateTime FechaRecepcion { get; set; }
    public string UsuarioReceptorId { get; set; } = null!;      // FK AspNetUsers
    public string Estado { get; set; } = null!;
    public List<RecepcionDetalle> Detalles { get; set; } = new();
}

public class RecepcionDetalle
{
    public int Id { get; set; }
    public int RecepcionId { get; set; }
    public RecepcionMercaderia Recepcion { get; set; } = null!;
    public int ArticuloId { get; set; }
    public Articulo Articulo { get; set; } = null!;
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public decimal CantidadPedida { get; set; }          // según OC         (18,4)
    public decimal CantidadGuia { get; set; }            // según proveedor  (18,4)
    public decimal CantidadRecibida { get; set; }        // contada          (18,4)
}

public class InspeccionCalidad
{
    public int Id { get; set; }
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public string UsuarioInspectorId { get; set; } = null!;     // FK AspNetUsers
    public bool EsConforme { get; set; }
    public string? MotivoRechazo { get; set; }
}

public class InventarioExistencia
{
    public int Id { get; set; }
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public int ZonaUbicacionId { get; set; }
    public ZonaUbicacion ZonaUbicacion { get; set; } = null!;
    public decimal CantidadFisica { get; set; }          // (18,4)
    public decimal CantidadReservada { get; set; }       // (18,4) CHECK <= Fisica
}

public class ReservaInventario
{
    public int Id { get; set; }
    public int PedidoVentaDetalleId { get; set; }
    public PedidoVentaDetalle PedidoVentaDetalle { get; set; } = null!;
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public int ZonaUbicacionId { get; set; }
    public ZonaUbicacion ZonaUbicacion { get; set; } = null!;
    public decimal Cantidad { get; set; }                // (18,4)
    public string Estado { get; set; } = null!;         // Activa | Inactiva
}

public class MovimientoKardex
{
    public int Id { get; set; }
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = null!;
    public decimal Cantidad { get; set; }                // (18,4)
    public int? ZonaOrigenId { get; set; }
    public ZonaUbicacion? ZonaOrigen { get; set; }
    public int? ZonaDestinoId { get; set; }
    public ZonaUbicacion? ZonaDestino { get; set; }
    public string? ReferenciaDocumento { get; set; }     // "OC-105", "PED-20"
}

public class AjusteInventario
{
    public int Id { get; set; }
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;
    public int ZonaUbicacionId { get; set; }
    public ZonaUbicacion ZonaUbicacion { get; set; } = null!;
    public string UsuarioId { get; set; } = null!;              // FK AspNetUsers
    public DateTime Fecha { get; set; }
    public string TipoAjuste { get; set; } = null!;
    public decimal Cantidad { get; set; }                // (18,4)
    public decimal CostoAsumido { get; set; }            // (18,2)
}
