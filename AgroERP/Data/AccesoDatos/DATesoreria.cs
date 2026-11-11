namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Quito. Escribe CuentasBancarias, MovimientosBancarios, ConciliacionesBancarias.
public class DATesoreria
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;

    public DATesoreria(ApplicationDbContext db, DAContabilidad contabilidad)
    {
        _db = db;
        _contabilidad = contabilidad;
    }

    /// Contrato llamado por Díaz (Ingreso por cobranza) y Changa (Egreso por pago). No llama SaveChangesAsync.
    public Task RegistrarMovimientoBancario(int cuentaBancariaId, string tipoMovimiento, decimal monto, string medioPago, string nroOperacion, string referencia)
        => throw new NotImplementedException("Contrato pendiente de implementar por Quito.");
}