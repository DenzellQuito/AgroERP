using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Planificacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

[Authorize]
public class ArticulosController : Controller
{
    private readonly DAMaestros _da;
    public ArticulosController(DAMaestros da) => _da = da;

    public async Task<IActionResult> Index() => View(await _da.ListarArticulosAsync());

    public IActionResult Crear() => View("Form", new ArticuloVM());

    public async Task<IActionResult> Editar(int id)
    {
        var vm = await _da.ObtenerArticuloAsync(id);
        return vm == null ? NotFound() : View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(ArticuloVM vm)
    {
        if (ModelState.IsValid)
        {
            var error = await _da.GuardarArticuloAsync(vm);
            if (error == null) { TempData["Ok"] = "Artículo guardado."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError("", error);
        }
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var error = await _da.EliminarArticuloAsync(id);
        if (error == null) TempData["Ok"] = "Artículo eliminado."; else TempData["Error"] = error;
        return RedirectToAction(nameof(Index));
    }
}
