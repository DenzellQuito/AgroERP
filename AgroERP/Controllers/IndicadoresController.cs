using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.BI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroERP.Controllers;

[Authorize]
public class IndicadoresController : Controller
{
    private readonly DABI _da;
    public IndicadoresController(DABI da) => _da = da;

    private static IndicadorVM ConNombres(IndicadorVM vm)
    {
        vm.Nombres = KpiNombres.Todos.Select(n => new SelectListItem(n, n)).ToList();
        return vm;
    }

    public async Task<IActionResult> Index() => View(await _da.ListarIndicadoresAsync());

    public IActionResult Crear() => View("Form", ConNombres(new IndicadorVM()));

    public async Task<IActionResult> Editar(int id)
    {
        var vm = await _da.ObtenerIndicadorAsync(id);
        return vm == null ? NotFound() : View("Form", ConNombres(vm));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(IndicadorVM vm)
    {
        if (ModelState.IsValid)
        {
            var error = await _da.GuardarIndicadorAsync(vm);
            if (error == null) { TempData["Ok"] = "Indicador guardado."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError("", error);
        }
        return View("Form", ConNombres(vm));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var error = await _da.EliminarIndicadorAsync(id);
        if (error == null) TempData["Ok"] = "Indicador eliminado."; else TempData["Error"] = error;
        return RedirectToAction(nameof(Index));
    }
}
