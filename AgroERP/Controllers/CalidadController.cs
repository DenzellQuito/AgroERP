using AgroERP.Data.AccesoDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: George Lavado
[Authorize]
public class CalidadController : Controller
{
    private readonly DACalidad _da;
    public CalidadController(DACalidad da) => _da = da;

    public IActionResult Index() => View();
}
