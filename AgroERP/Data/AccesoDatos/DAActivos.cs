namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Lavado. Escribe ActivosFijos, DepreciacionesActivo.
public class DAActivos
{
    private readonly ApplicationDbContext _db;
    private readonly DAPagos _pagos;
    private readonly DAContabilidad _contabilidad;

    public DAActivos(ApplicationDbContext db, DAPagos pagos, DAContabilidad contabilidad)
    {
        _db = db;
        _pagos = pagos;
        _contabilidad = contabilidad;
    }

    /// Contrato llamado por Méndez al transferir un activo fijo. No llama SaveChangesAsync.
    public Task ActualizarUbicacionYCustodio(int activoId, int zonaUbicacionId, string usuarioResponsableId)
        => throw new NotImplementedException("Contrato pendiente de implementar por Lavado.");

    /// Contrato llamado por Méndez al registrar una mejora capitalizable. No llama SaveChangesAsync.
    public Task CapitalizarMejora(int activoId, decimal montoMejora)
        => throw new NotImplementedException("Contrato pendiente de implementar por Lavado.");

    /// Contrato llamado por Méndez al dar de baja un activo fijo. No llama SaveChangesAsync.
    public Task MarcarActivoDadoDeBaja(int activoId)
        => throw new NotImplementedException("Contrato pendiente de implementar por Lavado.");
}