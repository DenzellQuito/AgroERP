using AgroERP.Models.ActivosFijos;
using Microsoft.EntityFrameworkCore;

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
    public async Task ActualizarUbicacionYCustodio(int activoId, int zonaUbicacionId, string usuarioResponsableId)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null) throw new Exception("Activo no encontrado.");

        activo.ZonaUbicacionId = zonaUbicacionId;
        activo.UsuarioResponsableId = usuarioResponsableId;

        _db.Set<ActivoFijo>().Update(activo);
    }

    /// Contrato llamado por Méndez al registrar una mejora capitalizable. No llama SaveChangesAsync.
    public async Task CapitalizarMejora(int activoId, decimal montoMejora)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null) throw new Exception("Activo no encontrado.");

        // La mejora suma valor al costo histórico y al valor neto según NIC 16
        activo.CostoAdquisicion += montoMejora;
        activo.ValorNetoLibros += montoMejora;

        _db.Set<ActivoFijo>().Update(activo);
    }

    /// Contrato llamado por Méndez al dar de baja un activo fijo. No llama SaveChangesAsync.
    public async Task MarcarActivoDadoDeBaja(int activoId)
    {
        var activo = await _db.Set<ActivoFijo>().FindAsync(activoId);
        if (activo == null) throw new Exception("Activo no encontrado.");

        // Texto exacto de estado según regla 8 del README
        activo.Estado = "DadoDeBaja";

        _db.Set<ActivoFijo>().Update(activo);
    }
}