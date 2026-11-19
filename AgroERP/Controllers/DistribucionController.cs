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

    // 1. GET: Listado
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
                Estado = p.EstadoLogistico,
                TieneStockReservado = _db.ReservasInventario.Any(r => r.PedidoVentaDetalle.PedidoId == p.Id && r.Estado == "Activa"),
                TieneFactura = _db.ComprobantesPago.Any(c => c.PedidoVentaId == p.Id),
                OrdenPickingId = _db.OrdenesPicking.Where(o => o.PedidoVentaId == p.Id).OrderByDescending(o => o.Id).Select(o => (int?)o.Id).FirstOrDefault(),
                GuiaRemisionId = _db.GuiasRemisionRemitente.Where(g => g.PedidoVentaId == p.Id).OrderByDescending(g => g.Id).Select(g => (int?)g.Id).FirstOrDefault(),
                ComprobantePagoId = _db.ComprobantesPago.Where(c => c.PedidoVentaId == p.Id).OrderByDescending(c => c.Id).Select(c => (int?)c.Id).FirstOrDefault(),
                UltimaNotaCreditoId = _db.NotaCredito.Where(n => n.ComprobantePago.PedidoVentaId == p.Id).OrderByDescending(n => n.Id).Select(n => (int?)n.Id).FirstOrDefault()
            })
            .ToListAsync();

        return View(pedidos);
    }

    // 2. GET: Crear
    public async Task<IActionResult> Crear()
    {
        await CargarCombos();
        return View(new PedidoViewModel());
    }

    // 3. POST: Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PedidoViewModel model)
    {
        if (!ModelState.IsValid) { await CargarCombos(); return View(model); }

        var cliente = await _db.Clientes.FindAsync(model.ClienteId);
        if (cliente == null) return NotFound("Cliente no encontrado");

        var articulo = await _db.Articulos.FindAsync(model.ArticuloId);
        if (articulo == null || articulo.Tipo != "Fresa")
        {
            ModelState.AddModelError("", "El artículo seleccionado no es válido (solo se venden fresas).");
            await CargarCombos();
            return View(model);
        }

        decimal total = model.Cantidad * model.PrecioUnitario;

        decimal deudaActual = await _db.CuentasPorCobrar
            .Where(c => c.ClienteId == model.ClienteId && c.Estado != "Cobrado")
            .SumAsync(c => (decimal?)c.SaldoPendiente) ?? 0m;
        decimal creditoDisponible = cliente.LimiteCredito - deudaActual;

        if (deudaActual + total > cliente.LimiteCredito)
        {
            ModelState.AddModelError("", $"El total ({total:C}) más la deuda vigente ({deudaActual:C}) supera el límite de crédito ({cliente.LimiteCredito:C}). Crédito disponible: {Math.Max(0m, creditoDisponible):C}.");
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
            TempData["Success"] = "Pedido creado y stock reservado. Listo para preparar.";
        }

        return RedirectToAction(nameof(Index));
    }

    // 4. POST: Marcar Entregado
    [HttpPost]
    public async Task<IActionResult> MarcarEntregado(int id)
    {
        var pedido = await _db.PedidosVenta.FindAsync(id);
        if (pedido == null) return NotFound();

        if (pedido.EstadoLogistico != "EnRuta")
        {
            TempData["Warning"] = "Solo se puede marcar como entregado un pedido que esté En Ruta.";
            return RedirectToAction(nameof(Index));
        }

        pedido.EstadoLogistico = "Entregado";
        pedido.FechaEntregaReal = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Pedido marcado como ENTREGADO.";
        return RedirectToAction(nameof(Index));
    }

    // 5. POST: Facturar
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

    // 6. GET: Devolución
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
                .Include(l => l.Articulo)
                .Where(l => l.Articulo.Tipo == "Fresa")
                .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = $"Lote: {l.NumeroLote}" }).ToListAsync()
        };
        return View(model);
    }

    // 7. POST: Devolución
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

    // 8. POST: Generar Picking
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

    // 9. GET: Despachar
    public async Task<IActionResult> Despachar(int id)
    {
        var pedido = await _db.PedidosVenta.FindAsync(id);
        if (pedido == null) return NotFound();
        ViewBag.PedidoId = id;
        return View();
    }

    // 10. POST: Despachar (CON PROTECCIÓN DE DATOS)
    [HttpPost]
    public async Task<IActionResult> Despachar(int id, string placa, string chofer)
    {
        var pedido = await _db.PedidosVenta.FindAsync(id);
        if (pedido == null) return NotFound();

        if (pedido.EstadoLogistico != "Picking")
        {
            TempData["Warning"] = "Solo se pueden despachar pedidos en estado Picking.";
            return RedirectToAction(nameof(Index));
        }

        var orden = await _db.OrdenesPicking
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync(o => o.PedidoVentaId == id);

        if (orden == null)
        {
            TempData["Warning"] = "El pedido no tiene una Orden de Picking válida. Vuelve a generarla.";
            return RedirectToAction(nameof(Index));
        }

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var guia = new GuiaRemisionRemitente
            {
                PedidoVentaId = id,
                PlacaVehiculo = placa,
                Chofer = chofer,
                MotivoTraslado = "Venta de mercadería",
                Estado = "Aceptada"
            };
            _db.GuiasRemisionRemitente.Add(guia);

            await _almacen.ConfirmarSalida(orden.Id);

            pedido.EstadoLogistico = "EnRuta";
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            TempData["Success"] = "Guía de Remisión emitida. Producto en ruta.";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Warning"] = "Error al despachar: " + ex.Message;

            // RECUPERAMOS LOS DATOS PARA QUE EL USUARIO NO LOS PIERDA
            ViewBag.PlacaAnterior = placa;
            ViewBag.ChoferAnterior = chofer;
            ViewBag.PedidoId = id;

            return View(); // Recargamos la misma vista
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task RecargarListasDevolucion(DevolucionViewModel model)
    {
        model.PedidosDisponibles = await _db.PedidosVenta
            .Where(p => p.EstadoLogistico == "Entregado" || p.EstadoLogistico == "Facturado")
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"Pedido #{p.Id} - {p.Cliente.RazonSocial}" })
            .ToListAsync();
        model.ArticulosDisponibles = await _db.Articulos.Where(a => a.Tipo == "Fresa")
            .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Nombre }).ToListAsync();
        model.LotesDisponibles = await _db.Lotes.Include(l => l.Articulo).Where(l => l.Articulo.Tipo == "Fresa")
            .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = $"Lote: {l.NumeroLote}" }).ToListAsync();
    }

    private async Task CargarCombos()
    {
        ViewBag.Clientes = await _db.Clientes.ToListAsync();
        ViewBag.Articulos = await _db.Articulos.Where(a => a.Tipo == "Fresa").ToListAsync();
    }

    // ===================== DOCUMENTOS PDF CONECTADOS =====================
    [HttpGet]
    public async Task<IActionResult> HojaPickingPdf(int id)
    {
        var orden = await _db.OrdenesPicking
            .Include(o => o.PedidoVenta).ThenInclude(p => p.Cliente)
            .Include(o => o.Detalles).ThenInclude(d => d.Lote).ThenInclude(l => l.Articulo)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (orden == null) return NotFound();

        var operario = await _db.Users
            .Where(u => u.Id == orden.UsuarioOperarioId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync() ?? orden.UsuarioOperarioId;

        var reservas = await _db.ReservasInventario
            .Include(r => r.ZonaUbicacion).ThenInclude(z => z.Almacen)
            .Where(r => r.PedidoVentaDetalle.PedidoId == orden.PedidoVentaId)
            .ToListAsync();

        var kardex = await _db.MovimientosKardex
            .Include(m => m.ZonaOrigen).ThenInclude(z => z!.Almacen)
            .Where(m => m.ReferenciaDocumento == $"PICK-{orden.Id}")
            .ToListAsync();

        var vm = new HojaPickingPdfVM
        {
            OrdenPickingId = orden.Id,
            PedidoVentaId = orden.PedidoVentaId,
            Cliente = orden.PedidoVenta.Cliente.RazonSocial,
            ClienteRuc = orden.PedidoVenta.Cliente.RucDni,
            FechaPedido = orden.PedidoVenta.FechaPedido,
            FechaPromesaEntrega = orden.PedidoVenta.FechaPromesaEntrega,
            EstadoPedido = orden.PedidoVenta.EstadoLogistico,
            Operario = operario,
            Items = orden.Detalles.Select(d =>
            {
                var k = kardex.FirstOrDefault(x => x.LoteId == d.LoteId && x.ZonaOrigen != null);
                var r = reservas.FirstOrDefault(x => x.LoteId == d.LoteId);
                string zona = k?.ZonaOrigen != null
                    ? $"{k.ZonaOrigen.Almacen.Nombre} / {k.ZonaOrigen.CodigoUbicacion}"
                    : r != null
                        ? $"{r.ZonaUbicacion.Almacen.Nombre} / {r.ZonaUbicacion.CodigoUbicacion}"
                        : "Por asignar";
                return new ItemPickingPdfVM
                {
                    Articulo = d.Lote.Articulo.Nombre,
                    Unidad = d.Lote.Articulo.UnidadMedida,
                    NumeroLote = d.Lote.NumeroLote,
                    FechaVencimiento = d.Lote.FechaVencimiento,
                    AlmacenZona = zona,
                    Cantidad = d.CantidadRecolectada
                };
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GuiaRemisionPdf(int id)
    {
        var guia = await _db.GuiasRemisionRemitente
            .Include(g => g.PedidoVenta).ThenInclude(p => p.Cliente)
            .Include(g => g.PedidoVenta).ThenInclude(p => p.Detalles).ThenInclude(d => d.Articulo)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (guia == null) return NotFound();

        var picking = await _db.OrdenesPicking
            .Include(o => o.Detalles).ThenInclude(d => d.Lote)
            .Where(o => o.PedidoVentaId == guia.PedidoVentaId)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        var vm = new GuiaRemisionPdfVM
        {
            GuiaId = guia.Id,
            PedidoVentaId = guia.PedidoVentaId,
            EstadoGuia = guia.Estado,
            Cliente = guia.PedidoVenta.Cliente.RazonSocial,
            ClienteRuc = guia.PedidoVenta.Cliente.RucDni,
            FechaPedido = guia.PedidoVenta.FechaPedido,
            FechaPromesaEntrega = guia.PedidoVenta.FechaPromesaEntrega,
            PlacaVehiculo = guia.PlacaVehiculo,
            Chofer = guia.Chofer,
            MotivoTraslado = guia.MotivoTraslado,
            Items = guia.PedidoVenta.Detalles.Select(d =>
            {
                var lotes = picking?.Detalles
                    .Where(pd => pd.Lote.ArticuloId == d.ArticuloId)
                    .Select(pd => $"{pd.Lote.NumeroLote} ({pd.CantidadRecolectada:N2})")
                    .ToList() ?? new List<string>();
                return new ItemGuiaPdfVM
                {
                    CodigoArticulo = d.Articulo.Codigo,
                    Articulo = d.Articulo.Nombre,
                    Unidad = d.Articulo.UnidadMedida,
                    Cantidad = d.Cantidad,
                    Lotes = lotes.Any() ? string.Join(", ", lotes) : "-"
                };
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> FacturaPdf(int id)
    {
        var cpe = await _db.ComprobantesPago
            .Include(c => c.PedidoVenta).ThenInclude(p => p.Cliente)
            .Include(c => c.PedidoVenta).ThenInclude(p => p.Detalles).ThenInclude(d => d.Articulo)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cpe == null) return NotFound();

        var cxc = await _db.CuentasPorCobrar.FirstOrDefaultAsync(c => c.ComprobantePagoId == cpe.Id);
        var guiaId = await _db.GuiasRemisionRemitente
            .Where(g => g.PedidoVentaId == cpe.PedidoVentaId)
            .OrderByDescending(g => g.Id)
            .Select(g => (int?)g.Id)
            .FirstOrDefaultAsync();

        var notas = await _db.NotaCredito
            .Where(n => n.ComprobantePagoId == cpe.Id)
            .OrderBy(n => n.Id)
            .Select(n => new { n.Id, n.Fecha, n.Monto })
            .ToListAsync();

        var neto = Math.Round(cpe.MontoTotal / 1.18m, 2, MidpointRounding.AwayFromZero);
        var vm = new FacturaPdfVM
        {
            ComprobanteId = cpe.Id,
            TipoCPE = cpe.TipoCPE,
            PedidoVentaId = cpe.PedidoVentaId,
            GuiaRemisionId = guiaId,
            Cliente = cpe.PedidoVenta.Cliente.RazonSocial,
            ClienteRuc = cpe.PedidoVenta.Cliente.RucDni,
            FechaEmision = cxc?.FechaEmision ?? cpe.PedidoVenta.FechaEntregaReal ?? cpe.PedidoVenta.FechaPedido,
            FechaVencimientoCxC = cxc?.FechaVencimiento,
            EstadoCxC = cxc?.Estado ?? "Emitida",
            SaldoPendienteCxC = cxc?.SaldoPendiente ?? cpe.MontoTotal,
            SubtotalNeto = neto,
            Igv = cpe.MontoTotal - neto,
            MontoTotal = cpe.MontoTotal,
            Items = cpe.PedidoVenta.Detalles.Select(d => new ItemFacturaPdfVM
            {
                CodigoArticulo = d.Articulo.Codigo,
                Descripcion = d.Articulo.Nombre,
                Unidad = d.Articulo.UnidadMedida,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = Math.Round(d.Cantidad * d.PrecioUnitario, 2, MidpointRounding.AwayFromZero)
            }).ToList(),
            NotasCredito = notas.Select(n => (n.Id, n.Fecha, n.Monto)).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> NotaCreditoPdf(int id)
    {
        var nc = await _db.NotaCredito
            .Include(n => n.Devolucion).ThenInclude(d => d.Articulo)
            .Include(n => n.Devolucion).ThenInclude(d => d.Lote)
            .Include(n => n.Devolucion).ThenInclude(d => d.PedidoVenta).ThenInclude(p => p.Cliente)
            .Include(n => n.ComprobantePago)
            .FirstOrDefaultAsync(n => n.Id == id);
        if (nc == null) return NotFound();

        var neto = Math.Round(nc.Monto / 1.18m, 2, MidpointRounding.AwayFromZero);
        var vm = new NotaCreditoPdfVM
        {
            NotaCreditoId = nc.Id,
            ComprobantePagoId = nc.ComprobantePagoId,
            PedidoVentaId = nc.Devolucion.PedidoVentaId,
            FechaEmision = nc.Fecha,
            Cliente = nc.Devolucion.PedidoVenta.Cliente.RazonSocial,
            ClienteRuc = nc.Devolucion.PedidoVenta.Cliente.RucDni,
            Articulo = nc.Devolucion.Articulo.Nombre,
            NumeroLote = nc.Devolucion.Lote.NumeroLote,
            CantidadDevuelta = nc.Devolucion.CantidadRechazada,
            Motivo = nc.Devolucion.Motivo,
            SubtotalNeto = neto,
            Igv = nc.Monto - neto,
            MontoTotal = nc.Monto
        };

        return View(vm);
    }
}