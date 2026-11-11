using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Activos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Lavado
[Authorize]
public class ActivosController : Controller
{
    private readonly DAActivos _da;
    public ActivosController(DAActivos da) => _da = da;

    public IActionResult Index() => View(new List<ActivoFijoItemVM>());
}