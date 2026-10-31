using AgroERP.Models.Maestros;
using AgroERP.Models.MRP_IA;
using AgroERP.ViewModels.Planificacion;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Quito. CRUD de Articulo (+ParametroInventario), CampaniaAgricola y RecetaAgricolaBOM.
/// Los métodos Guardar/Eliminar devuelven null si salió bien, o el mensaje de error.
public class DAMaestros
{
    private readonly ApplicationDbContext _db;
    public DAMaestros(ApplicationDbContext db) => _db = db;

    // ===================== ARTÍCULOS =====================
    public async Task<List<ArticuloVM>> ListarArticulosAsync()
    {
        var q = from a in _db.Articulos
                join p in _db.ParametrosInventario on a.Id equals p.ArticuloId into pj
                from p in pj.DefaultIfEmpty()
                orderby a.Codigo
                select new ArticuloVM
                {
                    Id = a.Id, Codigo = a.Codigo, Nombre = a.Nombre, Tipo = a.Tipo.ToString()!,
                    UnidadMedida = a.UnidadMedida,
                    StockSeguridad = p == null ? 0 : p.StockSeguridad,
                    LoteEconomico = p == null ? 0 : p.LoteEconomico
                };
        return await q.ToListAsync();
    }

    public async Task<ArticuloVM?> ObtenerArticuloAsync(int id)
        => (await ListarArticulosAsync()).FirstOrDefault(a => a.Id == id);

    public async Task<string?> GuardarArticuloAsync(ArticuloVM vm)
    {
        if (await _db.Articulos.AnyAsync(x => x.Codigo == vm.Codigo && x.Id != vm.Id))
            return "Ya existe un artículo con ese código.";

        Articulo a;
        if (vm.Id == 0) { a = new Articulo(); _db.Articulos.Add(a); }
        else
        {
            var existente = await _db.Articulos.FindAsync(vm.Id);
            if (existente == null) return "Artículo no encontrado.";
            a = existente;
        }
        a.Codigo = vm.Codigo.Trim();
        a.Nombre = vm.Nombre.Trim();
        a.Tipo = vm.Tipo;                      // si Tipo es enum, ver nota del mensaje
        a.UnidadMedida = vm.UnidadMedida.Trim();

        var p = vm.Id == 0 ? null : await _db.ParametrosInventario.FirstOrDefaultAsync(x => x.ArticuloId == a.Id);
        if (p == null) { p = new ParametroInventario { Articulo = a }; _db.ParametrosInventario.Add(p); }
        p.StockSeguridad = vm.StockSeguridad;
        p.LoteEconomico = vm.LoteEconomico;

        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> EliminarArticuloAsync(int id)
    {
        try
        {
            var p = await _db.ParametrosInventario.FirstOrDefaultAsync(x => x.ArticuloId == id);
            if (p != null) _db.Remove(p);
            var a = await _db.Articulos.FindAsync(id);
            if (a != null) _db.Remove(a);
            await _db.SaveChangesAsync();
            return null;
        }
        catch (DbUpdateException)
        {
            return "No se puede eliminar: el artículo tiene registros asociados (recetas, lotes, pedidos...).";
        }
    }

    // ===================== CAMPAÑAS =====================
    public Task<List<CampaniaVM>> ListarCampaniasAsync()
        => _db.CampaniasAgricolas.OrderByDescending(c => c.Id)
              .Select(c => new CampaniaVM
              {
                  Id = c.Id, Nombre = c.Nombre, Estado = c.Estado.ToString()!,
                  HectareasActivas = c.HectareasActivas, MesProyectado = c.MesProyectado
              }).ToListAsync();

    public async Task<CampaniaVM?> ObtenerCampaniaAsync(int id)
        => (await ListarCampaniasAsync()).FirstOrDefault(c => c.Id == id);

    public async Task<string?> GuardarCampaniaAsync(CampaniaVM vm)
    {
        CampaniaAgricola c;
        if (vm.Id == 0) { c = new CampaniaAgricola(); _db.CampaniasAgricolas.Add(c); }
        else
        {
            var existente = await _db.CampaniasAgricolas.FindAsync(vm.Id);
            if (existente == null) return "Campaña no encontrada.";
            c = existente;
        }
        c.Nombre = vm.Nombre.Trim();
        c.Estado = vm.Estado;
        c.HectareasActivas = vm.HectareasActivas;
        c.MesProyectado = vm.MesProyectado;
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> EliminarCampaniaAsync(int id)
    {
        try
        {
            var c = await _db.CampaniasAgricolas.FindAsync(id);
            if (c != null) _db.Remove(c);
            await _db.SaveChangesAsync();
            return null;
        }
        catch (DbUpdateException)
        {
            return "No se puede eliminar: la campaña ya tiene pronósticos calculados.";
        }
    }

    // ===================== RECETAS (BOM) =====================
    public Task<List<RecetaVM>> ListarRecetasAsync()
        => _db.RecetasAgricolasBOM
              .OrderBy(r => r.MesAplicacion).ThenBy(r => r.ArticuloInsumo.Nombre)
              .Select(r => new RecetaVM
              {
                  Id = r.Id, ArticuloFinalId = r.ArticuloFinalId, ArticuloInsumoId = r.ArticuloInsumoId,
                  MesAplicacion = r.MesAplicacion, CantidadPorJaba = r.CantidadPorJaba,
                  ArticuloFinal = r.ArticuloFinal.Nombre, ArticuloInsumo = r.ArticuloInsumo.Nombre
              }).ToListAsync();

    public async Task<RecetaVM?> ObtenerRecetaAsync(int id)
        => (await ListarRecetasAsync()).FirstOrDefault(r => r.Id == id);

    public async Task<string?> GuardarRecetaAsync(RecetaVM vm)
    {
        if (vm.ArticuloFinalId == vm.ArticuloInsumoId)
            return "El producto final y el insumo no pueden ser el mismo artículo.";
        if (await _db.RecetasAgricolasBOM.AnyAsync(x => x.Id != vm.Id && x.MesAplicacion == vm.MesAplicacion
                && x.ArticuloFinalId == vm.ArticuloFinalId && x.ArticuloInsumoId == vm.ArticuloInsumoId))
            return "Ya existe esa combinación de producto, insumo y mes.";

        RecetaAgricolaBOM r;
        if (vm.Id == 0) { r = new RecetaAgricolaBOM(); _db.RecetasAgricolasBOM.Add(r); }
        else
        {
            var existente = await _db.RecetasAgricolasBOM.FindAsync(vm.Id);
            if (existente == null) return "Receta no encontrada.";
            r = existente;
        }
        r.ArticuloFinalId = vm.ArticuloFinalId;
        r.ArticuloInsumoId = vm.ArticuloInsumoId;
        r.MesAplicacion = vm.MesAplicacion;
        r.CantidadPorJaba = vm.CantidadPorJaba;
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> EliminarRecetaAsync(int id)
    {
        var r = await _db.RecetasAgricolasBOM.FindAsync(id);
        if (r != null) { _db.Remove(r); await _db.SaveChangesAsync(); }
        return null;
    }
}
