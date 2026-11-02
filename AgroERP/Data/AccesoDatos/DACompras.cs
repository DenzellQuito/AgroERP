using Microsoft.EntityFrameworkCore;
using AgroERP.Models.Compras;
using AgroERP.Models.MRP_IA; // Agregado para poder leer la clase NecesidadMrp de Quito

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Changa. Tablas que escribe: Proveedor, RequerimientoCompra, RequerimientoDetalle,
/// CotizacionProveedor, OrdenCompra, OrdenCompraDetalle, GastoLogistico.
public class DACompras
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;

    public DACompras(ApplicationDbContext db, DAContabilidad contabilidad)
    {
        _db = db;
        _contabilidad = contabilidad;
    }

    // ===================== CONTRATOS (los llaman otros módulos) =====================

    /// LLAMA: Quito (MRP). Crea un RequerimientoCompra en estado "Borrador" con un detalle por cada
    /// Necesidad_MRP (guardando NecesidadMrpId) y marca esas necesidades como "ConRequerimiento".
    public async Task<int> CrearRequerimientoDesdeMrp(List<int> necesidadIds, string usuarioId)
    {
        // 1. Buscar las necesidades de MRP en la base de datos usando los Ids que envía Quito
        var necesidades = await _db.Set<NecesidadMrp>()
                                   .Where(n => necesidadIds.Contains(n.Id))
                                   .ToListAsync();

        // 2. Crear la cabecera del Requerimiento en estado exacto según Regla 8
        var requerimiento = new RequerimientoCompra
        {
            UsuarioSolicitanteId = usuarioId,
            FechaSolicitud = DateTime.Now,
            Estado = "Borrador"
        };

        // 3. Crear los detalles a partir de la información de las necesidades
        foreach (var nec in necesidades)
        {
            requerimiento.Detalles.Add(new RequerimientoDetalle
            {
                ArticuloId = nec.ArticuloId,
                NecesidadMrpId = nec.Id,
                Cantidad = nec.CantidadNetaSugerida
            });

            // 4. Actualizar el estado de la necesidad MRP (Regla 8)
            nec.Estado = "ConRequerimiento";
        }

        // 5. Agregar a la base de datos (Sin SaveChanges por Regla 4)
        _db.Set<RequerimientoCompra>().Add(requerimiento);

        return requerimiento.Id;
    }

    /// LLAMA: Lavado (al cerrar una recepción). Cambia OrdenCompra.Estado a
    /// "RecepcionadaParcial" si parcial = true, o "Cerrada" si parcial = false.
    public async Task MarcarOrdenRecepcionada(int ordenCompraId, bool parcial)
    {
        // Buscar la orden de compra que indica Lavado
        var orden = await _db.Set<OrdenCompra>().FindAsync(ordenCompraId);

        if (orden != null)
        {
            // Cambiar el estado en texto exacto (Regla 8)
            orden.Estado = parcial ? "RecepcionadaParcial" : "Cerrada";
        }
    }

    // ===================== TU CÓDIGO AQUÍ =====================

    // 1. Obtener todos los requerimientos (Para listar en tu pantalla principal)
    public async Task<List<RequerimientoCompra>> ObtenerRequerimientosAsync()
    {
        return await _db.Set<RequerimientoCompra>()
                        .Include(r => r.Detalles)
                            .ThenInclude(d => d.Articulo)
                        .OrderByDescending(r => r.FechaSolicitud)
                        .ToListAsync();
    }

    // 2. Obtener un requerimiento específico con sus detalles
    public async Task<RequerimientoCompra?> ObtenerRequerimientoPorIdAsync(int id)
    {
        return await _db.Set<RequerimientoCompra>()
                        .Include(r => r.Detalles)
                            .ThenInclude(d => d.Articulo)
                        .FirstOrDefaultAsync(r => r.Id == id);
    }

    // 3. Crear un requerimiento manual (El que hace el Agrónomo/Capataz)
    public async Task<int> CrearRequerimientoManualAsync(string usuarioId, List<RequerimientoDetalle> detalles)
    {
        var requerimiento = new RequerimientoCompra
        {
            UsuarioSolicitanteId = usuarioId,
            FechaSolicitud = DateTime.Now,
            Estado = "Borrador", // Cumpliendo la Regla 8: Texto exacto
            Detalles = detalles
        };

        _db.Set<RequerimientoCompra>().Add(requerimiento);

        // Aquí SÍ usamos SaveChangesAsync porque esta operación nace y termina en tu módulo
        await _db.SaveChangesAsync();

        return requerimiento.Id;
    }

    // 4. Enviar a aprobación (Avanzar el flujo)
    public async Task EnviarAprobacionAsync(int requerimientoId)
    {
        var requerimiento = await _db.Set<RequerimientoCompra>().FindAsync(requerimientoId);

        if (requerimiento != null && requerimiento.Estado == "Borrador")
        {
            requerimiento.Estado = "PendienteAprobacion"; // Cumpliendo la Regla 8
            await _db.SaveChangesAsync();
        }
    }

    // 5. Obtener lista de artículos (Lectura permitida por Regla 1)
    public async Task<List<AgroERP.Models.Maestros.Articulo>> ObtenerArticulosAsync()
    {
        return await _db.Set<AgroERP.Models.Maestros.Articulo>().ToListAsync();
    }
}