using AgroERP.Data.AccesoDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Changa
[Authorize]
public class ComprasController : Controller
{
    private readonly DACompras _da;
    public ComprasController(DACompras da) => _da = da;

    public IActionResult Index() => View();
}
