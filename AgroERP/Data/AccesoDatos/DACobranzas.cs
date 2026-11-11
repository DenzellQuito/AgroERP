namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Díaz. Escribe CuentasPorCobrar, CobranzasCliente.
public class DACobranzas
{
    private readonly ApplicationDbContext _db;
    private readonly DATesoreria _tesoreria;
    private readonly DAContabilidad _contabilidad;

    public DACobranzas(ApplicationDbContext db, DATesoreria tesoreria, DAContabilidad contabilidad)
    {
        _db = db;
        _tesoreria = tesoreria;
        _contabilidad = contabilidad;
    }

    /// Contrato para abrir la CuentaPorCobrar al emitir un ComprobantePago. No llama SaveChangesAsync.
    public Task CrearCuentaPorCobrar(int comprobantePagoId, int diasCredito = 30)
        => throw new NotImplementedException("Contrato pendiente de implementar por Díaz.");
}