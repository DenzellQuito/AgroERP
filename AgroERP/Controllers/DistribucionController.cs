using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.Models.Distribucion;
using AgroERP.ViewModels.Distribucion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Controllers;

/// Responsable: Díaz
[Authorize]
public class DistribucionController : Controller
{
    private readonly DADistribucion _da;
    private readonly ApplicationDbContext _db;

    public DistribucionController(DADistribucion da, ApplicationDbContext db)
    {
        _da = da;
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
        if (!ModelState.IsValid)
        {
            await CargarCombos();
            return View(model);
        }

        var cliente = await _db.Clientes.FindAsync(model.ClienteId);
        if (cliente == null) return NotFound("Cliente no encontrado");

        decimal total = model.Cantidad * model.PrecioUnitario;

        // Validación de límite de crédito (Regla de tu módulo)
        if (total > cliente.LimiteCredito)
        {
            ModelState.AddModelError("", $"El total ({total:C}) supera el límite de crédito del cliente ({cliente.LimiteCredito:C}).");
            await CargarCombos();
            return View(model);
        }

        // Crear el Pedido
        var pedido = new PedidoVenta
        {
            ClienteId = model.ClienteId,
            FechaPedido = DateTime.Now,
            FechaPromesaEntrega = model.FechaPromesaEntrega,
            Total = total,
            EstadoLogistico = "Pendiente",
            Detalles = new List<PedidoVentaDetalle>
            {
                new PedidoVentaDetalle
                {
                    ArticuloId = model.ArticuloId,
                    Cantidad = model.Cantidad,
                    PrecioUnitario = model.PrecioUnitario
                }
            }
        };

        _db.PedidosVenta.Add(pedido);
        await _db.SaveChangesAsync(); // Guardamos para generar el ID

        // Intentar reservar stock llamando a Méndez (DAAlmacen)
        bool reservado = await _da.IntentarReservarPedido(pedido.Id);

        if (!reservado)
        {
            TempData["Warning"] = "Pedido creado, pero no hay stock suficiente. Quedó en estado Pendiente.";
        }
        else
        {
            pedido.EstadoLogistico = "Picking"; // Si hay stock, avanza de estado
            await _db.SaveChangesAsync();
            TempData["Success"] = "Pedido creado y stock reservado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Método auxiliar para llenar los desplegables de la vista
    private async Task CargarCombos()
    {
        ViewBag.Clientes = await _db.Clientes.ToListAsync();
        // Trae todos los artículos sin filtro para evitar que el combo salga vacío
        ViewBag.Articulos = await _db.Articulos.ToListAsync();
    }
}