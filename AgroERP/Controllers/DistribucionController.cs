using System.Security.Claims;
using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.Models.Distribucion;
using AgroERP.ViewModels.Distribucion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Controllers;

/// Responsable: Díaz
[Authorize]
public class DistribucionController : Controller
{
    private readonly DADistribucion _da;
    private readonly DAAlmacen _almacen;
    private readonly ApplicationDbContext _db;

    public DistribucionController(DADistribucion da, DAAlmacen almacen, ApplicationDbContext db)
    {
        _da = da;
        _almacen = almacen;
        _db = db;
    }

    // 1. GET: Distribucion (Listado de Pedidos)
    public async Task<IActionResult> Index()
    {
        var pedidos = await _db.PedidosVenta
            .Include(p => p.Cliente)
            .OrderByDescending(p => p.FechaPedido)
            .Select(p => new PedidoListViewModel
            {
                Id = p.Id,
                Cliente = p.Cliente.RazonSocial,
                Fecha = p.FechaPedido,
                Total = p.Total,
                Estado = p.EstadoLogistico
            })
            .ToListAsync();

        return View(pedidos);
    }

    // 2. GET: Distribucion/Crear (Formulario)
    public async Task<IActionResult> Crear()
    {
        await CargarCombos();
        return View(new PedidoViewModel());
    }

    // 3. POST: Distribucion/Crear (Guardar Pedido)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PedidoViewModel model)
    {
        if (!ModelState.IsValid) { await CargarCombos(); return View(model); }

        var cliente = await _db.Clientes.FindAsync(model.ClienteId);
        if (cliente == null) return NotFound("Cliente no encontrado");

        // Validación: Solo fresas
        var articulo = await _db.Articulos.FindAsync(model.ArticuloId);
        if (articulo == null || articulo.Tipo != "Fresa")
        {
            ModelState.AddModelError("", "El artículo seleccionado no es válido (solo se venden fresas).");
            await CargarCombos();
            return View(model);
        }

        decimal total = model.Cantidad * model.PrecioUnitario;

        // Validación de crédito
        if (total > cliente.LimiteCredito)
        {
            ModelState.AddModelError("", $"El total ({total:C}) supera el límite de crédito ({cliente.LimiteCredito:C}).");
            await CargarCombos();
            return View(model);
        }

        var pedido = new PedidoVenta
        {
            ClienteId = model.ClienteId,
            FechaPedido = DateTime.Now,
            FechaPromesaEntrega = model.FechaPromesaEntrega,
            Total = total,
            EstadoLogistico = "Pendiente",
            Detalles = new List<PedidoVentaDetalle>
            {
                new PedidoVentaDetalle { ArticuloId = model.ArticuloId, Cantidad = model.Cantidad, PrecioUnitario = model.PrecioUnitario }
            }
        };

        _db.PedidosVenta.Add(pedido);
        await _db.SaveChangesAsync();

        bool reservado = await _da.IntentarReservarPedido(pedido.Id);

        if (!reservado)
        {
            TempData["Warning"] = "Pedido creado, pero no hay stock suficiente. Quedó en estado Pendiente.";
        }
        else
        {
            pedido.EstadoLogistico = "Picking";
            await _db.SaveChangesAsync();
            TempData["Success"] = "Pedido creado y stock reservado.";
        }

        return RedirectToAction(nameof(Index));
    }

    // 4. POST: Marcar Pedido como Entregado
    [HttpPost]
    public async Task<IActionResult> MarcarEntregado(int id)
    {
        var pedido = await _db.PedidosVenta.FindAsync(id);
        if (pedido != null)
        {
            pedido.EstadoLogistico = "Entregado";
            pedido.FechaEntregaReal = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Pedido marcado como ENTREGADO.";
        }
        return RedirectToAction(nameof(Index));
    }

    // 5. POST: Facturar Pedido
    [HttpPost]
    public async Task<IActionResult> Facturar(int id)
    {
        try
        {
            var yaFacturado = await _db.ComprobantesPago.AnyAsync(c => c.PedidoVentaId == id);
            if (yaFacturado)
            {
                TempData["Warning"] = "Este pedido YA FUE FACTURADO anteriormente.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "USR-005";
            await _da.FacturarPedido(id, userId);
            TempData["Success"] = "Factura generada y asiento contable registrado.";
        }
        catch (Exception ex)
        {
            TempData["Warning"] = "Error al facturar: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // 6. GET: Formulario de Devolución
    public async Task<IActionResult> Devolucion()
    {
        var model = new DevolucionViewModel
        {
            PedidosDisponibles = await _db.PedidosVenta
                .Where(p => p.EstadoLogistico == "Entregado" || p.EstadoLogistico == "Facturado")
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"Pedido #{p.Id} - {p.Cliente.RazonSocial}" })
                .ToListAsync(),
            ArticulosDisponibles = await _db.Articulos.Where(a => a.Tipo == "Fresa")
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Nombre }).ToListAsync(),
            LotesDisponibles = await _db.Lotes
                .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = $"Lote: {l.NumeroLote}" }).ToListAsync()
        };
        return View(model);
    }

    // 7. POST: Guardar Devolución
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolucion(DevolucionViewModel model)
    {
        if (!ModelState.IsValid) { await RecargarListasDevolucion(model); return View(model); }

        try
        {
            var detallePedido = await _db.DetallesPedidoVenta
                .FirstOrDefaultAsync(d => d.PedidoId == model.PedidoId && d.ArticuloId == model.ArticuloId);

            if (detallePedido == null)
            {
                ModelState.AddModelError("", "El artículo no pertenece al pedido original.");
                await RecargarListasDevolucion(model); return View(model);
            }

            if (model.CantidadRechazada > detallePedido.Cantidad)
            {
                ModelState.AddModelError("CantidadRechazada", $"No puedes devolver más de lo comprado (Máx: {detallePedido.Cantidad}).");
                await RecargarListasDevolucion(model); return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "USR-005";
            await _da.RegistrarDevolucion(model.PedidoId, model.ArticuloId, model.LoteId, model.CantidadRechazada, model.Motivo, userId);

            TempData["Success"] = "Devolución registrada. Se generó la Nota de Crédito.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al procesar la devolución: " + ex.Message);
            await RecargarListasDevolucion(model); return View(model);
        }
    }

    // 8. POST: Generar Orden de Picking
    [HttpPost]
    public async Task<IActionResult> GenerarPicking(int id)
    {
        var pedido = await _db.PedidosVenta.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null) return NotFound();

        if (await _db.OrdenesPicking.AnyAsync(o => o.PedidoVentaId == id)) return RedirectToAction(nameof(Index));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "USR-005";

        var orden = new OrdenPicking
        {
            PedidoVentaId = id,
            UsuarioOperarioId = userId,
            Detalles = new List<OrdenPickingDetalle>()
        };

        var reservas = await _db.ReservasInventario
            .Where(r => r.PedidoVentaDetalle.PedidoId == id && r.Estado == "Activa")
            .ToListAsync();

        foreach (var res in reservas)
        {
            orden.Detalles.Add(new OrdenPickingDetalle
            {
                LoteId = res.LoteId,
                CantidadRecolectada = res.Cantidad
            });
        }

        _db.OrdenesPicking.Add(orden);
        pedido.EstadoLogistico = "Picking";
        await _db.SaveChangesAsync();

        TempData["Success"] = "Orden de Picking generada.";
        return RedirectToAction(nameof(Index));
    }

    // 9. GET: Formulario de Despacho (Guía de Remisión)
    public async Task<IActionResult> Despachar(int id)
    {
        var pedido = await _db.PedidosVenta.FindAsync(id);
        if (pedido == null) return NotFound();
        ViewBag.PedidoId = id;
        return View();
    }

    // 10. POST: Registrar Guía de Remisión (PROTEGIDO CON TRANSACCIÓN)
    [HttpPost]
    public async Task<IActionResult> Despachar(int id, string placa, string chofer)
    {
        var pedido = await _db.PedidosVenta.FindAsync(id);
        if (pedido == null) return NotFound();

        var orden = await _db.OrdenesPicking.FirstOrDefaultAsync(o => o.PedidoVentaId == id);
        if (orden == null) return BadRequest("No existe orden de picking.");

        // Abrimos transacción para proteger todo
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // 1. Registrar la Guía de Remisión
            var guia = new GuiaRemisionRemitente
            {
                PedidoVentaId = id,
                PlacaVehiculo = placa,
                Chofer = chofer,
                MotivoTraslado = "Venta de mercadería",
                Estado = "Emitida"
            };
            _db.GuiasRemisionRemitente.Add(guia);

            // 2. Méndez intenta descontar el stock
            await _almacen.ConfirmarSalida(orden.Id);

            // 3. Si Méndez no falló, actualizamos el estado y guardamos
            pedido.EstadoLogistico = "EnRuta";
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            TempData["Success"] = "Guía de Remisión emitida. Producto en ruta.";
        }
        catch (Exception ex)
        {
            // Si Méndez falla (ej. falta stock), deshacemos TODO.
            await transaction.RollbackAsync();
            TempData["Warning"] = "Error al despachar: " + ex.Message;
            return RedirectToAction(nameof(Despachar), new { id = id });
        }

        return RedirectToAction(nameof(Index));
    }

    // --- MÉTODOS AUXILIARES ---

    private async Task RecargarListasDevolucion(DevolucionViewModel model)
    {
        model.PedidosDisponibles = await _db.PedidosVenta
            .Where(p => p.EstadoLogistico == "Entregado" || p.EstadoLogistico == "Facturado")
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"Pedido #{p.Id} - {p.Cliente.RazonSocial}" })
            .ToListAsync();
        model.ArticulosDisponibles = await _db.Articulos.Where(a => a.Tipo == "Fresa")
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Nombre }).ToListAsync();
        model.LotesDisponibles = await _db.Lotes
            .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = $"Lote: {l.NumeroLote}" }).ToListAsync();
    }

    private async Task CargarCombos()
    {
        ViewBag.Clientes = await _db.Clientes.ToListAsync();
        ViewBag.Articulos = await _db.Articulos.Where(a => a.Tipo == "Fresa").ToListAsync();
    }
}