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

    // GET: Mostrar formulario vacío
    public IActionResult Create()
    {
        // NOTA: En un proyecto real aquí cargaríamos ViewBag con Proveedores, Zonas y Usuarios.
        // Por ahora le pasamos el modelo vacío para que la vista no se caiga.
        return View(new CrearActivoVM());
    }

    // POST: Recibir los datos y guardarlos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearActivoVM modelo)
    {
        if (ModelState.IsValid)
        {
            var nuevoActivo = new AgroERP.Models.ActivosFijos.ActivoFijo
            {
                Codigo = modelo.Codigo,
                Nombre = modelo.Nombre,
                Categoria = modelo.Categoria,
                MarcaSerie = modelo.MarcaSerie,
                ProveedorId = modelo.ProveedorId,
                ZonaUbicacionId = modelo.ZonaUbicacionId,
                UsuarioResponsableId = modelo.UsuarioResponsableId,
                CostoAdquisicion = modelo.CostoAdquisicion,
                ValorResidual = modelo.ValorResidual,
                VidaUtilMeses = modelo.VidaUtilMeses
            };

            await _da.CrearActivoAsync(nuevoActivo);

            // Volvemos a la tabla para ver el activo creado
            return RedirectToAction(nameof(Index));
        }

        // Si hay errores de validación, devolvemos la misma vista con los mensajes
        return View(modelo);
    }

}