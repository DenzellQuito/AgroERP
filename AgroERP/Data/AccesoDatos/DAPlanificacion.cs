using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Quito. Tablas que escribe: Articulo, ParametroInventario, CampaniaAgricola, PronosticoDemanda,
/// RecetaAgricola_BOM, Necesidad_MRP, IndicadorKPI, PlanAccionMejora.
public class DAPlanificacion
{
    private readonly ApplicationDbContext _db;
    private readonly DACompras _compras;
    public DAPlanificacion(ApplicationDbContext db, DACompras compras)
    {
        _db = db;
        _compras = compras;
    }

    // ===================== TU CÓDIGO AQUÍ (Quito) =====================
}
