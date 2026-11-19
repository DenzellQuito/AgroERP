using System.Security.Claims;
using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Cobranzas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Controllers;

/// Responsable: Díaz
[Authorize]
public class CobranzaController : Controller
{
    private readonly DACobranzas _da;
    private readonly ApplicationDbContext _db;

    public CobranzaController(DACobranzas da, ApplicationDbContext db)
    {
        _da = da;
        _db = db;
    }

    // 1. BANDEJA DE CUENTAS POR COBRAR
    public async Task<IActionResult> Index()
    {
        var cuentas = await _db.CuentasPorCobrar
            .Include(c => c.Cliente)
            .Include(c => c.ComprobantePago)
            .OrderByDescending(c => c.FechaVencimiento)
            .Select(c => new CuentaPorCobrarItemVM
            {
                Id = c.Id,
                ComprobantePagoId = c.ComprobantePagoId,
                PedidoVentaId = c.ComprobantePago.PedidoVentaId,
                Cliente = c.Cliente.RazonSocial,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                MontoTotal = c.MontoTotal,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado,
                UltimaCobranzaId = _db.CobranzasCliente
                    .Where(cb => cb.CuentaPorCobrarId == c.Id)
                    .OrderByDescending(cb => cb.Id)
                    .Select(cb => (int?)cb.Id)
                    .FirstOrDefault(),
                UltimaNotaCreditoId = _db.NotaCredito
                    .Where(n => n.ComprobantePagoId == c.ComprobantePagoId)
                    .OrderByDescending(n => n.Id)
                    .Select(n => (int?)n.Id)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return View(cuentas);
    }

    // 2. REGISTRAR COBRO (GET)
    public async Task<IActionResult> RegistrarCobro(int id)
    {
        var cxc = await _db.CuentasPorCobrar
            .Include(c => c.Cliente)
            .Include(c => c.ComprobantePago)
            .Include(c => c.Cobranzas).ThenInclude(cb => cb.CuentaBancaria)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cxc == null) return NotFound();

        var bancos = await _db.CuentasBancarias
            .Where(c => c.Activa)
            .Select(c => new
            {
                c.Id,
                Descripcion = $"{c.Banco} · {c.NumeroCuenta} (Disp: S/ {c.SaldoDisponible:N2})"
            })
            .ToListAsync();

        ViewBag.CuentaBancariaId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(bancos, "Id", "Descripcion");
        ViewBag.NotasCredito = await _db.NotaCredito
            .Where(n => n.ComprobantePagoId == cxc.ComprobantePagoId)
            .OrderByDescending(n => n.Fecha)
            .ToListAsync();

        ViewBag.CxC = cxc;
        return View();
    }

    // 3. REGISTRAR COBRO (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarCobro(int cuentaPorCobrarId, int cuentaBancariaId, decimal monto, string medioPago, string nroOperacion)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            await _da.RegistrarCobranza(cuentaPorCobrarId, cuentaBancariaId, monto, medioPago, nroOperacion, userId);
            TempData["Success"] = "Cobro registrado exitosamente. El saldo ha sido actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Warning"] = "Error al registrar el cobro: " + ex.Message;
            return RedirectToAction(nameof(RegistrarCobro), new { id = cuentaPorCobrarId });
        }
    }

    // 4. RECIBO DE COBRANZA PDF
    [HttpGet]
    public async Task<IActionResult> ReciboCobranzaPdf(int id)
    {
        var cob = await _db.CobranzasCliente
            .Include(cb => cb.CuentaBancaria)
            .Include(cb => cb.CuentaPorCobrar).ThenInclude(c => c.Cliente)
            .FirstOrDefaultAsync(cb => cb.Id == id);
        if (cob == null) return NotFound();

        var usuario = await _db.Users
            .Where(u => u.Id == cob.UsuarioId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync() ?? cob.UsuarioId;

        var vm = new ReciboCobranzaPdfVM
        {
            CobranzaId = cob.Id,
            CuentaPorCobrarId = cob.CuentaPorCobrarId,
            ComprobantePagoId = cob.CuentaPorCobrar.ComprobantePagoId,
            Cliente = cob.CuentaPorCobrar.Cliente.RazonSocial,
            ClienteRuc = cob.CuentaPorCobrar.Cliente.RucDni,
            CuentaBancaria = $"{cob.CuentaBancaria.Banco} · {cob.CuentaBancaria.NumeroCuenta}",
            FechaCobro = cob.FechaCobro,
            MedioPago = cob.MedioPago,
            NumeroOperacion = cob.NumeroOperacion,
            MontoTotalFactura = cob.CuentaPorCobrar.MontoTotal,
            MontoCobrado = cob.MontoCobrado,
            SaldoRestante = cob.CuentaPorCobrar.SaldoPendiente,
            EstadoCxC = cob.CuentaPorCobrar.Estado,
            Usuario = usuario
        };

        return View(vm);
    }

    // 5. REPORTE DE ANTIGÜEDAD DE SALDOS PDF
    [HttpGet]
    public async Task<IActionResult> AntiguedadSaldosPdf()
    {
        var hoy = DateTime.Today;
        var pendientes = await _db.CuentasPorCobrar
            .Include(c => c.Cliente)
            .Where(c => c.SaldoPendiente > 0 && c.Estado != "Cobrado")
            .OrderBy(c => c.FechaVencimiento)
            .ToListAsync();

        var items = pendientes.Select(c =>
        {
            int atraso = (hoy - c.FechaVencimiento.Date).Days;
            string tramo = atraso <= 0 ? "Vigente" : atraso <= 30 ? "Vencido 1-30 días" : "Vencido >30 días";
            return new AntiguedadSaldosItemVM
            {
                CuentaPorCobrarId = c.Id,
                ComprobantePagoId = c.ComprobantePagoId,
                Cliente = c.Cliente.RazonSocial,
                ClienteRuc = c.Cliente.RucDni,
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                DiasAtraso = Math.Max(0, atraso),
                MontoTotal = c.MontoTotal,
                SaldoPendiente = c.SaldoPendiente,
                Tramo = tramo
            };
        }).ToList();

        var vm = new AntiguedadSaldosPdfVM
        {
            FechaCorte = DateTime.Now,
            TotalVigente = items.Where(i => i.Tramo == "Vigente").Sum(i => i.SaldoPendiente),
            TotalVencido1a30 = items.Where(i => i.Tramo == "Vencido 1-30 días").Sum(i => i.SaldoPendiente),
            TotalVencidoMas30 = items.Where(i => i.Tramo == "Vencido >30 días").Sum(i => i.SaldoPendiente),
            TotalCarteraPendiente = items.Sum(i => i.SaldoPendiente),
            Items = items
        };

        return View(vm);
    }
}