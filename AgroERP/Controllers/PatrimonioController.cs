using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Méndez
[Authorize]
public class PatrimonioController : Controller
{
    private readonly DAPatrimonio _da;
    public PatrimonioController(DAPatrimonio da) => _da = da;

    public IActionResult Index() => View(new List<MovimientoPatrimonioItemVM>());
}