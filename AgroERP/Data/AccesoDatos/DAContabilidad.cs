using AgroERP.Models.Contabilidad;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Quito. Todos los módulos llaman a RegistrarAsiento; nadie escribe en las tablas contables directamente.
public class DAContabilidad
{
    private readonly ApplicationDbContext _db;
    public DAContabilidad(ApplicationDbContext db) => _db = db;

    /// NO llama SaveChanges: el asiento se guarda junto con la operación que lo origina
    /// (un solo SaveChangesAsync al final del flujo en el DAO que lo invoca).
    public async Task RegistrarAsiento(string modulo, int? referenciaId, string glosa, string usuarioId,
        params (string cuenta, decimal debe, decimal haber)[] lineas)
    {
        if (lineas == null || lineas.Length < 2)
            throw new InvalidOperationException("Un asiento necesita al menos dos líneas.");

        if (lineas.Sum(l => l.debe) != lineas.Sum(l => l.haber))
            throw new InvalidOperationException("El asiento no cuadra: Debe y Haber son distintos.");

        var codigos = lineas.Select(l => l.cuenta).Distinct().ToList();
        var lista = await _db.Set<PlanCuenta>()
            .Where(c => codigos.Contains(c.Codigo) && c.EsImputable && c.Activa)
            .ToListAsync();
        var cuentas = lista.ToDictionary(c => c.Codigo);

        var faltan = codigos.Except(cuentas.Keys).ToList();
        if (faltan.Any())
            throw new InvalidOperationException($"Cuentas no encontradas: {string.Join(", ", faltan)}");

        _db.Add(new AsientoContable
        {
            FechaContable = DateTime.Now,
            Glosa = glosa,
            ReferenciaModulo = modulo,
            ReferenciaId = referenciaId,
            UsuarioId = usuarioId,
            Detalles = lineas.Select(l => new AsientoDetalle
            {
                CuentaId = cuentas[l.cuenta].Id,
                Debe = l.debe,
                Haber = l.haber
            }).ToList()
        });
    }
}
