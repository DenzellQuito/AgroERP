using AgroERP.Data.AccesoDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Quito
[Authorize]
public class PlanificacionController : Controller
{
    private readonly DAPlanificacion _da;
    public PlanificacionController(DAPlanificacion da) => _da = da;

    public IActionResult Index() => View();
}
