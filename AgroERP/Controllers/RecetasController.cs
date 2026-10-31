using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Planificacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroERP.Controllers;

[Authorize]
public class RecetasController : Controller
{
    private readonly DAMaestros _da;
    public RecetasController(DAMaestros da) => _da = da;

    private async Task<RecetaVM> ConArticulos(RecetaVM vm)
    {
        vm.Articulos = (await _da.ListarArticulosAsync())
            .Select(a => new SelectListItem($"{a.Codigo} - {a.Nombre}", a.Id.ToString())).ToList();
        return vm;
    }

    public async Task<IActionResult> Index() => View(await _da.ListarRecetasAsync());

    public async Task<IActionResult> Crear() => View("Form", await ConArticulos(new RecetaVM()));

    public async Task<IActionResult> Editar(int id)
    {
        var vm = await _da.ObtenerRecetaAsync(id);
        return vm == null ? NotFound() : View("Form", await ConArticulos(vm));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(RecetaVM vm)
    {
        if (ModelState.IsValid)
        {
            var error = await _da.GuardarRecetaAsync(vm);
            if (error == null) { TempData["Ok"] = "Receta guardada."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError("", error);
        }
        return View("Form", await ConArticulos(vm));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _da.EliminarRecetaAsync(id);
        TempData["Ok"] = "Receta eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
