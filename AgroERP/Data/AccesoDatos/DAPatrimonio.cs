namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Méndez. Escribe TransferenciasActivo, MantenimientosActivo, BajasActivo.
public class DAPatrimonio
{
    private readonly ApplicationDbContext _db;
    private readonly DAActivos _activos;
    private readonly DAPagos _pagos;
    private readonly DAContabilidad _contabilidad;

    public DAPatrimonio(ApplicationDbContext db, DAActivos activos, DAPagos pagos, DAContabilidad contabilidad)
    {
        _db = db;
        _activos = activos;
        _pagos = pagos;
        _contabilidad = contabilidad;
    }
}