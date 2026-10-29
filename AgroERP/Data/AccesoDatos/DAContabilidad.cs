using AgroERP.Models.Contabilidad;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos
{
    public class DAContabilidad
    {
        private readonly ApplicationDbContext _db;
        public DAContabilidad(ApplicationDbContext db) => _db = db;

        // No llama SaveChanges: se guarda junto con la operación que lo origina.
        public async Task RegistrarAsiento(string modulo, int? refId, string glosa, string usuarioId,
            params (string cuenta, decimal debe, decimal haber)[] lineas)
        {
            if (lineas.Sum(l => l.debe) != lineas.Sum(l => l.haber))
                throw new InvalidOperationException("El asiento no cuadra: Debe y Haber son distintos.");

            var codigos = lineas.Select(l => l.cuenta).Distinct().ToList();
            var cuentas = await _db.Set<PlanCuenta>()
                .Where(c => codigos.Contains(c.Codigo) && c.EsImputable && c.Activa)
                .ToDictionaryAsync(c => c.Codigo);

            var faltan = codigos.Except(cuentas.Keys).ToList();
            if (faltan.Any())
                throw new InvalidOperationException($"Cuentas no encontradas: {string.Join(", ", faltan)}");

            var asiento = new AsientoContable
            {
                FechaContable = DateTime.Now,
                Glosa = glosa,
                ReferenciaModulo = modulo,
                ReferenciaId = refId,
                UsuarioId = usuarioId,
                Detalles = lineas.Select(l => new AsientoDetalle
                {
                    CuentaId = cuentas[l.cuenta].Id,
                    Debe = l.debe,
                    Haber = l.haber
                }).ToList()
            };
            _db.Add(asiento);
        }
    }
}
