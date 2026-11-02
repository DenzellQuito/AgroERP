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

    [HttpGet]
    public async Task<IActionResult> Cotizaciones(int id)
    {
        var req = await _da.ObtenerRequerimientoConCotizacionesAsync(id);
        if (req == null) return NotFound();

        ViewBag.Proveedores = await _da.ObtenerProveedoresAsync();

        var viewModel = new CotizacionIndexViewModel
        {
            RequerimientoId = req.Id,
            Estado = req.Estado,
            Ofertas = req.Cotizaciones.Select(c => new OfertaViewModel
            {
                Id = c.Id,
                Proveedor = c.Proveedor.RazonSocial,
                Precio = c.PrecioOfertado,
                Dias = c.DiasEntrega,
                EsGanador = c.EsGanador
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AgregarCotizacion(int requerimientoId, int proveedorId, decimal precio, int dias)
    {
        await _da.RegistrarCotizacionAsync(requerimientoId, proveedorId, precio, dias);
        return RedirectToAction(nameof(Cotizaciones), new { id = requerimientoId });
    }

    [HttpPost]
    public async Task<IActionResult> Adjudicar(int cotizacionId, int requerimientoId)
    {
        await _da.AdjudicarCotizacionAsync(cotizacionId, requerimientoId);
        return RedirectToAction(nameof(Cotizaciones), new { id = requerimientoId });
    }

    [HttpPost]
    public async Task<IActionResult> GenerarOrden(int requerimientoId)
    {
        await _da.GenerarOrdenDesdeCotizacionAsync(requerimientoId);
        return RedirectToAction(nameof(Ordenes));
    }

    [HttpGet]
    public async Task<IActionResult> Ordenes()
    {
        var ordenes = await _da.ObtenerOrdenesAsync();
        var viewModel = ordenes.Select(o => new OrdenCompraViewModel
        {
            Id = o.Id,
            Proveedor = o.Proveedor.RazonSocial,
            FechaEmision = o.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
            Total = o.Total,
            Estado = o.Estado
        }).ToList();

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GenerarPdf(int id)
    {
        var orden = await _da.ObtenerOrdenImpresionAsync(id);
        if (orden == null) return NotFound();

        var viewModel = new OrdenCompraImpresionViewModel
        {
            Id = orden.Id,
            FechaEmision = orden.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
            ProveedorRuc = orden.Proveedor.Ruc,
            ProveedorRazonSocial = orden.Proveedor.RazonSocial,
            Total = orden.Total,
            Detalles = orden.Detalles.Select(d => new ItemImpresionViewModel
            {
                Codigo = d.Articulo.Codigo,
                Descripcion = d.Articulo.Nombre,
                Cantidad = d.CantidadPedida,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.CantidadPedida * d.PrecioUnitario
            }).ToList()
        };

        return View(viewModel);
    }
}