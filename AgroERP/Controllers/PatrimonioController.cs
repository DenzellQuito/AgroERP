using System.Security.Claims;
using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Méndez
[Authorize]
public class PatrimonioController : Controller
{
    private readonly DAPatrimonio _da;
    public PatrimonioController(DAPatrimonio da) => _da = da;

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = await _da.ObtenerPanelAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarTransferencia(RegistrarTransferenciaVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = PrimerError() ?? "Verifique los datos del traslado.";
            return RedirectToAction(nameof(Index));
        }

        var err = await _da.RegistrarTransferenciaAsync(vm, UsuarioActualId);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = "Traslado registrado: se actualizó la ubicación y el custodio del activo.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarMantenimiento(RegistrarMantenimientoVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = PrimerError() ?? "Verifique los datos del mantenimiento.";
            return RedirectToAction(nameof(Index));
        }

        var err = await _da.RegistrarMantenimientoAsync(vm, UsuarioActualId);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = vm.Tipo == "MejoraCapitalizable"
            ? $"Mejora capitalizada por S/ {vm.Costo:N2}: se incrementó el valor del activo, se contabilizó (3331 / 4654) y se generó la cuenta por pagar."
            : $"Mantenimiento {vm.Tipo.ToLower()} por S/ {vm.Costo:N2} registrado, contabilizado (6341 / 4212) y enviado a Cuentas por Pagar.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarBaja(RegistrarBajaVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = PrimerError() ?? "Verifique los datos de la baja.";
            return RedirectToAction(nameof(Index));
        }

        var err = await _da.RegistrarBajaAsync(vm, UsuarioActualId);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = $"Activo dado de baja por {vm.MotivoBaja} y contabilizado (3913 + 6551 / 3331).";

        return RedirectToAction(nameof(Index));
    }

    private string? PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
}
