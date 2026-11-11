using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Tesoreria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Quito
[Authorize]
public class TesoreriaController : Controller
{
    private readonly DATesoreria _da;
    public TesoreriaController(DATesoreria da) => _da = da;

    public IActionResult Index() => View(new List<CuentaBancariaItemVM>());
}