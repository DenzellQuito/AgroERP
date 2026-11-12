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
            .OrderByDescending(c => c.FechaVencimiento)
            .Select(c => new CuentaPorCobrarItemVM
            {
                Id = c.Id,
                Cliente = c.Cliente.RazonSocial,
                FechaVencimiento = c.FechaVencimiento,
                MontoTotal = c.MontoTotal,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado
            })
            .ToListAsync();

        return View(cuentas);
    }

    // 2. REGISTRAR COBRO (GET)
    public async Task<IActionResult> RegistrarCobro(int id)
    {
        var cxc = await _db.CuentasPorCobrar.Include(c => c.Cliente).FirstOrDefaultAsync(c => c.Id == id);
        if (cxc == null) return NotFound();

        ViewBag.CuentaBancariaId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            await _db.CuentasBancarias.Where(c => c.Activa).ToListAsync(), "Id", "Banco");

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
}