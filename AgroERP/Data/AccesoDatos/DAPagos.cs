using AgroERP.Models;
using AgroERP.Models.Contabilidad;
using AgroERP.Models.Tesoreria;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Changa. Escribe CuentasPorPagar, PagosProveedor.
public class DAPagos
{
    private readonly ApplicationDbContext _db;
    private readonly DATesoreria _tesoreria;
    private readonly DAContabilidad _contabilidad;

    public DAPagos(ApplicationDbContext db, DATesoreria tesoreria, DAContabilidad contabilidad)
    {
        _db = db;
        _tesoreria = tesoreria;
        _contabilidad = contabilidad;
    }

    /// Contrato llamado por Lavado (alta activo), Méndez (mantenimiento/mejora) y Changa (OC/fletes). No llama SaveChangesAsync.
    public async Task CrearCuentaPorPagar(int proveedorId, int? ordenCompraId, string tipoObligacion, string nroFactura, decimal montoTotal, int diasVencimiento = 30)
    {
        var cuentaPorPagar = new CuentaPorPagar
        {
            ProveedorId = proveedorId,
            OrdenCompraId = ordenCompraId, // null si es mantenimiento o activo sin OC
            TipoObligacion = tipoObligacion,
            NumeroFacturaProveedor = nroFactura, // <-- ¡Corregido!
            MontoTotal = montoTotal,
            SaldoPendiente = montoTotal, // Nace con toda la deuda pendiente
            FechaEmision = DateTime.Now,
            FechaVencimiento = DateTime.Now.AddDays(diasVencimiento),
            Prioridad = "Media",
            Estado = "Pendiente"
        };

        await _db.Set<CuentaPorPagar>().AddAsync(cuentaPorPagar);
    }

    // =========================================================================
    // BANDEJA PRINCIPAL: Lista las deudas para programar y pagar
    // =========================================================================
    public async Task<List<ViewModels.Pagos.CuentaPorPagarItemVM>> ObtenerBandejaPagosAsync()
    {
        return await _db.Set<CuentaPorPagar>()
            .Include(c => c.Proveedor) // Traemos los datos del proveedor
            .Where(c => c.Estado != "Pagado") // No mostramos los que ya se pagaron por completo
            .OrderBy(c => c.FechaVencimiento) // Ordenamos para saber qué se vence primero
            .ThenBy(c => c.Prioridad == "Alta" ? 1 : c.Prioridad == "Media" ? 2 : 3)
            .Select(c => new ViewModels.Pagos.CuentaPorPagarItemVM
            {
                Id = c.Id,
                OrdenCompraId = c.OrdenCompraId,
                Proveedor = c.Proveedor.RazonSocial,
                TipoObligacion = c.TipoObligacion,
                NumeroFactura = c.NumeroFacturaProveedor, // El campo que corregimos antes
                FechaEmision = c.FechaEmision,
                FechaVencimiento = c.FechaVencimiento,
                Prioridad = c.Prioridad,
                MontoTotal = c.MontoTotal,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado
            })
            .ToListAsync();
    }

    // =========================================================================
    // CRUCE CON ÓRDENES DE COMPRA RECEPCIONADAS
    // =========================================================================
    public async Task<List<ViewModels.Pagos.OrdenCompraPendienteCxPVM>> ObtenerOrdenesPendientesFacturarAsync()
    {
        var ordenes = await _db.OrdenesCompra
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles)
            .Where(o => (o.Estado == "RecepcionadaParcial" || o.Estado == "Cerrada")
                && !_db.CuentasPorPagar.Any(c => c.OrdenCompraId == o.Id && c.TipoObligacion == "Insumos"))
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        var ocIds = ordenes.Select(o => o.Id).ToList();
        var recepciones = await _db.RecepcionesMercaderia
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Lote)
            .Where(r => ocIds.Contains(r.OrdenCompraId))
            .ToListAsync();

        var resultado = new List<ViewModels.Pagos.OrdenCompraPendienteCxPVM>();
        foreach (var oc in ordenes)
        {
            var recsOc = recepciones.Where(r => r.OrdenCompraId == oc.Id).ToList();
            decimal montoConforme = CalcularMontoConforme(oc, recsOc);
            if (montoConforme <= 0) continue;

            resultado.Add(new ViewModels.Pagos.OrdenCompraPendienteCxPVM
            {
                OrdenCompraId = oc.Id,
                ProveedorId = oc.ProveedorId,
                Proveedor = oc.Proveedor.RazonSocial,
                EstadoOrden = oc.Estado,
                MontoConforme = montoConforme
            });
        }
        return resultado;
    }

    public async Task RegistrarFacturaOrdenCompraAsync(int ordenCompraId, string nroFactura, string prioridad = "Media", int diasVencimiento = 30)
    {
        var oc = await _db.OrdenesCompra
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.Id == ordenCompraId)
            ?? throw new Exception("Orden de compra no encontrada.");

        if (oc.Estado != "RecepcionadaParcial" && oc.Estado != "Cerrada")
            throw new Exception("Solo se pueden facturar órdenes de compra recepcionadas.");

        if (await _db.CuentasPorPagar.AnyAsync(c => c.OrdenCompraId == ordenCompraId && c.TipoObligacion == "Insumos"))
            throw new Exception($"La orden OC-{ordenCompraId:D4} ya tiene una factura registrada en Cuentas por Pagar.");

        var facturaLimpia = (nroFactura ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(facturaLimpia))
            throw new Exception("Debe ingresar el número de factura del proveedor.");

        bool facturaDuplicada = await _db.CuentasPorPagar
            .AnyAsync(c => c.ProveedorId == oc.ProveedorId && c.NumeroFacturaProveedor == facturaLimpia);
        if (facturaDuplicada)
            throw new Exception($"Ya existe una cuenta por pagar con la factura '{facturaLimpia}' para este proveedor.");

        if (prioridad is not ("Alta" or "Media" or "Baja"))
            prioridad = "Media";
        if (diasVencimiento < 0) diasVencimiento = 0;

        var recsOc = await _db.RecepcionesMercaderia
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Lote)
            .Where(r => r.OrdenCompraId == ordenCompraId)
            .ToListAsync();

        var montoConforme = CalcularMontoConforme(oc, recsOc);
        if (montoConforme <= 0)
            throw new Exception("La orden no tiene mercadería conforme liberada para facturar.");

        // Regla 7 del README: para Insumos el asiento ya fue generado en Recepción (Calidad), no se duplica.
        await CrearCuentaPorPagar(oc.ProveedorId, oc.Id, "Insumos", facturaLimpia, montoConforme, diasVencimiento);
        var creada = _db.ChangeTracker.Entries<CuentaPorPagar>()
            .FirstOrDefault(e => e.State == EntityState.Added)?.Entity;
        if (creada != null) creada.Prioridad = prioridad;

        await _db.SaveChangesAsync();
    }

    private static decimal CalcularMontoConforme(Models.Compras.OrdenCompra oc, System.Collections.Generic.List<Models.Calidad_WMS.RecepcionMercaderia> recepciones)
    {
        if (!recepciones.Any())
            return Math.Round(oc.Total, 2, MidpointRounding.AwayFromZero);

        decimal total = 0m;
        foreach (var rec in recepciones)
        {
            foreach (var det in rec.Detalles.Where(d => d.Lote.EstadoCalidad == "Liberado"))
            {
                var precio = oc.Detalles.FirstOrDefault(d => d.ArticuloId == det.ArticuloId)?.PrecioUnitario ?? 0m;
                total += Math.Round(det.CantidadRecibida * precio, 2, MidpointRounding.AwayFromZero);
            }
        }
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    // =========================================================================
    // Seleccionar obligaciones y cambiar estado a Programado
    // =========================================================================
    public async Task ProgramarPagoAsync(int cuentaPorPagarId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta != null && cuenta.Estado == "Pendiente")
        {
            cuenta.Estado = "Programado";
            await _db.SaveChangesAsync();
        }
    }

    // =========================================================================
    // Validar datos y autorizar pago
    // =========================================================================
    public async Task AutorizarPagoAsync(int cuentaPorPagarId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta != null && cuenta.Estado == "Programado")
        {
            cuenta.Estado = "Autorizado";
            await _db.SaveChangesAsync();
        }
    }
    // =========================================================================
    // Ejecutar el pago y contabilizar
    // =========================================================================
    public async Task ProcesarPagoAsync(int cuentaPorPagarId, int cuentaBancariaId, string medioPago, string nroOperacion, decimal montoPagado, string usuarioId)
    {
        var cuenta = await _db.Set<CuentaPorPagar>().FindAsync(cuentaPorPagarId);
        if (cuenta == null) throw new Exception("Cuenta por pagar no encontrada.");

        if (cuenta.Estado != "Autorizado" && cuenta.Estado != "PagadoParcial")
            throw new Exception("La cuenta por pagar debe estar Autorizada antes de ejecutar el desembolso.");

        if (montoPagado <= 0)
            throw new Exception("El monto a pagar debe ser mayor a cero.");

        if (montoPagado > cuenta.SaldoPendiente)
            throw new Exception("El monto a pagar supera el saldo pendiente.");

        // VALIDACIÓN DE DUPLICIDAD DE NÚMERO DE OPERACIÓN EN PAGOSPROVEEDOR
        var nroLimpio = (nroOperacion ?? "").Trim();
        if (string.IsNullOrEmpty(nroLimpio))
            throw new Exception("Debe ingresar un número de operación válido.");

        bool operacionDuplicada = await _db.Set<PagoProveedor>()
            .AnyAsync(p => p.NumeroOperacion == nroLimpio);

        if (operacionDuplicada)
            throw new Exception($"El número de operación '{nroLimpio}' ya fue registrado en otro pago a proveedor.");

        // 1. Escribir en tu tabla PagosProveedor
        var pago = new PagoProveedor
        {
            CuentaPorPagarId = cuentaPorPagarId,
            CuentaBancariaId = cuentaBancariaId,
            FechaPago = DateTime.Now,
            MedioPago = medioPago, // "Transferencia", "Deposito" o "POS"
            NumeroOperacion = nroLimpio,
            MontoPagado = montoPagado,
            UsuarioId = usuarioId
        };
        await _db.Set<PagoProveedor>().AddAsync(pago);

        // 2. Rebajar saldo de la deuda y actualizar estado
        cuenta.SaldoPendiente -= montoPagado;
        cuenta.Estado = cuenta.SaldoPendiente == 0 ? "Pagado" : "PagadoParcial";

        // 3. CONTRATO TESORERÍA: Descontar el dinero de la cuenta bancaria (Paso B)
        await _tesoreria.RegistrarMovimientoBancario(cuentaBancariaId, "Egreso", montoPagado, medioPago, nroLimpio, $"PAG-FACT-{cuenta.NumeroFacturaProveedor}");

        // 4. CONTRATO CONTABILIDAD: Asiento Doble Automático (Paso C y D)
        string cuentaDebe = cuenta.TipoObligacion == "ActivoFijo" ? Cuentas.PasivoActivoFijo : Cuentas.FacturasPorPagar;

        await _contabilidad.RegistrarAsiento(
            "Pagos",
            cuentaPorPagarId,
            $"Pago de factura {cuenta.NumeroFacturaProveedor}",
            usuarioId,
            (cuentaDebe, montoPagado, 0m),     // DEBE: 4212 o 4654
            (Cuentas.Bancos, 0m, montoPagado)  // HABER: 1041
        );

        // 5. UN SOLO GUARDADO POR OPERACIÓN (Regla 4)
        await _db.SaveChangesAsync();
    }

    // =========================================================================
    // HISTORIAL DE PAGOS EJECUTADOS Y VOUCHER DE EGRESO
    // =========================================================================
    public async Task<List<ViewModels.Pagos.PagoEjecutadoItemVM>> ObtenerPagosEjecutadosAsync()
    {
        return await _db.Set<PagoProveedor>()
            .Include(p => p.CuentaPorPagar).ThenInclude(c => c.Proveedor)
            .Include(p => p.CuentaBancaria)
            .OrderByDescending(p => p.FechaPago).ThenByDescending(p => p.Id)
            .Take(40)
            .Select(p => new ViewModels.Pagos.PagoEjecutadoItemVM
            {
                PagoId = p.Id,
                CuentaPorPagarId = p.CuentaPorPagarId,
                OrdenCompraId = p.CuentaPorPagar.OrdenCompraId,
                Proveedor = p.CuentaPorPagar.Proveedor.RazonSocial,
                TipoObligacion = p.CuentaPorPagar.TipoObligacion,
                NumeroFactura = p.CuentaPorPagar.NumeroFacturaProveedor,
                Banco = p.CuentaBancaria.Banco + " (" + p.CuentaBancaria.NumeroCuenta + ")",
                FechaPago = p.FechaPago,
                MedioPago = p.MedioPago,
                NumeroOperacion = p.NumeroOperacion,
                MontoPagado = p.MontoPagado
            })
            .ToListAsync();
    }

    public async Task<ViewModels.Pagos.VoucherPagoPdfVM?> ObtenerVoucherPagoAsync(int pagoId)
    {
        var p = await _db.Set<PagoProveedor>()
            .Include(x => x.CuentaPorPagar).ThenInclude(c => c.Proveedor)
            .Include(x => x.CuentaBancaria)
            .FirstOrDefaultAsync(x => x.Id == pagoId);
        if (p == null) return null;

        var usuario = await _db.Users
            .Where(u => u.Id == p.UsuarioId)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync() ?? p.UsuarioId;

        return new ViewModels.Pagos.VoucherPagoPdfVM
        {
            PagoId = p.Id,
            CuentaPorPagarId = p.CuentaPorPagarId,
            OrdenCompraId = p.CuentaPorPagar.OrdenCompraId,
            Proveedor = p.CuentaPorPagar.Proveedor.RazonSocial,
            ProveedorRuc = p.CuentaPorPagar.Proveedor.Ruc,
            TipoObligacion = p.CuentaPorPagar.TipoObligacion,
            NumeroFactura = p.CuentaPorPagar.NumeroFacturaProveedor,
            CuentaBancaria = $"{p.CuentaBancaria.Banco} · {p.CuentaBancaria.NumeroCuenta}",
            FechaPago = p.FechaPago,
            MedioPago = p.MedioPago,
            NumeroOperacion = p.NumeroOperacion,
            MontoTotalObligacion = p.CuentaPorPagar.MontoTotal,
            MontoPagado = p.MontoPagado,
            SaldoRestante = p.CuentaPorPagar.SaldoPendiente,
            CuentaContableDebe = p.CuentaPorPagar.TipoObligacion == "ActivoFijo"
                ? "4654 Cuentas por pagar diversas - Activo fijo"
                : "4212 Facturas por pagar",
            Usuario = usuario
        };
    }
}