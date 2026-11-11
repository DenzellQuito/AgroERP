using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Changa
[Authorize]
public class PagosController : Controller
{
    private readonly DAPagos _da;
    public PagosController(DAPagos da) => _da = da;

    public IActionResult Index() => View(new List<CuentaPorPagarItemVM>());
}