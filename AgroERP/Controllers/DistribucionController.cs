using AgroERP.Data.AccesoDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Díaz
[Authorize]
public class DistribucionController : Controller
{
    private readonly DADistribucion _da;
    public DistribucionController(DADistribucion da) => _da = da;

    public IActionResult Index() => View();
}
