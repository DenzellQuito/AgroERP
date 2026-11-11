namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Changa. Escribe CuentasPorPagar, PagosProveedor.
public class DAPagos
{
    private readonly ApplicationDbContext _db;
    private readonly DATesoreria _tesoreria;
    private readonly DAContabilidad _contabilidad;

    public DAPagos(ApplicationDbContext db, DATesoreria tesoreria, DAContabilidad contabilidad)
    {
        _db = db;
        _tesoreria = tesoreria;
        _contabilidad = contabilidad;
    }

    /// Contrato llamado por Lavado (alta activo), Méndez (mantenimiento/mejora) y Changa (OC/fletes). No llama SaveChangesAsync.
    public Task CrearCuentaPorPagar(int proveedorId, int? ordenCompraId, string tipoObligacion, string nroFactura, decimal montoTotal, int diasVencimiento = 30)
        => throw new NotImplementedException("Contrato pendiente de implementar por Changa.");
}