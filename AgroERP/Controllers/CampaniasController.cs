using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Planificacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

[Authorize]
public class CampaniasController : Controller
{
    private readonly DAMaestros _da;
    public CampaniasController(DAMaestros da) => _da = da;

    public async Task<IActionResult> Index() => View(await _da.ListarCampaniasAsync());

    public IActionResult Crear() => View("Form", new CampaniaVM());

    public async Task<IActionResult> Editar(int id)
    {
        var vm = await _da.ObtenerCampaniaAsync(id);
        return vm == null ? NotFound() : View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(CampaniaVM vm)
    {
        if (ModelState.IsValid)
        {
            var error = await _da.GuardarCampaniaAsync(vm);
            if (error == null) { TempData["Ok"] = "Campaña guardada."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError("", error);
        }
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var error = await _da.EliminarCampaniaAsync(id);
        if (error == null) TempData["Ok"] = "Campaña eliminada."; else TempData["Error"] = error;
        return RedirectToAction(nameof(Index));
    }
}
