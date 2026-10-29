using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Díaz. Tablas que escribe: Cliente, PedidoVenta, PedidoVentaDetalle, OrdenPicking,
/// OrdenPickingDetalle, GuiaRemisionRemitente, ComprobantePago, DevolucionCliente, NotaCredito.
/// REGLA: NO modifiques InventarioExistencia/Reserva/Kardex; usa los métodos de DAAlmacen.
public class DADistribucion
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    private readonly DAAlmacen _almacen;
    public DADistribucion(ApplicationDbContext db, DAContabilidad contabilidad, DAAlmacen almacen)
    {
        _db = db;
        _contabilidad = contabilidad;
        _almacen = almacen;
    }

    // ===================== TU CÓDIGO AQUÍ =====================
    // Factura:  (Cuentas.FacturasPorCobrar, total, 0), (Cuentas.VentasProductos, 0, neto), (Cuentas.IgvPorPagar, 0, igv)
    // Nota de crédito: el asiento inverso.
}
