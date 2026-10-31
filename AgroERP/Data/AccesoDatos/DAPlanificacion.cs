using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AgroERP.Models.Maestros;
using AgroERP.Models.MRP_IA;
using AgroERP.Models.Calidad_WMS;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Quito. MRP agrícola: pronóstico (XGBoost vía Python) -> BOM -> necesidad neta -> requerimiento.
public class DAPlanificacion
{
    private readonly ApplicationDbContext _db;
    private readonly HttpClient _http;
    private readonly DACompras _compras;

    public DAPlanificacion(ApplicationDbContext db, HttpClient http, DACompras compras)
    {
        _db = db;
        _http = http;
        _compras = compras;
    }

    private class ForecastRequest
    {
        [JsonPropertyName("hectareas")] public decimal Hectareas { get; set; }
        [JsonPropertyName("mes")] public int Mes { get; set; }
    }

    private class ForecastResponse
    {
        [JsonPropertyName("volumen_estimado")] public int VolumenEstimado { get; set; }
        [JsonPropertyName("modelo")] public string? Modelo { get; set; }
    }

    public Task<List<CampaniaAgricola>> ListarCampaniasAsync()
        => _db.Set<CampaniaAgricola>().OrderByDescending(c => c.Id).ToListAsync();

    /// Paso 1: llama al microservicio Python y guarda el PronosticoDemanda.
    public async Task<PronosticoDemanda> CalcularPronosticoAsync(int campaniaId)
    {
        var c = await _db.Set<CampaniaAgricola>().FindAsync(campaniaId)
                ?? throw new InvalidOperationException("Campaña no encontrada.");

        ForecastResponse? data;
        try
        {
            var resp = await _http.PostAsJsonAsync("/forecast",
                new ForecastRequest { Hectareas = c.HectareasActivas, Mes = c.MesProyectado });
            resp.EnsureSuccessStatusCode();
            data = await resp.Content.ReadFromJsonAsync<ForecastResponse>();
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "No se pudo conectar con el microservicio de pronóstico (Python). ¿Está ejecutándose en el puerto 8000?");
        }
        if (data == null) throw new InvalidOperationException("Respuesta vacía del microservicio.");

        var p = new PronosticoDemanda
        {
            CampaniaId = c.Id,
            FechaCalculo = DateTime.Now,
            VolumenCosechaEstimado = data.VolumenEstimado
        };
        _db.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    /// Paso 2: BOM del mes x jabas estimadas + stock de seguridad - stock disponible liberado.
    /// Redondea hacia arriba al lote económico. Reemplaza las necesidades "Pendiente" previas del pronóstico.
    public async Task<List<NecesidadMrp>> GenerarNecesidadesAsync(int pronosticoId)
    {
        var p = await _db.Set<PronosticoDemanda>().Include(x => x.Campania)
                    .FirstOrDefaultAsync(x => x.Id == pronosticoId)
                ?? throw new InvalidOperationException("Pronóstico no encontrado.");

        var mes = p.Campania.MesProyectado;
        var receta = await _db.Set<RecetaAgricolaBOM>().Where(r => r.MesAplicacion == mes).ToListAsync();
        if (!receta.Any())
            throw new InvalidOperationException($"No hay receta agrícola (BOM) cargada para el mes {mes}.");

        var previas = await _db.Set<NecesidadMrp>()
            .Where(n => n.PronosticoId == pronosticoId && n.Estado == "Pendiente").ToListAsync();
        _db.RemoveRange(previas);

        var resultado = new List<NecesidadMrp>();
        foreach (var g in receta.GroupBy(r => r.ArticuloInsumoId))
        {
            var bruta = g.Sum(r => r.CantidadPorJaba) * p.VolumenCosechaEstimado;

            var disponible = await _db.Set<InventarioExistencia>()
                .Where(i => i.Lote.ArticuloId == g.Key && i.Lote.EstadoCalidad == "Liberado")
                .SumAsync(i => i.CantidadFisica - i.CantidadReservada);

            var param = await _db.Set<ParametroInventario>().FirstOrDefaultAsync(x => x.ArticuloId == g.Key);
            var neta = bruta + (param?.StockSeguridad ?? 0) - disponible;
            if (neta <= 0) continue;

            if (param != null && param.LoteEconomico > 0)
                neta = Math.Ceiling(neta / param.LoteEconomico) * param.LoteEconomico;

            resultado.Add(new NecesidadMrp
            {
                PronosticoId = pronosticoId,
                ArticuloId = g.Key,
                CantidadNetaSugerida = Math.Round(neta, 4),
                Estado = "Pendiente"
            });
        }

        _db.AddRange(resultado);
        await _db.SaveChangesAsync();
        return resultado;
    }

    public Task<PronosticoDemanda?> ObtenerPronosticoAsync(int id)
        => _db.Set<PronosticoDemanda>().Include(x => x.Campania).FirstOrDefaultAsync(x => x.Id == id);

    public Task<List<NecesidadMrp>> ListarNecesidadesAsync(int pronosticoId)
        => _db.Set<NecesidadMrp>().Include(n => n.Articulo)
              .Where(n => n.PronosticoId == pronosticoId).OrderBy(n => n.Id).ToListAsync();

    /// Paso 3: entrega las necesidades a Compras (implementado por Changa).
    public Task<int> EnviarARequerimientoAsync(List<int> necesidadIds, string usuarioId)
        => _compras.CrearRequerimientoDesdeMrp(necesidadIds, usuarioId);
}
