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

        ViewBag.RecepcionesRecientes = await _db.RecepcionesMercaderia
            .Include(r => r.OrdenCompra).ThenInclude(o => o.Proveedor)
            .OrderByDescending(r => r.FechaRecepcion)
            .Take(25)
            .Select(r => new RecepcionHistorialItemVM
            {
                RecepcionId = r.Id,
                OrdenCompraId = r.OrdenCompraId,
                GuiaProveedor = r.GuiaProveedor,
                Proveedor = r.OrdenCompra.Proveedor.RazonSocial,
                FechaRecepcion = r.FechaRecepcion,
                Estado = r.Estado
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarRecepcion(RegistrarRecepcionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var oc = await _db.OrdenesCompra.Include(o => o.Detalles).ThenInclude(d => d.Articulo).FirstOrDefaultAsync(o => o.Id == model.OrdenCompraId);
            if (oc != null)
            {
                for (int i = 0; i < model.Detalles.Count; i++)
                {
                    var detOc = oc.Detalles.FirstOrDefault(d => d.ArticuloId == model.Detalles[i].ArticuloId);
                    model.Detalles[i].NombreArticulo = detOc?.Articulo.Nombre ?? string.Empty;
                }
            }
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarInspeccion(int detalleId, bool esConforme, string? motivoRechazo, int zonaId = 0)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            if (esConforme && zonaId <= 0)
            {
                TempData["Warning"] = "Debe seleccionar una zona para liberar el lote.";
                return RedirectToAction(nameof(Inspeccionar));
            }

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

    [HttpGet]
    public async Task<IActionResult> ReporteRecepcionPdf(int id)
    {
        var rec = await _db.RecepcionesMercaderia
            .Include(r => r.OrdenCompra).ThenInclude(o => o.Proveedor)
            .Include(r => r.Detalles).ThenInclude(d => d.Articulo)
            .Include(r => r.Detalles).ThenInclude(d => d.Lote)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rec == null) return NotFound();

        var loteIds = rec.Detalles.Select(d => d.LoteId).ToList();
        var inspecciones = await _db.InspeccionesCalidad
            .Where(i => loteIds.Contains(i.LoteId))
            .ToListAsync();

        var usuarios = await _db.Users.ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id);
        string NombreUsr(string? uId) => uId != null && usuarios.TryGetValue(uId, out var n) ? n : (uId ?? "-");

        var vm = new ReporteRecepcionCalidadPdfVM
        {
            RecepcionId = rec.Id,
            OrdenCompraId = rec.OrdenCompraId,
            Proveedor = rec.OrdenCompra.Proveedor.RazonSocial,
            ProveedorRuc = rec.OrdenCompra.Proveedor.Ruc,
            GuiaProveedor = rec.GuiaProveedor,
            FechaRecepcion = rec.FechaRecepcion,
            Receptor = NombreUsr(rec.UsuarioReceptorId),
            EstadoRecepcion = rec.Estado,
            EstadoOrden = rec.OrdenCompra.Estado,
            Detalles = rec.Detalles.Select(d =>
            {
                var insp = inspecciones.LastOrDefault(i => i.LoteId == d.LoteId);
                return new DetalleReporteCalidadVM
                {
                    Articulo = d.Articulo.Nombre,
                    NumeroLote = d.Lote.NumeroLote,
                    FechaVencimiento = d.Lote.FechaVencimiento,
                    CantidadPedida = d.CantidadPedida,
                    CantidadGuia = d.CantidadGuia,
                    CantidadRecibida = d.CantidadRecibida,
                    EstadoCalidad = d.Lote.EstadoCalidad,
                    MotivoRechazo = insp?.MotivoRechazo,
                    Inspector = insp != null ? NombreUsr(insp.UsuarioInspectorId) : "-"
                };
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ReportePorOrdenPdf(int ordenCompraId)
    {
        var recId = await _db.RecepcionesMercaderia
            .Where(r => r.OrdenCompraId == ordenCompraId)
            .OrderByDescending(r => r.Id)
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync();

        if (!recId.HasValue)
        {
            TempData["Warning"] = $"La orden OC-{ordenCompraId:D4} aún no tiene una recepción registrada.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(ReporteRecepcionPdf), new { id = recId.Value });
    }
}