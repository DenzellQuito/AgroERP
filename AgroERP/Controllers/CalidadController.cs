using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
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

    // 1. LISTADO DE ÓRDENES DE COMPRA (Solo 'Emitidas')
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

    // 2. REGISTRAR LA RECEPCIÓN (Con Guía Manual)
    [HttpPost]
    public async Task<IActionResult> RegistrarRecepcion(int ordenCompraId, string guiaProveedor)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "USR-003";

            if (string.IsNullOrWhiteSpace(guiaProveedor))
            {
                TempData["Warning"] = "Debe ingresar el número de guía del proveedor.";
                return RedirectToAction(nameof(Index));
            }

            await _da.RegistrarRecepcion(ordenCompraId, guiaProveedor, userId);
            TempData["Success"] = "Recepción registrada. Los lotes están en Cuarentena.";
            return RedirectToAction(nameof(Inspeccionar));
        }
        catch (Exception ex)
        {
            TempData["Warning"] = "Error al recibir: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // 3. LISTADO DE LOTES EN CUARENTENA
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
                GuiaProveedor = d.Recepcion.GuiaProveedor // Mapeamos el nuevo dato
            })
            .ToListAsync();

        return View(detalles);
    }

    // 4. GUARDAR LA INSPECCIÓN (Con validación de motivo)
    [HttpPost]
    public async Task<IActionResult> GuardarInspeccion(int detalleId, bool esConforme, string motivoRechazo)
    {
        try
        {
            // VALIDACIÓN DE SEGURIDAD: Si rechaza, el motivo es obligatorio
            if (!esConforme && string.IsNullOrWhiteSpace(motivoRechazo))
            {
                TempData["Warning"] = "Debe ingresar un motivo para rechazar el lote.";
                return RedirectToAction(nameof(Inspeccionar));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "USR-003";
            await _da.InspeccionarLote(detalleId, esConforme, motivoRechazo, userId);

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