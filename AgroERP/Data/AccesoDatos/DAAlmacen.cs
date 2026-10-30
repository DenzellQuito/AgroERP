using AgroERP.Models.Calidad_WMS;
using AgroERP.Models.Contabilidad;
using AgroERP.Models.Distribucion;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// <summary>
/// DUEÑO: Méndez. Tablas que escribe: Almacenes, ZonasUbicacion,
/// InventariosExistencia, ReservasInventario, MovimientosKardex y AjustesInventario.
/// Los métodos de contrato NO ejecutan SaveChangesAsync: el módulo llamador guarda
/// la operación completa una sola vez.
/// </summary>
public class DAAlmacen
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;

    public DAAlmacen(ApplicationDbContext db, DAContabilidad contabilidad)
    {
        _db = db;
        _contabilidad = contabilidad;
    }

    // Contrato de Lavado: sumar stock al liberar un lote.
    public async Task RegistrarIngreso(int loteId, int zonaId, decimal cantidad, string referencia)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");
        var lote = await _db.Lotes.SingleOrDefaultAsync(x => x.Id == loteId)
            ?? throw new InvalidOperationException("No existe el lote indicado.");
        if (lote.EstadoCalidad != "Liberado")
            throw new InvalidOperationException("Solo se puede ingresar inventario de lotes con estado Liberado.");
        if (!await _db.ZonasUbicacion.AnyAsync(x => x.Id == zonaId))
            throw new InvalidOperationException("La zona de ubicación no existe.");

        var existencia = await _db.InventariosExistencia
            .SingleOrDefaultAsync(x => x.LoteId == loteId && x.ZonaUbicacionId == zonaId);
        if (existencia == null)
            _db.InventariosExistencia.Add(new InventarioExistencia
            {
                LoteId = loteId, ZonaUbicacionId = zonaId,
                CantidadFisica = cantidad, CantidadReservada = 0
            });
        else existencia.CantidadFisica += cantidad;

        _db.MovimientosKardex.Add(new MovimientoKardex
        {
            LoteId = loteId, Fecha = DateTime.Now, TipoMovimiento = "Ingreso",
            Cantidad = cantidad, ZonaDestinoId = zonaId,
            ReferenciaDocumento = referencia
        });
    }

    // Contrato de Díaz: reserva por FEFO. Devuelve false si el stock disponible no alcanza.
    public async Task<bool> ReservarStock(int pedidoVentaDetalleId, int articuloId, decimal cantidad)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
        var detalle = await _db.DetallesPedidoVenta.SingleOrDefaultAsync(x => x.Id == pedidoVentaDetalleId)
            ?? throw new InvalidOperationException("No existe el detalle del pedido.");
        if (detalle.ArticuloId != articuloId)
            throw new InvalidOperationException("El artículo no corresponde al detalle del pedido.");

        var activas = await _db.ReservasInventario
            .Where(x => x.PedidoVentaDetalleId == pedidoVentaDetalleId && x.Estado == "Activa")
            .SumAsync(x => (decimal?)x.Cantidad) ?? 0m;
        var pendiente = cantidad - activas;
        if (pendiente <= 0) return true;

        var existencias = await _db.InventariosExistencia
            .Include(x => x.Lote)
            .Where(x => x.Lote.ArticuloId == articuloId && x.Lote.EstadoCalidad == "Liberado")
            .OrderBy(x => x.Lote.FechaVencimiento)
            .ThenBy(x => x.LoteId)
            .ToListAsync();
        var disponibleTotal = existencias.Sum(x => x.CantidadFisica - x.CantidadReservada);
        if (disponibleTotal < pendiente) return false;

        foreach (var ex in existencias)
        {
            var disponible = ex.CantidadFisica - ex.CantidadReservada;
            if (disponible <= 0) continue;
            var tomar = Math.Min(disponible, pendiente);
            ex.CantidadReservada += tomar;
            _db.ReservasInventario.Add(new ReservaInventario
            {
                PedidoVentaDetalleId = pedidoVentaDetalleId,
                LoteId = ex.LoteId,
                ZonaUbicacionId = ex.ZonaUbicacionId,
                Cantidad = tomar,
                Estado = "Activa"
            });
            pendiente -= tomar;
            if (pendiente == 0) break;
        }
        return pendiente == 0;
    }

    // Contrato de Díaz: liberar todas las reservas activas de un pedido.
    public async Task LiberarReserva(int pedidoVentaId)
    {
        var reservas = await _db.ReservasInventario
            .Include(x => x.PedidoVentaDetalle)
            .Where(x => x.PedidoVentaDetalle.PedidoId == pedidoVentaId && x.Estado == "Activa")
            .ToListAsync();
        foreach (var reserva in reservas)
        {
            var ex = await _db.InventariosExistencia.SingleOrDefaultAsync(x =>
                x.LoteId == reserva.LoteId && x.ZonaUbicacionId == reserva.ZonaUbicacionId);
            if (ex == null || ex.CantidadReservada < reserva.Cantidad)
                throw new InvalidOperationException("La existencia reservada no coincide; se canceló la liberación para evitar stock negativo.");
            ex.CantidadReservada -= reserva.Cantidad;
            reserva.Estado = "Liberada";
        }
    }

    // Contrato de Díaz: salida física de lo preparado; consume reservas por pedido y lote.
    public async Task ConfirmarSalida(int ordenPickingId)
    {
        var orden = await _db.OrdenesPicking
            .Include(x => x.Detalles)
            .SingleOrDefaultAsync(x => x.Id == ordenPickingId)
            ?? throw new InvalidOperationException("No existe la orden de picking.");

        foreach (var item in orden.Detalles)
        {
            if (item.CantidadRecolectada <= 0) throw new InvalidOperationException("La cantidad recolectada debe ser mayor que cero.");
            var reservas = await _db.ReservasInventario
                .Include(x => x.PedidoVentaDetalle)
                .Where(x => x.PedidoVentaDetalle.PedidoId == orden.PedidoVentaId
                    && x.LoteId == item.LoteId && x.Estado == "Activa")
                .OrderBy(x => x.Id).ToListAsync();
            var reservado = reservas.Sum(x => x.Cantidad);
            if (reservado < item.CantidadRecolectada)
                throw new InvalidOperationException($"No existe reserva suficiente para el lote {item.LoteId}.");

            var porConsumir = item.CantidadRecolectada;
            foreach (var reserva in reservas)
            {
                if (porConsumir <= 0) break;
                var ex = await _db.InventariosExistencia.SingleAsync(x =>
                    x.LoteId == reserva.LoteId && x.ZonaUbicacionId == reserva.ZonaUbicacionId);
                var aplicar = Math.Min(reserva.Cantidad, porConsumir);
                if (ex.CantidadFisica < aplicar || ex.CantidadReservada < aplicar)
                    throw new InvalidOperationException("El stock físico o reservado es insuficiente para confirmar la salida.");
                ex.CantidadFisica -= aplicar;
                ex.CantidadReservada -= aplicar;
                reserva.Cantidad -= aplicar;
                porConsumir -= aplicar;
                if (reserva.Cantidad == 0) reserva.Estado = "Consumida";
                _db.MovimientosKardex.Add(new MovimientoKardex
                {
                    LoteId = item.LoteId, Fecha = DateTime.Now, TipoMovimiento = "Salida",
                    Cantidad = aplicar, ZonaOrigenId = ex.ZonaUbicacionId,
                    ReferenciaDocumento = $"PICK-{ordenPickingId}"
                });
            }
        }
    }

    // Contrato de Díaz. El modelo actual solo guarda CantidadRechazada y Motivo;
    // por seguridad se registra como merma rechazada, no como producto conforme.
    // La clasificación de una devolución buena requiere ampliar el modelo (con aprobación de Quito).
    public async Task ReingresarDevolucion(int devolucionId)
    {
        var devolucion = await _db.DevolucionesCliente
            .Include(x => x.Lote)
            .SingleOrDefaultAsync(x => x.Id == devolucionId)
            ?? throw new InvalidOperationException("No existe la devolución indicada.");
        if (devolucion.CantidadRechazada <= 0) throw new InvalidOperationException("La cantidad devuelta debe ser mayor que cero.");
        var zona = await _db.InventariosExistencia
            .Where(x => x.LoteId == devolucion.LoteId)
            .Select(x => (int?)x.ZonaUbicacionId).FirstOrDefaultAsync();
        if (zona == null)
            zona = await _db.ZonasUbicacion.Select(x => (int?)x.Id).FirstOrDefaultAsync();
        if (zona == null) throw new InvalidOperationException("Registra al menos una zona antes de procesar devoluciones.");
        var usuarioId = await _db.OrdenesPicking
            .Where(x => x.PedidoVentaId == devolucion.PedidoVentaId)
            .OrderByDescending(x => x.Id).Select(x => x.UsuarioOperarioId).FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new InvalidOperationException("No se puede identificar al responsable de la devolución. Debe vincularse la devolución con un operario para registrar la merma.");

        _db.AjustesInventario.Add(new AjusteInventario
        {
            LoteId = devolucion.LoteId, ZonaUbicacionId = zona.Value,
            UsuarioId = usuarioId, Fecha = DateTime.Now, TipoAjuste = "DevolucionPendienteValorizacion",
            Cantidad = devolucion.CantidadRechazada, CostoAsumido = 0
        });
        _db.MovimientosKardex.Add(new MovimientoKardex
        {
            LoteId = devolucion.LoteId, Fecha = DateTime.Now,
            TipoMovimiento = "DevolucionPendienteValorizacion", Cantidad = devolucion.CantidadRechazada,
            ZonaDestinoId = zona.Value, ReferenciaDocumento = $"DEV-{devolucionId}"
        });
        // El modelo DevolucionCliente no separa cantidad conforme/dañada ni guarda costo.
        // Por eso queda como incidencia pendiente: no se incorpora a stock disponible ni se
        // contabiliza sin valoración. Quito debe ampliar el modelo para cerrar esa parte del flujo.
    }

    // Consultas para el módulo web.
    public Task<List<Almacen>> ObtenerAlmacenesAsync() => _db.Almacenes.Include(x => x.Zonas).OrderBy(x => x.Nombre).ToListAsync();
    public Task<List<ZonaUbicacion>> ObtenerZonasAsync() => _db.ZonasUbicacion.Include(x => x.Almacen).OrderBy(x => x.Almacen.Nombre).ThenBy(x => x.CodigoUbicacion).ToListAsync();
    public Task<List<InventarioExistencia>> ObtenerInventarioAsync() => _db.InventariosExistencia
        .Include(x => x.Lote).ThenInclude(x => x.Articulo)
        .Include(x => x.ZonaUbicacion).ThenInclude(x => x.Almacen)
        .OrderBy(x => x.Lote.FechaVencimiento).ToListAsync();
    public Task<List<MovimientoKardex>> ObtenerMovimientosAsync() => _db.MovimientosKardex
        .Include(x => x.Lote).ThenInclude(x => x.Articulo)
        .Include(x => x.ZonaOrigen).Include(x => x.ZonaDestino)
        .OrderByDescending(x => x.Fecha).Take(200).ToListAsync();
    public Task<List<OrdenPicking>> ObtenerOrdenesPickingAsync() => _db.OrdenesPicking
        .Include(x => x.PedidoVenta).ThenInclude(x => x.Detalles).ThenInclude(x => x.Articulo)
        .Include(x => x.Detalles).ThenInclude(x => x.Lote).ThenInclude(x => x.Articulo)
        .OrderByDescending(x => x.Id).Take(100).ToListAsync();

    public async Task RegistrarAlmacenAsync(string nombre, string tipo)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new InvalidOperationException("El nombre es obligatorio.");
        if (tipo is not ("Insumos" or "AcopioFresa")) throw new InvalidOperationException("Tipo de almacén inválido.");
        _db.Almacenes.Add(new Almacen { Nombre = nombre.Trim(), Tipo = tipo });
        await _db.SaveChangesAsync();
    }

    public async Task RegistrarZonaAsync(int almacenId, string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new InvalidOperationException("El código de ubicación es obligatorio.");
        if (!await _db.Almacenes.AnyAsync(x => x.Id == almacenId)) throw new InvalidOperationException("El almacén no existe.");
        if (await _db.ZonasUbicacion.AnyAsync(x => x.AlmacenId == almacenId && x.CodigoUbicacion == codigo.Trim()))
            throw new InvalidOperationException("Ya existe esa ubicación en el almacén.");
        _db.ZonasUbicacion.Add(new ZonaUbicacion { AlmacenId = almacenId, CodigoUbicacion = codigo.Trim() });
        await _db.SaveChangesAsync();
    }

    public async Task RegistrarTrasladoAsync(int loteId, int zonaOrigenId, int zonaDestinoId, decimal cantidad, string referencia)
    {
        if (cantidad <= 0 || zonaOrigenId == zonaDestinoId) throw new InvalidOperationException("Verifica la cantidad y las ubicaciones.");
        var origen = await _db.InventariosExistencia.SingleOrDefaultAsync(x => x.LoteId == loteId && x.ZonaUbicacionId == zonaOrigenId)
            ?? throw new InvalidOperationException("No existe stock en la ubicación de origen.");
        if (origen.CantidadFisica - origen.CantidadReservada < cantidad) throw new InvalidOperationException("Stock disponible insuficiente.");
        var destino = await _db.InventariosExistencia.SingleOrDefaultAsync(x => x.LoteId == loteId && x.ZonaUbicacionId == zonaDestinoId);
        origen.CantidadFisica -= cantidad;
        if (destino == null) _db.InventariosExistencia.Add(new InventarioExistencia { LoteId = loteId, ZonaUbicacionId = zonaDestinoId, CantidadFisica = cantidad, CantidadReservada = 0 });
        else destino.CantidadFisica += cantidad;
        _db.MovimientosKardex.Add(new MovimientoKardex { LoteId = loteId, Fecha = DateTime.Now, TipoMovimiento = "Traslado", Cantidad = cantidad, ZonaOrigenId = zonaOrigenId, ZonaDestinoId = zonaDestinoId, ReferenciaDocumento = referencia });
        await _db.SaveChangesAsync();
    }

    public async Task RegistrarMermaAsync(int loteId, int zonaId, decimal cantidad, decimal costoUnitario, string usuarioId, string motivo)
    {
        if (cantidad <= 0 || costoUnitario < 0) throw new InvalidOperationException("Cantidad y costo no válidos.");
        if (string.IsNullOrWhiteSpace(usuarioId)) throw new InvalidOperationException("No se pudo identificar al usuario responsable.");
        var ex = await _db.InventariosExistencia.SingleOrDefaultAsync(x => x.LoteId == loteId && x.ZonaUbicacionId == zonaId)
            ?? throw new InvalidOperationException("No existe la existencia indicada.");
        var lote = await _db.Lotes.Include(x => x.Articulo).SingleAsync(x => x.Id == loteId);
        if (ex.CantidadFisica - ex.CantidadReservada < cantidad) throw new InvalidOperationException("La merma supera el stock disponible no reservado.");
        var monto = Math.Round(cantidad * costoUnitario, 2, MidpointRounding.AwayFromZero);
        ex.CantidadFisica -= cantidad;
        var ajuste = new AjusteInventario { LoteId = loteId, ZonaUbicacionId = zonaId, UsuarioId = usuarioId, Fecha = DateTime.Now, TipoAjuste = "Merma", Cantidad = cantidad, CostoAsumido = monto };
        _db.AjustesInventario.Add(ajuste);
        _db.MovimientosKardex.Add(new MovimientoKardex { LoteId = loteId, Fecha = DateTime.Now, TipoMovimiento = "Merma", Cantidad = cantidad, ZonaOrigenId = zonaId, ReferenciaDocumento = motivo });
        if (monto > 0)
        {
            var tipo = lote.Articulo.Tipo?.ToLowerInvariant() ?? string.Empty;
            var cuentaInventario = tipo.Contains("jaba") || tipo.Contains("envase")
                ? Cuentas.Envases
                : (tipo.Contains("fresa") || tipo.Contains("terminado") || tipo.Contains("producto")
                    ? Cuentas.ProductoTerminado : Cuentas.MaterialesAux);
            await _contabilidad.RegistrarAsiento("Merma", null, $"Merma de inventario: {motivo}", usuarioId,
                (Cuentas.MermasGasto, monto, 0), (cuentaInventario, 0, monto));
        }
        await _db.SaveChangesAsync();
    }
}
