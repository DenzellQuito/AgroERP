using AgroERP.Models.BI_Gestion;
using AgroERP.ViewModels.BI;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// Nombres EXACTOS de los indicadores con fórmula automática.
public static class KpiNombres
{
    public const string Otif = "OTIF";
    public const string CumplProveedores = "Cumplimiento de Proveedores";
    public const string Rotacion = "Rotación de Inventario";
    public const string Exactitud = "Exactitud de Inventario";
    public const string CostoLogistico = "Costo Logístico";
    public const string Merma = "Merma";
    public static readonly string[] Todos = { Otif, CumplProveedores, Rotacion, Exactitud, CostoLogistico, Merma };
}

/// DUEÑO: Quito. Solo LEE las tablas de otros módulos; escribe IndicadoresKPI.
public class DABI
{
    private readonly ApplicationDbContext _db;
    public DABI(ApplicationDbContext db) => _db = db;

    private static readonly Dictionary<string, (string unidad, bool mayorMejor)> Def = new()
    {
        [KpiNombres.Otif] = ("%", true),
        [KpiNombres.CumplProveedores] = ("%", true),
        [KpiNombres.Rotacion] = ("veces", true),
        [KpiNombres.Exactitud] = ("%", true),
        [KpiNombres.CostoLogistico] = ("%", false),
        [KpiNombres.Merma] = ("%", false),
    };

    // ===================== CRUD DE INDICADORES =====================
    public Task<List<IndicadorVM>> ListarIndicadoresAsync()
        => _db.IndicadoresKPI.OrderBy(i => i.Id)
              .Select(i => new IndicadorVM
              {
                  Id = i.Id, NombreMetrica = i.NombreMetrica,
                  MetaObjetivo = i.MetaObjetivo, ToleranciaAlarma = i.ToleranciaAlarma
              }).ToListAsync();

    public async Task<IndicadorVM?> ObtenerIndicadorAsync(int id)
        => (await ListarIndicadoresAsync()).FirstOrDefault(i => i.Id == id);

    public async Task<string?> GuardarIndicadorAsync(IndicadorVM vm)
    {
        if (await _db.IndicadoresKPI.AnyAsync(x => x.NombreMetrica == vm.NombreMetrica && x.Id != vm.Id))
            return "Ya existe ese indicador.";
        IndicadorKPI i;
        if (vm.Id == 0) { i = new IndicadorKPI(); _db.IndicadoresKPI.Add(i); }
        else
        {
            var e = await _db.IndicadoresKPI.FindAsync(vm.Id);
            if (e == null) return "Indicador no encontrado.";
            i = e;
        }
        i.NombreMetrica = vm.NombreMetrica;
        i.MetaObjetivo = vm.MetaObjetivo;
        i.ToleranciaAlarma = vm.ToleranciaAlarma;
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> EliminarIndicadorAsync(int id)
    {
        try
        {
            var i = await _db.IndicadoresKPI.FindAsync(id);
            if (i != null) { _db.Remove(i); await _db.SaveChangesAsync(); }
            return null;
        }
        catch (DbUpdateException)
        {
            return "No se puede eliminar: el indicador tiene planes de acción asociados.";
        }
    }

    // ===================== CÁLCULO DE KPIs =====================
    public async Task<List<KpiResultadoVM>> CalcularKpisAsync(int dias)
    {
        var desde = DateTime.Today.AddDays(-dias);
        var lista = new List<KpiResultadoVM>();
        foreach (var ind in await _db.IndicadoresKPI.OrderBy(i => i.Id).ToListAsync())
        {
            var tiene = Def.TryGetValue(ind.NombreMetrica, out var d);
            decimal? valor = ind.NombreMetrica switch
            {
                KpiNombres.Otif => await Otif(desde),
                KpiNombres.CumplProveedores => await CumplProveedores(desde),
                KpiNombres.Rotacion => await Rotacion(desde),
                KpiNombres.Exactitud => await Exactitud(desde),
                KpiNombres.CostoLogistico => await CostoLogistico(desde),
                KpiNombres.Merma => await Merma(desde),
                _ => null
            };
            lista.Add(new KpiResultadoVM
            {
                Nombre = ind.NombreMetrica, Valor = valor.HasValue ? Math.Round(valor.Value, 1) : null,
                Meta = ind.MetaObjetivo, Tolerancia = ind.ToleranciaAlarma,
                Unidad = tiene ? d.unidad : "", MayorEsMejor = !tiene || d.mayorMejor,
                Semaforo = Semaforo(valor, ind.MetaObjetivo, ind.ToleranciaAlarma, !tiene || d.mayorMejor)
            });
        }
        return lista;
    }

    private static string Semaforo(decimal? v, decimal meta, decimal tol, bool mayorMejor)
    {
        if (v == null) return "SinDatos";
        if (mayorMejor) return v >= meta ? "Verde" : v >= meta - tol ? "Amarillo" : "Rojo";
        return v <= meta ? "Verde" : v <= meta + tol ? "Amarillo" : "Rojo";
    }

    /// OTIF = pedidos entregados A TIEMPO y COMPLETOS (sin devolución) / pedidos entregados. Dueño de datos: Díaz.
    private async Task<decimal?> Otif(DateTime desde)
    {
        var e = await _db.PedidosVenta
            .Where(p => p.FechaEntregaReal != null && p.FechaPedido >= desde)
            .Select(p => new
            {
                ATiempo = p.FechaEntregaReal <= p.FechaPromesaEntrega,
                ConDev = _db.DevolucionesCliente.Any(x => x.PedidoVentaId == p.Id)
            }).ToListAsync();
        if (e.Count == 0) return null;
        return 100m * e.Count(x => x.ATiempo && !x.ConDev) / e.Count;
    }

    /// Cumplimiento = líneas recibidas completas (recibido >= pedido) / líneas recibidas. Dueño de datos: Lavado.
    private async Task<decimal?> CumplProveedores(DateTime desde)
    {
        var l = await _db.DetallesRecepcion
            .Where(d => d.Recepcion.FechaRecepcion >= desde)
            .Select(d => d.CantidadRecibida >= d.CantidadPedida).ToListAsync();
        if (l.Count == 0) return null;
        return 100m * l.Count(x => x) / l.Count;
    }

    /// Rotación = salidas del período / stock físico actual. Dueño de datos: Méndez (TipoMovimiento = "Salida").
    private async Task<decimal?> Rotacion(DateTime desde)
    {
        var salidas = await _db.MovimientosKardex
            .Where(m => m.TipoMovimiento == "Salida" && m.Fecha >= desde)
            .SumAsync(m => (decimal?)m.Cantidad) ?? 0;
        var stock = await _db.InventariosExistencia.SumAsync(i => (decimal?)i.CantidadFisica) ?? 0;
        if (stock <= 0 || salidas == 0) return null;
        return Math.Abs(salidas) / stock;
    }

    /// Exactitud = 100 - (diferencias de conteo / stock) x 100. Ajustes cuyo TipoAjuste contiene "Conteo". Dueño: Méndez.
    private async Task<decimal?> Exactitud(DateTime desde)
    {
        var conteos = _db.AjustesInventario.Where(a => a.Fecha >= desde && a.TipoAjuste.Contains("Conteo"));
        if (!await conteos.AnyAsync()) return null;
        var difs = await conteos.SumAsync(a => (decimal?)Math.Abs(a.Cantidad)) ?? 0;
        var stock = await _db.InventariosExistencia.SumAsync(i => (decimal?)i.CantidadFisica) ?? 0;
        if (stock <= 0) return null;
        return Math.Max(0, 100m - 100m * difs / stock);
    }

    /// Costo logístico = gastos logísticos / ventas facturadas x 100.
    private async Task<decimal?> CostoLogistico(DateTime desde)
    {
        var gasto = await _db.GastosLogisticos.Where(g => g.FechaRegistro >= desde).SumAsync(g => (decimal?)g.Monto) ?? 0;
        var ventas = await _db.ComprobantesPago.Where(c => c.PedidoVenta.FechaPedido >= desde).SumAsync(c => (decimal?)c.MontoTotal) ?? 0;
        if (ventas <= 0) return null;
        return 100m * gasto / ventas;
    }

    /// Merma = unidades de merma / unidades ingresadas x 100 (TipoAjuste = "Merma", TipoMovimiento = "Ingreso").
    private async Task<decimal?> Merma(DateTime desde)
    {
        var ingresos = await _db.MovimientosKardex
            .Where(m => m.TipoMovimiento == "Ingreso" && m.Fecha >= desde).SumAsync(m => (decimal?)m.Cantidad) ?? 0;
        if (ingresos <= 0) return null;
        var merma = await _db.AjustesInventario
            .Where(a => a.TipoAjuste == "Merma" && a.Fecha >= desde).SumAsync(a => (decimal?)Math.Abs(a.Cantidad)) ?? 0;
        return 100m * merma / ingresos;
    }

    // ===================== SERIES PARA GRÁFICOS =====================
    private static List<DateTime> Meses(int n)
    {
        var inicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-(n - 1));
        return Enumerable.Range(0, n).Select(i => inicio.AddMonths(i)).ToList();
    }

    public async Task<List<PuntoMesVM>> OtifPorMesAsync(int meses = 6)
    {
        var ms = Meses(meses);
        var datos = await _db.PedidosVenta
            .Where(p => p.FechaEntregaReal != null && p.FechaEntregaReal >= ms[0])
            .Select(p => new
            {
                Fecha = p.FechaEntregaReal!.Value,
                ATiempo = p.FechaEntregaReal <= p.FechaPromesaEntrega,
                ConDev = _db.DevolucionesCliente.Any(x => x.PedidoVentaId == p.Id)
            }).ToListAsync();
        return ms.Select(m =>
        {
            var g = datos.Where(x => x.Fecha.Year == m.Year && x.Fecha.Month == m.Month).ToList();
            return new PuntoMesVM
            {
                Mes = m.ToString("yyyy-MM"),
                Valor = g.Count == 0 ? null : Math.Round(100m * g.Count(x => x.ATiempo && !x.ConDev) / g.Count, 1)
            };
        }).ToList();
    }

    public async Task<List<PuntoMesVM>> CostoLogisticoPorMesAsync(int meses = 6)
    {
        var ms = Meses(meses);
        var gastos = await _db.GastosLogisticos.Where(g => g.FechaRegistro >= ms[0])
            .Select(g => new { g.FechaRegistro, g.Monto }).ToListAsync();
        var ventas = await _db.ComprobantesPago.Where(c => c.PedidoVenta.FechaPedido >= ms[0])
            .Select(c => new { c.PedidoVenta.FechaPedido, c.MontoTotal }).ToListAsync();
        return ms.Select(m =>
        {
            var v = ventas.Where(x => x.FechaPedido.Year == m.Year && x.FechaPedido.Month == m.Month).Sum(x => x.MontoTotal);
            var g = gastos.Where(x => x.FechaRegistro.Year == m.Year && x.FechaRegistro.Month == m.Month).Sum(x => x.Monto);
            return new PuntoMesVM { Mes = m.ToString("yyyy-MM"), Valor = v <= 0 ? null : Math.Round(100m * g / v, 2) };
        }).ToList();
    }

    /// Pareto de mermas por artículo (los 8 mayores) con porcentaje acumulado.
    public async Task<List<ParetoItemVM>> ParetoMermasAsync(int dias)
    {
        var desde = DateTime.Today.AddDays(-dias);
        var datos = await _db.AjustesInventario
            .Where(a => a.TipoAjuste == "Merma" && a.Fecha >= desde)
            .GroupBy(a => a.Lote.Articulo.Nombre)
            .Select(g => new { Articulo = g.Key, Total = g.Sum(a => Math.Abs(a.Cantidad)) })
            .OrderByDescending(x => x.Total).Take(8).ToListAsync();
        var suma = datos.Sum(x => x.Total);
        decimal acum = 0;
        return datos.Select(x =>
        {
            acum += x.Total;
            return new ParetoItemVM { Articulo = x.Articulo, Total = x.Total, AcumuladoPct = suma <= 0 ? 0 : Math.Round(100m * acum / suma, 1) };
        }).ToList();
    }
}
