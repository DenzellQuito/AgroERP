using AgroERP.Models;
using AgroERP.Models.Tesoreria;
using System;
using System.Threading.Tasks;

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
    public async Task CrearCuentaPorPagar(int proveedorId, int? ordenCompraId, string tipoObligacion, string nroFactura, decimal montoTotal, int diasVencimiento = 30)
    {
        var cuentaPorPagar = new CuentaPorPagar
        {
            ProveedorId = proveedorId,
            OrdenCompraId = ordenCompraId, // null si es mantenimiento o activo sin OC
            TipoObligacion = tipoObligacion,
            NumeroFacturaProveedor = nroFactura, // <-- ¡Corregido!
            MontoTotal = montoTotal,
            SaldoPendiente = montoTotal, // Nace con toda la deuda pendiente
            FechaEmision = DateTime.Now,
            FechaVencimiento = DateTime.Now.AddDays(diasVencimiento),
            Prioridad = "Media",
            Estado = "Pendiente"
        };

        await _db.Set<CuentaPorPagar>().AddAsync(cuentaPorPagar);
    }
}