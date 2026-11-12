using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Activos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AgroERP.Controllers;

/// Responsable: Lavado
[Authorize]
public class ActivosController : Controller
{
    private readonly DAActivos _da;

    public ActivosController(DAActivos da) => _da = da;

    public async Task<IActionResult> Index()
    {
        // 1. Llamamos a tu método del Paso 2
        var activos = await _da.ObtenerActivosAsync();

        // 2. Llenamos el ViewModel exacto que creó Denzell
        var viewModel = activos.Select(a => new ActivoFijoItemVM
        {
            Id = a.Id,
            Codigo = a.Codigo,
            Nombre = a.Nombre,
            Categoria = a.Categoria,
            CostoAdquisicion = a.CostoAdquisicion,
            ValorNetoLibros = a.ValorNetoLibros,
            Estado = a.Estado
        }).ToList();

        // 3. Lo enviamos a la vista
        return View(viewModel);
    }
}