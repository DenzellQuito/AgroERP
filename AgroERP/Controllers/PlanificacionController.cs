using System.Security.Claims;
using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Planificacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

/// Responsable: Quito
[Authorize]
public class PlanificacionController : Controller
{
    private readonly DAPlanificacion _da;
    public PlanificacionController(DAPlanificacion da) => _da = da;

    public async Task<IActionResult> Index()
    {
        var lista = await _da.ListarCampaniasAsync();
        return View(lista.Select(c => new CampaniaItemVM
        {
            Id = c.Id, Nombre = c.Nombre, HectareasActivas = c.HectareasActivas,
            MesProyectado = c.MesProyectado, Estado = c.Estado.ToString()
        }).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Calcular(int campaniaId)
    {
        try
        {
            var p = await _da.CalcularPronosticoAsync(campaniaId);
            await _da.GenerarNecesidadesAsync(p.Id);
            return RedirectToAction(nameof(Resultado), new { id = p.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Resultado(int id)
    {
        var p = await _da.ObtenerPronosticoAsync(id);
        if (p == null) return NotFound();
        var nec = await _da.ListarNecesidadesAsync(id);
        return View(new ResultadoMrpVM
        {
            PronosticoId = p.Id, Campania = p.Campania.Nombre,
            VolumenCosechaEstimado = p.VolumenCosechaEstimado, FechaCalculo = p.FechaCalculo,
            Necesidades = nec.Select(n => new NecesidadItemVM
            {
                Id = n.Id, Articulo = n.Articulo.Nombre,
                CantidadNetaSugerida = n.CantidadNetaSugerida, Estado = n.Estado.ToString()
            }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarACompras(int pronosticoId)
    {
        try
        {
            var ids = (await _da.ListarNecesidadesAsync(pronosticoId))
                .Where(n => n.Estado.ToString() == "Pendiente").Select(n => n.Id).ToList();
            if (!ids.Any()) throw new InvalidOperationException("No hay necesidades pendientes.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity!.Name!;
            var reqId = await _da.EnviarARequerimientoAsync(ids, userId);
            TempData["Ok"] = $"Requerimiento #{reqId} creado en Compras.";
        }
        catch (NotImplementedException)
        {
            TempData["Error"] = "Compras aún no implementó CrearRequerimientoDesdeMrp (pendiente: Changa).";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Resultado), new { id = pronosticoId });
    }
}
