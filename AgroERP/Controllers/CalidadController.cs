using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.Models.Calidad_WMS;
using AgroERP.ViewModels.Calidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgroERP.Controllers;

[Authorize]
public class CalidadController : Controller
{
    private readonly DACalidad _da;
    private readonly ApplicationDbContext _db;

    public CalidadController(DACalidad da, ApplicationDbContext db)
    {
        _da = da;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var ordenes = await _db.OrdenesCompra
            .Include(o => o.Proveedor)
            .Where(o => o.Estado == "Emitida")
            .OrderByDescending(o => o.FechaEmision)
            .Select(o => new RecepcionListViewModel
            {
                OrdenCompraId = o.Id,
                Proveedor = o.Proveedor.RazonSocial,
                FechaEmision = o.FechaEmision,
                Total = o.Total
            })
            .ToListAsync();

        return View(ordenes);
    }

    // GET: Formulario 3-Way Matching
    public async Task<IActionResult> RegistrarRecepcion(int id)
    {
        var oc = await _db.OrdenesCompra.Include(o => o.Detalles).ThenInclude(d => d.Articulo).FirstOrDefaultAsync(o => o.Id == id);
        if (oc == null) return NotFound();

        var model = new RegistrarRecepcionViewModel
        {
            OrdenCompraId = oc.Id,
            Detalles = oc.Detalles.Select(d => new DetalleRecepcionViewModel
            {
                ArticuloId = d.ArticuloId,
                NombreArticulo = d.Articulo.Nombre,
                CantidadPedida = d.CantidadPedida,
                CantidadGuia = d.CantidadPedida, // Precargado
                CantidadRecibida = d.CantidadPedida, // Precargado
                FechaVencimiento = DateTime.Now.AddMonths(6)
            }).ToList()
        };

        return View(model);
    }

    // POST: Guardar Recepción
    [HttpPost]
    public async Task<IActionResult> RegistrarRecepcion(RegistrarRecepcionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var oc = await _db.OrdenesCompra.Include(o => o.Detalles).ThenInclude(d => d.Articulo).FirstOrDefaultAsync(o => o.Id == model.OrdenCompraId);
            for (int i = 0; i < model.Detalles.Count; i++)
                model.Detalles[i].NombreArticulo = oc.Detalles.ElementAt(i).Articulo.Nombre;
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            await _da.RegistrarRecepcion(model, userId);

            var hayDiferencias = model.Detalles.Any(d => d.CantidadGuia != d.CantidadPedida || d.CantidadRecibida != d.CantidadGuia);
            if (hayDiferencias) TempData["Warning"] = "Recepción registrada con diferencias en las cantidades. Revise el 3-way matching.";
            else TempData["Success"] = "Recepción registrada conforme.";

            return RedirectToAction(nameof(Inspeccionar));
        }
        catch (Exception ex)
        {
            TempData["Warning"] = "Error al recibir: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Inspeccionar()
    {
        var detalles = await _db.DetallesRecepcion
            .Include(d => d.Lote).ThenInclude(l => l.Articulo)
            .Include(d => d.Recepcion).ThenInclude(r => r.OrdenCompra).ThenInclude(o => o.Proveedor)
            .Where(d => d.Lote.EstadoCalidad == "Cuarentena")
            .Select(d => new InspeccionListViewModel
            {
                DetalleId = d.Id,
                NumeroLote = d.Lote.NumeroLote,
                Articulo = d.Lote.Articulo.Nombre,
                CantidadRecibida = d.CantidadRecibida,
                Proveedor = d.Recepcion.OrdenCompra.Proveedor.RazonSocial,
                GuiaProveedor = d.Recepcion.GuiaProveedor
            })
            .ToListAsync();

        ViewBag.Zonas = await _db.ZonasUbicacion.ToListAsync();
        return View(detalles);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarInspeccion(int detalleId, bool esConforme, string motivoRechazo, int zonaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            if (!esConforme && string.IsNullOrWhiteSpace(motivoRechazo))
            {
                TempData["Warning"] = "Debe ingresar un motivo para rechazar el lote.";
                return RedirectToAction(nameof(Inspeccionar));
            }

            await _da.InspeccionarLote(detalleId, esConforme, motivoRechazo, zonaId, userId);

            if (esConforme) TempData["Success"] = "Lote LIBERADO. Stock ingresado y asiento contable generado.";
            else TempData["Warning"] = "Lote RECHAZADO. No se ingresó stock.";
        }
        catch (Exception ex)
        {
            TempData["Warning"] = "Error: " + ex.Message;
        }
        return RedirectToAction(nameof(Inspeccionar));
    }
}