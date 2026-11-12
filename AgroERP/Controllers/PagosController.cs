using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AgroERP.Controllers;

/// Responsable: Changa
[Authorize]
public class PagosController : Controller
{
    private readonly DAPagos _da;

    public PagosController(DAPagos da) => _da = da;

    // GET: /Pagos/Index
    public async Task<IActionResult> Index()
    {
        // Llamamos a tu método en la capa de datos
        var modelo = await _da.ObtenerBandejaPagosAsync();
        return View(modelo);
    }

    // GET: /Pagos/Programar/5
    public async Task<IActionResult> Programar(int id)
    {
        await _da.ProgramarPagoAsync(id);
        return RedirectToAction(nameof(Index)); // Recarga la bandeja actualizada
    }

    // GET: /Pagos/Autorizar/5
    public async Task<IActionResult> Autorizar(int id)
    {
        await _da.AutorizarPagoAsync(id);
        return RedirectToAction(nameof(Index)); // Recarga la bandeja actualizada
    }

}