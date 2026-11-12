using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.Models.Tesoreria;
using AgroERP.ViewModels.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgroERP.Controllers;

/// Responsable: Changa
[Authorize]
public class PagosController : Controller
{
    private readonly DAPagos _da;

    public PagosController(DAPagos da) => _da = da;

    // GET: /Pagos/Index
    public async Task<IActionResult> Index()
    {
        // Llamamos a tu método en la capa de datos
        var modelo = await _da.ObtenerBandejaPagosAsync();
        return View(modelo);
    }

    // GET: /Pagos/Programar/5
    public async Task<IActionResult> Programar(int id)
    {
        await _da.ProgramarPagoAsync(id);
        return RedirectToAction(nameof(Index)); // Recarga la bandeja actualizada
    }

    // GET: /Pagos/Autorizar/5
    public async Task<IActionResult> Autorizar(int id)
    {
        await _da.AutorizarPagoAsync(id);
        return RedirectToAction(nameof(Index)); // Recarga la bandeja actualizada
    }

    // GET: /Pagos/Pagar/5
    public async Task<IActionResult> Pagar(int id, [FromServices] ApplicationDbContext db)
    {
        // "Lectura libre" (Regla 1) - Buscamos la deuda y leemos los bancos de Quito
        var cuenta = await db.Set<CuentaPorPagar>().Include(c => c.Proveedor).FirstOrDefaultAsync(c => c.Id == id);
        if (cuenta == null || (cuenta.Estado != "Autorizado" && cuenta.Estado != "PagadoParcial"))
            return RedirectToAction(nameof(Index));

        ViewBag.Bancos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await db.Set<CuentaBancaria>().Where(b => b.Activa).ToListAsync(), "Id", "Banco");

        var modelo = new PagoViewModel
        {
            CuentaPorPagarId = cuenta.Id,
            Proveedor = cuenta.Proveedor.RazonSocial,
            NumeroFactura = cuenta.NumeroFacturaProveedor,
            SaldoPendiente = cuenta.SaldoPendiente,
            MontoAPagar = cuenta.SaldoPendiente // Por defecto sugerimos pagar todo
        };
        return View(modelo);
    }

    // POST: /Pagos/Pagar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(PagoViewModel modelo, [FromServices] ApplicationDbContext db)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Bancos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await db.Set<CuentaBancaria>().Where(b => b.Activa).ToListAsync(), "Id", "Banco");
            return View(modelo);
        }

        // Usuario conectado (Regla 9)
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            await _da.ProcesarPagoAsync(modelo.CuentaPorPagarId, modelo.CuentaBancariaId, modelo.MedioPago, modelo.NumeroOperacion, modelo.MontoAPagar, usuarioId);
            return RedirectToAction(nameof(Index));
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("", "Error: " + ex.Message);
            ViewBag.Bancos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await db.Set<CuentaBancaria>().Where(b => b.Activa).ToListAsync(), "Id", "Banco");
            return View(modelo);
        }
    }


}