using AgroERP.Models.Contabilidad;
using AgroERP.Models.Tesoreria;
using AgroERP.ViewModels.Tesoreria;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Quito. Escribe CuentasBancarias, MovimientosBancarios, ConciliacionesBancarias.
/// Solo LEE CuentasPorCobrar y CuentasPorPagar para proyectar liquidez.
public class DATesoreria
{
    private readonly ApplicationDbContext _db;
    private readonly DAContabilidad _contabilidad;

    public DATesoreria(ApplicationDbContext db, DAContabilidad contabilidad)
    {
        _db = db;
        _contabilidad = contabilidad;
    }

    // ===================== CONTRATO PARA OTROS MÓDULOS (DÍAZ Y CHANGA) =====================
    /// Regla 4 y 7 del README: Actualiza saldos e inserta en MovimientosBancarios SIN llamar a SaveChangesAsync.
    public async Task RegistrarMovimientoBancario(
        int cuentaBancariaId,
        string tipoMovimiento,
        decimal monto,
        string medioPago,
        string nroOperacion,
        string referencia)
    {
        if (monto <= 0)
            throw new InvalidOperationException("El monto del movimiento bancario debe ser mayor a cero.");

        if (tipoMovimiento is not ("Ingreso" or "Egreso"))
            throw new InvalidOperationException("TipoMovimiento inválido. Use 'Ingreso' o 'Egreso'.");

        var cuenta = await _db.CuentasBancarias.FindAsync(cuentaBancariaId)
            ?? throw new InvalidOperationException("La cuenta bancaria seleccionada no existe.");

        if (!cuenta.Activa)
            throw new InvalidOperationException($"La cuenta {cuenta.Banco} - {cuenta.NumeroCuenta} está inactiva.");

        monto = Math.Round(monto, 2, MidpointRounding.AwayFromZero);

        if (tipoMovimiento == "Egreso")
        {
            if (cuenta.SaldoDisponible < monto)
                throw new InvalidOperationException(
                    $"Saldo disponible insuficiente en {cuenta.Banco} ({cuenta.NumeroCuenta}). Disponible: S/ {cuenta.SaldoDisponible:N2}, requerido: S/ {monto:N2}.");

            cuenta.SaldoContable -= monto;
            cuenta.SaldoDisponible -= monto;
        }
        else
        {
            cuenta.SaldoContable += monto;
            cuenta.SaldoDisponible += monto;
        }

        _db.MovimientosBancarios.Add(new MovimientoBancario
        {
            CuentaBancariaId = cuenta.Id,
            Fecha = DateTime.Now,
            TipoMovimiento = tipoMovimiento,
            MedioPago = string.IsNullOrWhiteSpace(medioPago) ? "Transferencia" : medioPago,
            NumeroOperacion = nroOperacion.Trim(),
            Monto = monto,
            ReferenciaDocumento = referencia,
            Conciliado = false
        });
    }

    // ===================== DASHBOARD, FLUJO DE CAJA Y LIQUIDEZ =====================
    public async Task<TesoreriaDashboardVM> ObtenerDashboardAsync()
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var cuentas = await _db.CuentasBancarias
            .OrderBy(c => c.Banco)
            .Select(c => new CuentaBancariaItemVM
            {
                Id = c.Id,
                Banco = c.Banco,
                NumeroCuenta = c.NumeroCuenta,
                Moneda = c.Moneda,
                Titular = c.Titular,
                SaldoContable = c.SaldoContable,
                SaldoDisponible = c.SaldoDisponible,
                Activa = c.Activa,
                MovimientosSinConciliar = c.Movimientos.Count(m => !m.Conciliado)
            }).ToListAsync();

        var movsMes = await _db.MovimientosBancarios
            .Where(m => m.Fecha >= inicioMes)
            .Select(m => new { m.TipoMovimiento, m.Monto, m.ReferenciaDocumento })
            .ToListAsync();

        // Cobros y pagos operativos del mes (excluyendo el asiento de apertura inicial para mostrar el flujo limpio)
        var cobrosMes = movsMes
            .Where(m => m.TipoMovimiento == "Ingreso" && m.ReferenciaDocumento != "SALDO-INICIAL")
            .Sum(m => m.Monto);

        var pagosMes = movsMes
            .Where(m => m.TipoMovimiento == "Egreso")
            .Sum(m => m.Monto);

        var saldoFinalDisponible = cuentas.Where(c => c.Activa).Sum(c => c.SaldoDisponible);
        var saldoFinalContable = cuentas.Where(c => c.Activa).Sum(c => c.SaldoContable);

        // Fórmula PDF Slide 20: Saldo Inicial + Cobros - Pagos = Saldo Final
        var saldoInicialCaja = saldoFinalContable - cobrosMes + pagosMes;

        // Proyección de liquidez leyendo CxC (Díaz) y CxP (Changa)
        var totalPorCobrar = await _db.CuentasPorCobrar
            .Where(x => x.Estado != "Cobrado")
            .SumAsync(x => (decimal?)x.SaldoPendiente) ?? 0m;

        var totalPorPagar = await _db.CuentasPorPagar
            .Where(x => x.Estado != "Pagado")
            .SumAsync(x => (decimal?)x.SaldoPendiente) ?? 0m;

        var ultimosMovs = await _db.MovimientosBancarios
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .Take(25)
            .Select(m => new MovimientoBancarioItemVM
            {
                Id = m.Id,
                CuentaBanco = m.CuentaBancaria.Banco + " (" + m.CuentaBancaria.NumeroCuenta + ")",
                Fecha = m.Fecha,
                TipoMovimiento = m.TipoMovimiento,
                MedioPago = m.MedioPago,
                NumeroOperacion = m.NumeroOperacion,
                Monto = m.Monto,
                ReferenciaDocumento = m.ReferenciaDocumento,
                Conciliado = m.Conciliado
            }).ToListAsync();

        var ultimasConciliaciones = await _db.ConciliacionesBancarias
            .OrderByDescending(c => c.FechaCorte)
            .ThenByDescending(c => c.Id)
            .Take(10)
            .Select(c => new ConciliacionItemVM
            {
                Id = c.Id,
                CuentaBanco = c.CuentaBancaria.Banco + " (" + c.CuentaBancaria.NumeroCuenta + ")",
                FechaCorte = c.FechaCorte,
                SaldoExtractoBanco = c.SaldoExtractoBanco,
                DepositosEnTransito = c.DepositosEnTransito,
                PagosPendientes = c.PagosPendientes,
                ComisionesBancarias = c.ComisionesBancarias,
                SaldoContableAjustado = c.SaldoContableAjustado,
                Estado = c.Estado,
                UsuarioId = c.UsuarioId
            }).ToListAsync();

        return new TesoreriaDashboardVM
        {
            SaldoInicialCaja = saldoInicialCaja,
            CobrosMes = cobrosMes,
            PagosMes = pagosMes,
            SaldoFinalDisponible = saldoFinalDisponible,
            SaldoFinalContable = saldoFinalContable,
            TotalPorCobrarPendiente = totalPorCobrar,
            TotalPorPagarPendiente = totalPorPagar,
            LiquidezProyectada = saldoFinalDisponible + totalPorCobrar - totalPorPagar,
            Cuentas = cuentas,
            UltimosMovimientos = ultimosMovs,
            UltimasConciliaciones = ultimasConciliaciones
        };
    }

    // ===================== MAESTRO DE CUENTAS BANCARIAS =====================
    public async Task<string?> CrearCuentaBancariaAsync(CrearCuentaBancariaVM vm)
    {
        var nro = vm.NumeroCuenta.Trim();
        if (await _db.CuentasBancarias.AnyAsync(c => c.NumeroCuenta == nro))
            return "Ya existe una cuenta bancaria con ese número.";

        var saldoInicial = Math.Round(vm.SaldoInicial, 2, MidpointRounding.AwayFromZero);

        var cuenta = new CuentaBancaria
        {
            Banco = vm.Banco.Trim(),
            NumeroCuenta = nro,
            Moneda = "PEN",
            Titular = vm.Titular.Trim(),
            SaldoContable = saldoInicial,
            SaldoDisponible = saldoInicial,
            Activa = true
        };

        if (saldoInicial > 0)
        {
            cuenta.Movimientos.Add(new MovimientoBancario
            {
                Fecha = DateTime.Now,
                TipoMovimiento = "Ingreso",
                MedioPago = "Deposito",
                NumeroOperacion = "APERTURA-" + DateTime.Now.ToString("yyyyMMdd"),
                Monto = saldoInicial,
                ReferenciaDocumento = "SALDO-INICIAL",
                Conciliado = true
            });
        }

        _db.CuentasBancarias.Add(cuenta);
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> CambiarEstadoCuentaAsync(int id)
    {
        var c = await _db.CuentasBancarias.FindAsync(id);
        if (c == null) return "Cuenta bancaria no encontrada.";

        if (c.Activa)
        {
            bool tienePendientes = await _db.MovimientosBancarios
                .AnyAsync(m => m.CuentaBancariaId == id && !m.Conciliado);
            if (tienePendientes)
                return $"No se puede desactivar la cuenta {c.Banco} ({c.NumeroCuenta}) porque aún tiene movimientos sin conciliar.";
        }

        c.Activa = !c.Activa;
        await _db.SaveChangesAsync();
        return null;
    }

    /// Permite registrar un ingreso/egreso directo de tesorería (ej. aporte a caja/bancos) usando el propio contrato.
    public async Task<string?> RegistrarMovimientoManualAsync(RegistrarMovimientoManualVM vm)
    {
        try
        {
            await RegistrarMovimientoBancario(
                vm.CuentaBancariaId,
                vm.TipoMovimiento,
                vm.Monto,
                vm.MedioPago,
                vm.NumeroOperacion,
                string.IsNullOrWhiteSpace(vm.ReferenciaDocumento) ? "TESORERIA-DIRECTO" : vm.ReferenciaDocumento.Trim());

            await _db.SaveChangesAsync();
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    // ===================== CONCILIACIÓN BANCARIA =====================
    public async Task<ConciliarCuentaVM?> ObtenerDatosConciliacionAsync(int cuentaBancariaId)
    {
        var c = await _db.CuentasBancarias.FindAsync(cuentaBancariaId);
        if (c == null) return null;

        var movs = await _db.MovimientosBancarios
            .Where(m => m.CuentaBancariaId == cuentaBancariaId)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.Id)
            .Select(m => new MovimientoBancarioItemVM
            {
                Id = m.Id,
                CuentaBanco = c.Banco,
                Fecha = m.Fecha,
                TipoMovimiento = m.TipoMovimiento,
                MedioPago = m.MedioPago,
                NumeroOperacion = m.NumeroOperacion,
                Monto = m.Monto,
                ReferenciaDocumento = m.ReferenciaDocumento,
                Conciliado = m.Conciliado
            }).ToListAsync();

        var historial = await _db.ConciliacionesBancarias
            .Where(x => x.CuentaBancariaId == cuentaBancariaId)
            .OrderByDescending(x => x.FechaCorte)
            .ThenByDescending(x => x.Id)
            .Select(x => new ConciliacionItemVM
            {
                Id = x.Id,
                CuentaBanco = c.Banco,
                FechaCorte = x.FechaCorte,
                SaldoExtractoBanco = x.SaldoExtractoBanco,
                DepositosEnTransito = x.DepositosEnTransito,
                PagosPendientes = x.PagosPendientes,
                ComisionesBancarias = x.ComisionesBancarias,
                SaldoContableAjustado = x.SaldoContableAjustado,
                Estado = x.Estado,
                UsuarioId = x.UsuarioId
            }).ToListAsync();

        return new ConciliarCuentaVM
        {
            CuentaBancariaId = c.Id,
            Banco = c.Banco,
            NumeroCuenta = c.NumeroCuenta,
            Moneda = c.Moneda,
            SaldoContableActual = c.SaldoContable,
            SaldoDisponibleActual = c.SaldoDisponible,
            FechaCorte = DateTime.Today,
            SaldoExtractoBanco = c.SaldoContable,
            MovimientosPendientes = movs.Where(m => !m.Conciliado).ToList(),
            MovimientosConciliados = movs.Where(m => m.Conciliado).Take(15).ToList(),
            HistorialConciliaciones = historial
        };
    }

    public async Task<string?> ConciliarMovimientoIndividualAsync(int movimientoId)
    {
        var m = await _db.MovimientosBancarios.FindAsync(movimientoId);
        if (m == null) return "Movimiento no encontrado.";
        m.Conciliado = !m.Conciliado;
        await _db.SaveChangesAsync();
        return null;
    }

    /// Ejecuta el BPMN de Conciliación Bancaria en 1 solo SaveChangesAsync:
    /// 1) Empareja movimientos seleccionados.
    /// 2) Si hay comisiones bancarias (> 0), descuenta saldo, registra el egreso y genera asiento (Debe 6391 / Haber 1041).
    /// 3) Verifica que (Extracto + Depósitos en Tránsito - Pagos Pendientes) == Saldo Contable Ajustado y cierra la conciliación.
    public async Task<string?> EjecutarConciliacionAsync(ConciliarCuentaVM vm, string usuarioId)
    {
        var cuenta = await _db.CuentasBancarias.FindAsync(vm.CuentaBancariaId);
        if (cuenta == null) return "Cuenta bancaria no encontrada.";
        if (!cuenta.Activa) return "No se puede ejecutar una conciliación sobre una cuenta bancaria inactiva.";

        var extracto = Math.Round(vm.SaldoExtractoBanco, 2, MidpointRounding.AwayFromZero);
        var transito = Math.Round(vm.DepositosEnTransito, 2, MidpointRounding.AwayFromZero);
        var pendientes = Math.Round(vm.PagosPendientes, 2, MidpointRounding.AwayFromZero);
        var comisiones = Math.Round(vm.ComisionesBancarias, 2, MidpointRounding.AwayFromZero);

        if (transito < 0 || pendientes < 0 || comisiones < 0)
            return "Los montos de depósitos en tránsito, pagos pendientes y comisiones no pueden ser negativos.";

        // Saldo contable del ERP luego de descontar comisiones del extracto
        var saldoContableAjustado = Math.Round(cuenta.SaldoContable - comisiones, 2, MidpointRounding.AwayFromZero);

        // Saldo conciliado según fórmula de Slide 25: Extracto + Depósitos en tránsito - Pagos pendientes
        var saldoBancoConciliado = Math.Round(extracto + transito - pendientes, 2, MidpointRounding.AwayFromZero);

        var diferencia = saldoBancoConciliado - saldoContableAjustado;
        if (diferencia != 0m)
        {
            return $"La conciliación no cuadra por S/ {diferencia:N2}. " +
                   $"Saldo Extracto Ajustado (S/ {saldoBancoConciliado:N2}) vs. Saldo ERP Ajustado (S/ {saldoContableAjustado:N2}). " +
                   $"Verifique depósitos en tránsito, pagos pendientes o comisiones.";
        }

        // 1. Emparejar los movimientos marcados en el corte
        if (vm.MovimientosSeleccionadosIds != null && vm.MovimientosSeleccionadosIds.Count > 0)
        {
            var movsEmparejados = await _db.MovimientosBancarios
                .Where(m => m.CuentaBancariaId == cuenta.Id && vm.MovimientosSeleccionadosIds.Contains(m.Id))
                .ToListAsync();

            foreach (var m in movsEmparejados)
                m.Conciliado = true;
        }

        // 2. Si existen comisiones bancarias en el extracto, registrar el egreso y el asiento (6391 vs 1041)
        if (comisiones > 0)
        {
            if (cuenta.SaldoDisponible < comisiones)
                return $"La cuenta no tiene saldo disponible suficiente para cubrir S/ {comisiones:N2} de comisiones bancarias.";

            cuenta.SaldoContable -= comisiones;
            cuenta.SaldoDisponible -= comisiones;

            var movComision = new MovimientoBancario
            {
                CuentaBancariaId = cuenta.Id,
                Fecha = DateTime.Now,
                TipoMovimiento = "Egreso",
                MedioPago = "Transferencia",
                NumeroOperacion = "COM-" + DateTime.Now.ToString("MMddHHmmss"),
                Monto = comisiones,
                ReferenciaDocumento = $"CONCILIACION-{vm.FechaCorte:yyyyMMdd}",
                Conciliado = true
            };
            _db.MovimientosBancarios.Add(movComision);

            // Asiento contable: Debe 6391 (GastosBancarios) / Haber 1041 (Bancos)
            await _contabilidad.RegistrarAsiento(
                "Conciliacion",
                cuenta.Id,
                $"Comisiones bancarias conciliación {cuenta.Banco} {cuenta.NumeroCuenta}",
                usuarioId,
                (Cuentas.GastosBancarios, comisiones, 0m),
                (Cuentas.Bancos, 0m, comisiones)
            );
        }

        // 3. Registrar la conciliación cerrada
        _db.ConciliacionesBancarias.Add(new ConciliacionBancaria
        {
            CuentaBancariaId = cuenta.Id,
            FechaCorte = vm.FechaCorte,
            SaldoExtractoBanco = extracto,
            DepositosEnTransito = transito,
            PagosPendientes = pendientes,
            ComisionesBancarias = comisiones,
            SaldoContableAjustado = saldoContableAjustado,
            Estado = "Cerrada",
            UsuarioId = usuarioId
        });

        // Un único SaveChangesAsync para toda la operación (Regla 4 del README)
        await _db.SaveChangesAsync();
        return null;
    }
}