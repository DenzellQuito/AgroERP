using AgroERP.Data.AccesoDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Méndez
[Authorize]
public class AlmacenController : Controller
{
    private readonly DAAlmacen _da;
    public AlmacenController(DAAlmacen da) => _da = da;

    public IActionResult Index() => View();
}
