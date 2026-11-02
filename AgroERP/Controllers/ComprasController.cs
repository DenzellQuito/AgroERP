using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Compras;
using AgroERP.Models.Compras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroERP.Controllers;

[Authorize]
public class ComprasController : Controller
{
    private readonly DACompras _da;

    public ComprasController(DACompras da)
    {
        _da = da;
    }

    public async Task<IActionResult> Index()
    {
        var requerimientos = await _da.ObtenerRequerimientosAsync();

        var viewModel = requerimientos.Select(r => new RequerimientoIndexViewModel
        {
            Id = r.Id,
            FechaSolicitud = r.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"),
            Estado = r.Estado,
            CantidadArticulos = r.Detalles.Count
        }).ToList();

        return View(viewModel);
    }

    // ================= NUEVO CÓDIGO: CREACIÓN =================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Pasamos la lista de artículos a la vista para el menú desplegable
        ViewBag.Articulos = await _da.ObtenerArticulosAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(int articuloId, decimal cantidad)
    {
        // Tu ID fijo como 'changa' según la Regla 3 del README es "USR-002"
        var detalles = new List<RequerimientoDetalle>
        {
            new RequerimientoDetalle { ArticuloId = articuloId, Cantidad = cantidad }
        };

        await _da.CrearRequerimientoManualAsync("USR-002", detalles);

        // Volver a la pantalla principal
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var requerimiento = await _da.ObtenerRequerimientoPorIdAsync(id);

        if (requerimiento == null) return NotFound();

        var viewModel = new RequerimientoDetalleViewModel
        {
            Id = requerimiento.Id,
            FechaSolicitud = requerimiento.FechaSolicitud.ToString("dd/MM/yyyy HH:mm"),
            Estado = requerimiento.Estado,
            Articulos = requerimiento.Detalles.Select(d => new ItemRequerimientoViewModel
            {
                CodigoArticulo = d.Articulo.Codigo,
                NombreArticulo = d.Articulo.Nombre,
                Cantidad = d.Cantidad
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EnviarAprobacion(int id)
    {
        await _da.EnviarAprobacionAsync(id);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Proveedores()
    {
        var proveedores = await _da.ObtenerProveedoresAsync();
        var viewModel = proveedores.Select(p => new ProveedorViewModel
        {
            Ruc = p.Ruc,
            RazonSocial = p.RazonSocial,
            NivelConfiabilidad = p.NivelConfiabilidad
        }).ToList();

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateProveedor()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateProveedor(string ruc, string razonSocial, int nivelConfiabilidad)
    {
        var proveedor = new AgroERP.Models.Maestros.Proveedor
        {
            Ruc = ruc,
            RazonSocial = razonSocial,
            NivelConfiabilidad = nivelConfiabilidad
        };

        await _da.CrearProveedorAsync(proveedor);

        return RedirectToAction(nameof(Proveedores));
    }
}