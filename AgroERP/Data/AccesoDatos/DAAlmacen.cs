using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Méndez. Tablas que escribe: Almacen, ZonaUbicacion, InventarioExistencia,
/// ReservaInventario, MovimientoKardex, AjusteInventario.
/// REGLA: SOLO este DAO modifica el stock. Los demás módulos llaman a estos métodos.
public class DAAlmacen
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;
    public DAAlmacen(ApplicationDbContext db, DAContabilidad contabilidad)
    {
        _db = db;
        _contabilidad = contabilidad;
    }

    // ===================== CONTRATOS (los llaman otros módulos) =====================

    /// LLAMA: Lavado, al liberar un lote. Suma CantidadFisica en InventarioExistencia (lote + zona)
    /// y registra un MovimientoKardex tipo "Ingreso". NO llama SaveChanges (lo hace quien invoca).
    public Task RegistrarIngreso(int loteId, int zonaId, decimal cantidad, string referencia)
        => throw new NotImplementedException("Pendiente: Méndez");

    /// LLAMA: Díaz, al confirmar un pedido. Aplica FEFO (lotes "Liberado", ORDER BY FechaVencimiento ASC),
    /// valida disponible = CantidadFisica - CantidadReservada, crea ReservaInventario "Activa" y suma
    /// CantidadReservada. Devuelve false si no hay stock suficiente.
    public Task<bool> ReservarStock(int pedidoVentaDetalleId, int articuloId, decimal cantidad)
        => throw new NotImplementedException("Pendiente: Méndez");

    /// LLAMA: Díaz, si se cancela el pedido. Pasa las reservas "Activa" del pedido a "Liberada"
    /// y resta CantidadReservada.
    public Task LiberarReserva(int pedidoVentaId)
        => throw new NotImplementedException("Pendiente: Méndez");

    /// LLAMA: Díaz, al cargar el vehículo. Por cada OrdenPickingDetalle: baja CantidadFisica y
    /// CantidadReservada, marca la reserva "Consumida" y registra MovimientoKardex tipo "Salida".
    public Task ConfirmarSalida(int ordenPickingId)
        => throw new NotImplementedException("Pendiente: Méndez");

    /// LLAMA: Díaz, al registrar una devolución. Jabas buenas vuelven al stock; las dañadas se
    /// registran como merma (AjusteInventario + asiento con Cuentas.MermasGasto).
    public Task ReingresarDevolucion(int devolucionId)
        => throw new NotImplementedException("Pendiente: Méndez");

    // ===================== TU CÓDIGO AQUÍ =====================
}
