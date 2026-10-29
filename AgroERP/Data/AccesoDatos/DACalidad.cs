using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: George Lavado. Tablas que escribe: RecepcionMercaderia, RecepcionDetalle, Lote, InspeccionCalidad.
/// REGLA: solo este DAO cambia Lote.EstadoCalidad (Cuarentena -> Liberado / Rechazado).
public class DACalidad
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    private readonly DAAlmacen _almacen;
    private readonly DACompras _compras;
    public DACalidad(ApplicationDbContext db, DAContabilidad contabilidad, DAAlmacen almacen, DACompras compras)
    {
        _db = db;
        _contabilidad = contabilidad;
        _almacen = almacen;
        _compras = compras;
    }

    // ===================== TU CÓDIGO AQUÍ =====================
    // Al liberar un lote conforme debes, en UN SOLO SaveChanges:
    //   1) Lote.EstadoCalidad = "Liberado"
    //   2) await _almacen.RegistrarIngreso(loteId, zonaId, cantidad, "OC-xxx")
    //   3) await _contabilidad.RegistrarAsiento("Recepcion", recepcionId, glosa, userId,
    //          (Cuentas.MaterialesAux, monto, 0), (Cuentas.FacturasPorPagar, 0, monto));
    //   4) await _compras.MarcarOrdenRecepcionada(ocId, parcial);
}
