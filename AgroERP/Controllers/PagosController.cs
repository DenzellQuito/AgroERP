using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.Models.Tesoreria;
using AgroERP.ViewModels.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgroERP.Controllers;

/// Responsable: Changa
[Authorize]
public class PagosController : Controller
{
    private readonly DAPagos _da;

    public PagosController(DAPagos da) => _da = da;

    private static async Task CargarBancosAsync(Controller ctrl, ApplicationDbContext db, int? seleccionadoId = null)
    {
        var bancos = await db.Set<CuentaBancaria>()
            .Where(b => b.Activa)
            .Select(b => new
            {
                b.Id,
                Descripcion = $"{b.Banco} · {b.NumeroCuenta} (Disp: S/ {b.SaldoDisponible:N2})"
            })
            .ToListAsync();

        ctrl.ViewBag.Bancos = new SelectList(bancos, "Id", "Descripcion", seleccionadoId);
    }

    // GET: /Pagos/Index
    public async Task<IActionResult> Index()
    {
        var modelo = await _da.ObtenerBandejaPagosAsync();
        return View(modelo);
    }

    // GET: /Pagos/Programar/5
    public async Task<IActionResult> Programar(int id)
    {
        try
        {
            await _da.ProgramarPagoAsync(id);
            TempData["Ok"] = "Obligación programada para pago correctamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Pagos/Autorizar/5
    public async Task<IActionResult> Autorizar(int id)
    {
        try
        {
            await _da.AutorizarPagoAsync(id);
            TempData["Ok"] = "Pago autorizado correctamente. Ya puede ejecutar el desembolso.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Pagos/Pagar/5
    public async Task<IActionResult> Pagar(int id, [FromServices] ApplicationDbContext db)
    {
        var cuenta = await db.Set<CuentaPorPagar>()
            .Include(c => c.Proveedor)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cuenta == null || (cuenta.Estado != "Autorizado" && cuenta.Estado != "PagadoParcial"))
        {
            TempData["Error"] = "La cuenta por pagar no existe o aún no está autorizada.";
            return RedirectToAction(nameof(Index));
        }

        await CargarBancosAsync(this, db);

        var modelo = new PagoViewModel
        {
            CuentaPorPagarId = cuenta.Id,
            Proveedor = cuenta.Proveedor.RazonSocial,
            NumeroFactura = cuenta.NumeroFacturaProveedor,
            SaldoPendiente = cuenta.SaldoPendiente,
            MontoAPagar = decimal.Round(cuenta.SaldoPendiente, 2)
        };
        return View(modelo);
    }

    // POST: /Pagos/Pagar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(PagoViewModel modelo, [FromServices] ApplicationDbContext db)
    {
        ModelState.Remove(nameof(modelo.Proveedor));
        ModelState.Remove(nameof(modelo.NumeroFactura));

        if (!ModelState.IsValid)
        {
            await RecargarEncabezadoFacturaAsync(db, modelo);
            await CargarBancosAsync(this, db, modelo.CuentaBancariaId);
            return View(modelo);
        }

        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        try
        {
            await _da.ProcesarPagoAsync(
                modelo.CuentaPorPagarId,
                modelo.CuentaBancariaId,
                modelo.MedioPago,
                modelo.NumeroOperacion,
                modelo.MontoAPagar,
                usuarioId);

            TempData["Ok"] = $"Pago por S/ {modelo.MontoAPagar:N2} registrado, descontado en Tesorería y contabilizado en el Libro Diario.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error: " + ex.Message);
            await RecargarEncabezadoFacturaAsync(db, modelo);
            await CargarBancosAsync(this, db, modelo.CuentaBancariaId);
            return View(modelo);
        }
    }

    private static async Task RecargarEncabezadoFacturaAsync(ApplicationDbContext db, PagoViewModel modelo)
    {
        var cuenta = await db.Set<CuentaPorPagar>()
            .Include(c => c.Proveedor)
            .FirstOrDefaultAsync(c => c.Id == modelo.CuentaPorPagarId);

        if (cuenta != null)
        {
            modelo.Proveedor = cuenta.Proveedor.RazonSocial;
            modelo.NumeroFactura = cuenta.NumeroFacturaProveedor;
            modelo.SaldoPendiente = cuenta.SaldoPendiente;
        }
    }
}