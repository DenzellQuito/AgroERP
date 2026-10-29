using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Changa. Tablas que escribe: Proveedor, RequerimientoCompra, RequerimientoDetalle,
/// CotizacionProveedor, OrdenCompra, OrdenCompraDetalle, GastoLogistico.
public class DACompras
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    public DACompras(ApplicationDbContext db, DAContabilidad contabilidad)
    {
        _db = db;
        _contabilidad = contabilidad;
    }

    // ===================== CONTRATOS (los llaman otros módulos) =====================

    /// LLAMA: Quito (MRP). Crea un RequerimientoCompra en estado "Borrador" con un detalle por cada
    /// Necesidad_MRP (guardando NecesidadMrpId) y marca esas necesidades como "ConRequerimiento".
    /// Devuelve el Id del requerimiento creado.
    public Task<int> CrearRequerimientoDesdeMrp(List<int> necesidadIds, string usuarioId)
        => throw new NotImplementedException("Pendiente: Changa");

    /// LLAMA: Lavado (al cerrar una recepción). Cambia OrdenCompra.Estado a
    /// "RecepcionadaParcial" si parcial = true, o "Cerrada" si parcial = false.
    public Task MarcarOrdenRecepcionada(int ordenCompraId, bool parcial)
        => throw new NotImplementedException("Pendiente: Changa");

    // ===================== TU CÓDIGO AQUÍ =====================
}
