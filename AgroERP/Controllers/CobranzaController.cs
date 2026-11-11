using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Cobranzas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Díaz
[Authorize]
public class CobranzaController : Controller
{
    private readonly DACobranzas _da;
    public CobranzaController(DACobranzas da) => _da = da;

    public IActionResult Index() => View(new List<CuentaPorCobrarItemVM>());
}