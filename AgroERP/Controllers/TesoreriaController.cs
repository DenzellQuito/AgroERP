using System.Security.Claims;
using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Tesoreria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Quito
[Authorize]
public class TesoreriaController : Controller
{
    private readonly DATesoreria _da;
    public TesoreriaController(DATesoreria da) => _da = da;

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = await _da.ObtenerDashboardAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCuenta(CrearCuentaBancariaVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Verifique los datos de la cuenta bancaria.";
            return RedirectToAction(nameof(Index));
        }

        var err = await _da.CrearCuentaBancariaAsync(vm);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = $"Cuenta bancaria {vm.Banco} ({vm.NumeroCuenta}) registrada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoCuenta(int id)
    {
        var err = await _da.CambiarEstadoCuentaAsync(id);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = "Estado de la cuenta actualizado.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarMovimiento(RegistrarMovimientoManualVM vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Complete todos los campos del movimiento bancario.";
            return RedirectToAction(nameof(Index));
        }

        var err = await _da.RegistrarMovimientoManualAsync(vm);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = $"Movimiento de {vm.TipoMovimiento} por S/ {vm.Monto:N2} registrado en Libro Bancos.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Conciliar(int id)
    {
        var vm = await _da.ObtenerDatosConciliacionAsync(id);
        if (vm == null)
        {
            TempData["Error"] = "La cuenta bancaria solicitada no existe.";
            return RedirectToAction(nameof(Index));
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarMovimientoConciliado(int movimientoId, int cuentaBancariaId)
    {
        var err = await _da.ConciliarMovimientoIndividualAsync(movimientoId);
        if (err != null) TempData["Error"] = err;
        return RedirectToAction(nameof(Conciliar), new { id = cuentaBancariaId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EjecutarConciliacion(ConciliarCuentaVM vm)
    {
        var err = await _da.EjecutarConciliacionAsync(vm, UsuarioActualId);
        if (err != null)
        {
            TempData["Error"] = err;
            return RedirectToAction(nameof(Conciliar), new { id = vm.CuentaBancariaId });
        }

        TempData["Ok"] = "Conciliación bancaria cuadrada, contabilizada y cerrada exitosamente.";
        return RedirectToAction(nameof(Conciliar), new { id = vm.CuentaBancariaId });
    }
}