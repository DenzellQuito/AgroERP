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
            Pareto = await _da.ParetoMermasAsync(dias)
        };
        return View(vm);
    }
}
