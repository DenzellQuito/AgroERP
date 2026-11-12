using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.Models.Calidad_WMS;
using AgroERP.Models.Maestros;
using AgroERP.ViewModels.Activos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgroERP.Controllers;

/// Responsable: Lavado
[Authorize]
public class ActivosController : Controller
{
    private readonly DAActivos _da;

    public ActivosController(DAActivos da) => _da = da;

    private static async Task CargarCombosAsync(Controller ctrl, ApplicationDbContext db, CrearActivoVM? vm = null)
    {
        var proveedores = await db.Set<Proveedor>()
            .OrderBy(p => p.RazonSocial)
            .Select(p => new { p.Id, p.RazonSocial })
            .ToListAsync();

        var zonas = await db.Set<ZonaUbicacion>()
            .Include(z => z.Almacen)
            .Select(z => new { z.Id, Nombre = z.Almacen.Nombre + " - " + z.CodigoUbicacion })
            .ToListAsync();

        ctrl.ViewBag.Proveedores = new SelectList(proveedores, "Id", "RazonSocial", vm?.ProveedorId);
        ctrl.ViewBag.Zonas = new SelectList(zonas, "Id", "Nombre", vm?.ZonaUbicacionId);
    }

    public async Task<IActionResult> Index()
    {
        var activos = await _da.ObtenerActivosAsync();

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

        return View(viewModel);
    }

    // GET: Mostrar formulario con desplegables reales
    public async Task<IActionResult> Create([FromServices] ApplicationDbContext db)
    {
        await CargarCombosAsync(this, db);
        return View(new CrearActivoVM { UsuarioResponsableId = "USR-004" });
    }

    // POST: Recibir los datos y guardarlos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearActivoVM modelo, [FromServices] ApplicationDbContext db)
    {
        if (!modelo.ProveedorId.HasValue)
            ModelState.AddModelError(nameof(modelo.ProveedorId), "Seleccione el proveedor del activo para poder capitalizarlo.");

        if (!ModelState.IsValid)
        {
            await CargarCombosAsync(this, db, modelo);
            return View(modelo);
        }

        try
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
            TempData["MensajeExito"] = $"Ficha del activo {nuevoActivo.Codigo} registrada en estado 'En Proceso'.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await CargarCombosAsync(this, db, modelo);
            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Capitalizar(int id)
    {
        try
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "USR-001";
            string nroFactura = $"FACT-AF-{id}-{DateTime.Now:MMddHHmm}";

            await _da.CapitalizarActivoAsync(id, nroFactura, usuarioId);

            TempData["MensajeExito"] = "¡Activo capitalizado! Se envió la factura a Cuentas por Pagar (Pagos) y se generó el asiento contable.";
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = $"Error al capitalizar: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EjecutarDepreciacion(int id)
    {
        try
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "USR-001";

            await _da.DepreciarActivoAsync(id, usuarioId);

            TempData["MensajeExito"] = "Depreciación mensual ejecutada y contabilizada correctamente.";
        }
        catch (Exception ex)
        {
            TempData["MensajeError"] = $"Error en depreciación: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}