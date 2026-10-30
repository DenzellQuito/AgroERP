using AgroERP.Data.AccesoDatos;
using AgroERP.ViewModels.Almacen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgroERP.Controllers;

/// <summary>Responsable: Méndez. Solo administra las tablas asignadas a Almacén.</summary>
[Authorize]
public class AlmacenController : Controller
{
    private readonly DAAlmacen _da;
    public AlmacenController(DAAlmacen da) => _da = da;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var almacenes = await _da.ObtenerAlmacenesAsync();
        var zonas = await _da.ObtenerZonasAsync();
        var existencias = await _da.ObtenerInventarioAsync();
        var movimientos = await _da.ObtenerMovimientosAsync();
        var picking = await _da.ObtenerOrdenesPickingAsync();
        var vm = new AlmacenDashboardViewModel
        {
            Almacenes = almacenes.Select(x => new AlmacenItemViewModel(x.Id, x.Nombre, x.Tipo, x.Zonas.Count)).ToList(),
            Zonas = zonas.Select(x => new ZonaItemViewModel(x.Id, x.AlmacenId, x.Almacen.Nombre, x.CodigoUbicacion)).ToList(),
            Existencias = existencias.Select(x => new ExistenciaItemViewModel(x.LoteId, x.Lote.Articulo.Nombre,
                x.Lote.NumeroLote, x.Lote.FechaVencimiento, x.Lote.EstadoCalidad, x.ZonaUbicacionId,
                x.ZonaUbicacion.CodigoUbicacion, x.ZonaUbicacion.Almacen.Nombre, x.CantidadFisica,
                x.CantidadReservada, x.CantidadFisica - x.CantidadReservada)).ToList(),
            Movimientos = movimientos.Select(x => new MovimientoItemViewModel(x.Fecha, x.Lote.Articulo.Nombre,
                x.Lote.NumeroLote, x.TipoMovimiento, x.Cantidad, x.ZonaOrigen?.CodigoUbicacion,
                x.ZonaDestino?.CodigoUbicacion, x.ReferenciaDocumento)).ToList(),
            OrdenesPicking = picking.Select(x => new PickingItemViewModel(x.Id, x.PedidoVentaId,
                x.UsuarioOperarioId, x.Detalles.Count, x.Detalles.Sum(d => d.CantidadRecolectada))).ToList()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearAlmacen(AlmacenFormViewModel model)
    {
        if (!ModelState.IsValid) return await IndexConError("Revisa los datos del almacén.");
        try { await _da.RegistrarAlmacenAsync(model.Nombre, model.Tipo); TempData["Ok"] = "Almacén registrado correctamente."; }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearZona(ZonaFormViewModel model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Completa los datos de la ubicación."; return RedirectToAction(nameof(Index)); }
        try { await _da.RegistrarZonaAsync(model.AlmacenId, model.CodigoUbicacion); TempData["Ok"] = "Ubicación registrada correctamente."; }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Trasladar(TrasladoFormViewModel model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Revisa los datos del traslado."; return RedirectToAction(nameof(Index)); }
        try { await _da.RegistrarTrasladoAsync(model.LoteId, model.ZonaOrigenId, model.ZonaDestinoId, model.Cantidad, model.Referencia); TempData["Ok"] = "Traslado registrado en Kardex."; }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarMerma(MermaFormViewModel model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Revisa los datos de la merma."; return RedirectToAction(nameof(Index)); }
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) throw new InvalidOperationException("No se pudo identificar al usuario autenticado.");
            await _da.RegistrarMermaAsync(model.LoteId, model.ZonaId, model.Cantidad, model.CostoUnitario, usuarioId, model.Motivo);
            TempData["Ok"] = "Merma registrada; inventario, Kardex y asiento contable procesados.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> IndexConError(string mensaje)
    {
        TempData["Error"] = mensaje;
        return await Index();
    }
}
