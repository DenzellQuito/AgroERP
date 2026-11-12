using AgroERP.Models;
using AgroERP.Models.Tesoreria;
using Microsoft.EntityFrameworkCore;
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

    // =========================================================================
    // BANDEJA PRINCIPAL: Lista las deudas para programar y pagar
    // =========================================================================
    public async Task<List<ViewModels.Pagos.CuentaPorPagarItemVM>> ObtenerBandejaPagosAsync()
    {
        return await _db.Set<CuentaPorPagar>()
            .Include(c => c.Proveedor) // Traemos los datos del proveedor
            .Where(c => c.Estado != "Pagado") // No mostramos los que ya se pagaron por completo
            .OrderBy(c => c.FechaVencimiento) // Ordenamos para saber qué se vence primero
            .Select(c => new ViewModels.Pagos.CuentaPorPagarItemVM
            {
                Id = c.Id,
                Proveedor = c.Proveedor.RazonSocial,
                TipoObligacion = c.TipoObligacion,
                NumeroFactura = c.NumeroFacturaProveedor, // El campo que corregimos antes
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                Prioridad = c.Prioridad,
                MontoTotal = c.MontoTotal,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado
            })
            .ToListAsync();
    }

    // =========================================================================
    // TAREA BIZAGI (A02): Seleccionar obligaciones y cambiar estado a Programado
    // =========================================================================
    public async Task ProgramarPagoAsync(int cuentaPorPagarId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta != null && cuenta.Estado == "Pendiente")
        {
            cuenta.Estado = "Programado";
            await _db.SaveChangesAsync();
        }
    }

    // =========================================================================
    // TAREA BIZAGI (A03): Validar datos y autorizar pago
    // =========================================================================
    public async Task AutorizarPagoAsync(int cuentaPorPagarId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta != null && cuenta.Estado == "Programado")
        {
            cuenta.Estado = "Autorizado";
            await _db.SaveChangesAsync();
        }
    }
}