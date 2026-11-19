using System.Security.Claims;
using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.BI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Quito
[Authorize]
public class BIController : Controller
{
    private readonly DABI _da;
    public BIController(DABI da) => _da = da;

    public async Task<IActionResult> Dashboard(int dias = 90)
    {
        if (dias is not (30 or 90 or 180 or 365)) dias = 90;
        var vm = new DashboardVM
        {
            Dias = dias,
            Kpis = await _da.CalcularKpisAsync(dias),
            OtifMensual = await _da.OtifPorMesAsync(6),
            CostoMensual = await _da.CostoLogisticoPorMesAsync(6),
            Pareto = await _da.ParetoMermasAsync(dias),
            PlanesAccion = await _da.ListarPlanesAccionAsync(),
            IndicadoresDisponibles = await _da.ListarIndicadoresAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearPlanAccion(int indicadorId, string analisisCausaRaiz, int dias = 90, string? modulo = null)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var err = await _da.CrearPlanAccionAsync(indicadorId, analisisCausaRaiz, usuarioId);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = "Plan de acción registrado en estado EnProgreso.";

        return RedirectToAction(nameof(Dashboard), new { dias, modulo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompletarPlanAccion(int planId, int dias = 90, string? modulo = null)
    {
        var err = await _da.CompletarPlanAccionAsync(planId);
        if (err != null) TempData["Error"] = err;
        else TempData["Ok"] = "Plan de acción marcado como Completado.";

        return RedirectToAction(nameof(Dashboard), new { dias, modulo });
    }

    [HttpGet]
    public async Task<IActionResult> InformeEjecutivoPdf(int dias = 90)
    {
        if (dias is not (30 or 90 or 180 or 365)) dias = 90;
        var vm = new DashboardVM
        {
            Dias = dias,
            Kpis = await _da.CalcularKpisAsync(dias),
            OtifMensual = await _da.OtifPorMesAsync(6),
            CostoMensual = await _da.CostoLogisticoPorMesAsync(6),
            Pareto = await _da.ParetoMermasAsync(dias),
            PlanesAccion = await _da.ListarPlanesAccionAsync(),
            IndicadoresDisponibles = await _da.ListarIndicadoresAsync()
        };
        return View(vm);
    }
}
