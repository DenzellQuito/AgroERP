using AgroERP.Models.ActivosFijos;
using AgroERP.Models.Contabilidad;
using AgroERP.ViewModels.Patrimonio;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.AccesoDatos;

/// DUEÑO: Méndez. Escribe TransferenciasActivo, MantenimientosActivo, BajasActivo.
/// Solo LEE ActivosFijos: cualquier cambio en el activo se hace vía contratos de DAActivos (Lavado).
/// Las obligaciones con proveedores se crean vía DAPagos.CrearCuentaPorPagar (Changa).
public class DAPatrimonio
{
    private readonly ApplicationDbContext _db;
    private readonly DAActivos _activos;
    private readonly DAPagos _pagos;
    private readonly DAContabilidad _contabilidad;

    private static readonly string[] TiposMantenimiento = { "Preventivo", "Correctivo", "MejoraCapitalizable" };
    private static readonly string[] MotivosBaja = { "Desuso", "Retiro", "Siniestro" };

    public DAPatrimonio(ApplicationDbContext db, DAActivos activos, DAPagos pagos, DAContabilidad contabilidad)
    {
        _db = db;
        _activos = activos;
        _pagos = pagos;
        _contabilidad = contabilidad;
    }

    private static decimal R2(decimal x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);

    // ===================== PANEL PRINCIPAL =====================
    public async Task<PatrimonioPanelVM> ObtenerPanelAsync()
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        // Nombres de usuario (custodios) para mostrar en lugar del Id
        var usuarios = await _db.Users
            .OrderBy(u => u.UserName)
            .Select(u => new OpcionTextoVM { Id = u.Id, Texto = u.UserName ?? u.Id })
            .ToListAsync();
        var nombreUsuario = usuarios.ToDictionary(u => u.Id, u => u.Texto);
        string Nombre(string? id) => id != null && nombreUsuario.TryGetValue(id, out var n) ? n : (id ?? "-");

        var activos = await _db.ActivosFijos
            .AsNoTracking()
            .OrderBy(a => a.Codigo)
            .Select(a => new ActivoPatrimonioItemVM
            {
                Id = a.Id,
                Codigo = a.Codigo,
                Nombre = a.Nombre,
                Categoria = a.Categoria,
                ZonaUbicacionId = a.ZonaUbicacionId,
                Ubicacion = a.ZonaUbicacion.Almacen.Nombre + " / " + a.ZonaUbicacion.CodigoUbicacion,
                UsuarioResponsableId = a.UsuarioResponsableId,
                CostoAdquisicion = a.CostoAdquisicion,
                DepreciacionAcumulada = a.DepreciacionAcumulada,
                ValorNetoLibros = a.ValorNetoLibros,
                Estado = a.Estado
            }).ToListAsync();
        foreach (var a in activos) a.Responsable = Nombre(a.UsuarioResponsableId);

        var transferencias = await _db.TransferenciasActivo
            .AsNoTracking()
            .OrderByDescending(t => t.FechaTransferencia).ThenByDescending(t => t.Id)
            .Take(50)
            .Select(t => new
            {
                t.Id,
                t.ActivoFijo.Codigo,
                t.ActivoFijo.Nombre,
                t.FechaTransferencia,
                Origen = t.ZonaOrigen.Almacen.Nombre + " / " + t.ZonaOrigen.CodigoUbicacion,
                Destino = t.ZonaDestino.Almacen.Nombre + " / " + t.ZonaDestino.CodigoUbicacion,
                t.ResponsableAnteriorId,
                t.ResponsableNuevoId,
                t.Motivo
            }).ToListAsync();

        var mantenimientos = await _db.MantenimientosActivo
            .AsNoTracking()
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .Take(50)
            .Select(m => new
            {
                m.Id,
                m.ActivoFijo.Codigo,
                m.ActivoFijo.Nombre,
                m.Fecha,
                m.Tipo,
                Proveedor = m.Proveedor != null ? m.Proveedor.RazonSocial : "-",
                m.Descripcion,
                m.Costo,
                m.UsuarioId
            }).ToListAsync();

        var bajas = await _db.BajasActivo
            .AsNoTracking()
            .OrderByDescending(b => b.FechaBaja).ThenByDescending(b => b.Id)
            .Take(50)
            .Select(b => new
            {
                b.Id,
                b.ActivoFijo.Codigo,
                b.ActivoFijo.Nombre,
                b.FechaBaja,
                b.MotivoBaja,
                b.CostoHistorico,
                b.DepreciacionAcumuladaAlRetiro,
                b.ValorNetoEnBaja,
                b.UsuarioId
            }).ToListAsync();

        var vm = new PatrimonioPanelVM
        {
            Activos = activos,
            TotalActivosVigentes = activos.Count(a => a.Estado != "DadoDeBaja"),
            TotalDadosDeBaja = activos.Count(a => a.Estado == "DadoDeBaja"),
            TransferenciasMes = await _db.TransferenciasActivo.CountAsync(t => t.FechaTransferencia >= inicioMes),
            GastoMantenimientoMes = await _db.MantenimientosActivo
                .Where(m => m.Fecha >= inicioMes && m.Tipo != "MejoraCapitalizable")
                .SumAsync(m => (decimal?)m.Costo) ?? 0m,
            MejorasCapitalizadasMes = await _db.MantenimientosActivo
                .Where(m => m.Fecha >= inicioMes && m.Tipo == "MejoraCapitalizable")
                .SumAsync(m => (decimal?)m.Costo) ?? 0m,

            Transferencias = transferencias.Select(t => new TransferenciaItemVM
            {
                Id = t.Id,
                CodigoActivo = t.Codigo,
                NombreActivo = t.Nombre,
                FechaTransferencia = t.FechaTransferencia,
                ZonaOrigen = t.Origen,
                ZonaDestino = t.Destino,
                ResponsableAnterior = Nombre(t.ResponsableAnteriorId),
                ResponsableNuevo = Nombre(t.ResponsableNuevoId),
                Motivo = t.Motivo
            }).ToList(),

            Mantenimientos = mantenimientos.Select(m => new MantenimientoItemVM
            {
                Id = m.Id,
                CodigoActivo = m.Codigo,
                NombreActivo = m.Nombre,
                Fecha = m.Fecha,
                Tipo = m.Tipo,
                Proveedor = m.Proveedor,
                Descripcion = m.Descripcion,
                Costo = m.Costo,
                Usuario = Nombre(m.UsuarioId)
            }).ToList(),

            Bajas = bajas.Select(b => new BajaItemVM
            {
                Id = b.Id,
                CodigoActivo = b.Codigo,
                NombreActivo = b.Nombre,
                FechaBaja = b.FechaBaja,
                MotivoBaja = b.MotivoBaja,
                CostoHistorico = b.CostoHistorico,
                DepreciacionAcumuladaAlRetiro = b.DepreciacionAcumuladaAlRetiro,
                ValorNetoEnBaja = b.ValorNetoEnBaja,
                Usuario = Nombre(b.UsuarioId)
            }).ToList(),

            Zonas = await _db.ZonasUbicacion
                .OrderBy(z => z.Almacen.Nombre).ThenBy(z => z.CodigoUbicacion)
                .Select(z => new OpcionVM { Id = z.Id, Texto = z.Almacen.Nombre + " / " + z.CodigoUbicacion })
                .ToListAsync(),

            Usuarios = usuarios,

            Proveedores = await _db.Proveedores
                .OrderBy(p => p.RazonSocial)
                .Select(p => new OpcionVM { Id = p.Id, Texto = p.RazonSocial + " (RUC " + p.Ruc + ")" })
                .ToListAsync()
        };

        // Historial unificado (últimos 15 eventos patrimoniales)
        vm.UltimosMovimientos = vm.Transferencias.Select(t => new MovimientoPatrimonioItemVM
            {
                Id = t.Id,
                CodigoActivo = t.CodigoActivo,
                TipoMovimiento = "Transferencia",
                Fecha = t.FechaTransferencia,
                Detalle = $"{t.ZonaOrigen} → {t.ZonaDestino} · {t.ResponsableAnterior} → {t.ResponsableNuevo}"
            })
            .Concat(vm.Mantenimientos.Select(m => new MovimientoPatrimonioItemVM
            {
                Id = m.Id,
                CodigoActivo = m.CodigoActivo,
                TipoMovimiento = m.Tipo,
                Fecha = m.Fecha,
                Detalle = $"{m.Descripcion} · {m.Proveedor}",
                Monto = m.Costo
            }))
            .Concat(vm.Bajas.Select(b => new MovimientoPatrimonioItemVM
            {
                Id = b.Id,
                CodigoActivo = b.CodigoActivo,
                TipoMovimiento = "Baja",
                Fecha = b.FechaBaja,
                Detalle = $"Motivo: {b.MotivoBaja} · VNL en baja S/ {b.ValorNetoEnBaja:N2}",
                Monto = b.CostoHistorico
            }))
            .OrderByDescending(x => x.Fecha)
            .Take(15)
            .ToList();

        return vm;
    }

    // ===================== 1. TRASLADO DE ACTIVO (UBICACIÓN Y CUSTODIO) =====================
    /// Inserta TransferenciaActivo + invoca DAActivos.ActualizarUbicacionYCustodio. Un solo SaveChangesAsync.
    public async Task<string?> RegistrarTransferenciaAsync(RegistrarTransferenciaVM vm, string usuarioId)
    {
        try
        {
            var activo = await _db.ActivosFijos.FindAsync(vm.ActivoFijoId);
            if (activo == null) return "El activo seleccionado no existe.";
            if (activo.Estado == "DadoDeBaja") return $"El activo {activo.Codigo} está dado de baja y no puede trasladarse.";

            if (!await _db.ZonasUbicacion.AnyAsync(z => z.Id == vm.ZonaDestinoId))
                return "La zona de destino no existe.";
            if (!await _db.Users.AnyAsync(u => u.Id == vm.ResponsableNuevoId))
                return "El nuevo responsable no existe.";

            if (activo.ZonaUbicacionId == vm.ZonaDestinoId && activo.UsuarioResponsableId == vm.ResponsableNuevoId)
                return "El activo ya se encuentra en esa ubicación y con ese responsable.";

            _db.TransferenciasActivo.Add(new TransferenciaActivo
            {
                ActivoFijoId = activo.Id,
                FechaTransferencia = DateTime.Now,
                ZonaOrigenId = activo.ZonaUbicacionId,          // se toma ANTES de llamar al contrato
                ZonaDestinoId = vm.ZonaDestinoId,
                ResponsableAnteriorId = activo.UsuarioResponsableId,
                ResponsableNuevoId = vm.ResponsableNuevoId,
                Motivo = vm.Motivo.Trim()
            });

            // Contrato de Lavado (no guarda)
            await _activos.ActualizarUbicacionYCustodio(activo.Id, vm.ZonaDestinoId, vm.ResponsableNuevoId);

            await _db.SaveChangesAsync();
            return null;
        }
        catch (NotImplementedException ex) { return ex.Message; }
        catch (Exception ex) { return ex.InnerException?.Message ?? ex.Message; }
    }

    // ===================== 2. MANTENIMIENTO / MEJORA CAPITALIZABLE =====================
    /// Preventivo/Correctivo: asiento 6341 vs 4212 + CxP "Mantenimiento".
    /// MejoraCapitalizable: DAActivos.CapitalizarMejora + asiento 3331 vs 4654 + CxP "ActivoFijo"
    /// (así el pago de Changa debita 4654, la misma cuenta que se abonó aquí).
    /// Todo en un solo SaveChangesAsync.
    public async Task<string?> RegistrarMantenimientoAsync(RegistrarMantenimientoVM vm, string usuarioId)
    {
        try
        {
            if (!TiposMantenimiento.Contains(vm.Tipo))
                return "Tipo de mantenimiento inválido. Use Preventivo, Correctivo o MejoraCapitalizable.";

            var costo = R2(vm.Costo);
            if (costo <= 0) return "El costo debe ser mayor a cero.";

            var activo = await _db.ActivosFijos.FindAsync(vm.ActivoFijoId);
            if (activo == null) return "El activo seleccionado no existe.";
            if (activo.Estado == "DadoDeBaja") return $"El activo {activo.Codigo} está dado de baja.";

            var esMejora = vm.Tipo == "MejoraCapitalizable";
            if (esMejora && activo.Estado != "Activo")
                return $"Solo se puede capitalizar una mejora en un activo en estado Activo (estado actual de {activo.Codigo}: {activo.Estado}).";

            var proveedor = await _db.Proveedores.FindAsync(vm.ProveedorId);
            if (proveedor == null) return "El proveedor seleccionado no existe.";

            var nroFactura = vm.NumeroFactura.Trim();
            var descripcion = vm.Descripcion.Trim();

            _db.MantenimientosActivo.Add(new MantenimientoActivo
            {
                ActivoFijoId = activo.Id,
                ProveedorId = proveedor.Id,
                Fecha = DateTime.Now,
                Tipo = vm.Tipo,
                Descripcion = descripcion,
                Costo = costo,
                UsuarioId = usuarioId
            });

            if (esMejora)
            {
                // Contrato de Lavado: suma a CostoAdquisicion y ValorNetoLibros (no guarda)
                await _activos.CapitalizarMejora(activo.Id, costo);

                await _contabilidad.RegistrarAsiento(
                    "MejoraActivo",
                    activo.Id,
                    $"Mejora capitalizable {activo.Codigo} - {descripcion} (Fact. {nroFactura})",
                    usuarioId,
                    (Cuentas.MaquinariaEquipo, costo, 0m),
                    (Cuentas.PasivoActivoFijo, 0m, costo));

                // Contrato de Changa (no guarda)
                await _pagos.CrearCuentaPorPagar(proveedor.Id, null, "ActivoFijo", nroFactura, costo, vm.DiasVencimiento);
            }
            else
            {
                await _contabilidad.RegistrarAsiento(
                    "MantenimientoActivo",
                    activo.Id,
                    $"Mantenimiento {vm.Tipo.ToLower()} {activo.Codigo} - {descripcion} (Fact. {nroFactura})",
                    usuarioId,
                    (Cuentas.MantenimientoGasto, costo, 0m),
                    (Cuentas.FacturasPorPagar, 0m, costo));

                await _pagos.CrearCuentaPorPagar(proveedor.Id, null, "Mantenimiento", nroFactura, costo, vm.DiasVencimiento);
            }

            await _db.SaveChangesAsync();
            return null;
        }
        catch (NotImplementedException ex) { return ex.Message; }
        catch (Exception ex) { return ex.InnerException?.Message ?? ex.Message; }
    }

    // ===================== 3. BAJA PATRIMONIAL =====================
    /// Inserta BajaActivo + DAActivos.MarcarActivoDadoDeBaja + asiento:
    /// Debe 3913 (Dep. acumulada) + 6551 (Costo neto) / Haber 3331 (Costo histórico). Un solo SaveChangesAsync.
    public async Task<string?> RegistrarBajaAsync(RegistrarBajaVM vm, string usuarioId)
    {
        try
        {
            if (!MotivosBaja.Contains(vm.MotivoBaja))
                return "Motivo de baja inválido. Use Desuso, Retiro o Siniestro.";

            var activo = await _db.ActivosFijos.FindAsync(vm.ActivoFijoId);
            if (activo == null) return "El activo seleccionado no existe.";
            if (activo.Estado == "DadoDeBaja") return $"El activo {activo.Codigo} ya fue dado de baja.";
            if (activo.Estado != "Activo")
                return $"El activo {activo.Codigo} aún no está capitalizado (estado {activo.Estado}); no se puede dar de baja contablemente.";

            var costo = R2(activo.CostoAdquisicion);
            var depAcum = R2(activo.DepreciacionAcumulada);
            var vnl = R2(costo - depAcum);   // Dep. acumulada + VNL = Costo (siempre cuadra)

            if (costo <= 0) return "El activo no tiene costo de adquisición registrado.";
            if (vnl < 0) return "La depreciación acumulada supera el costo del activo; revise la depreciación con Lavado.";

            _db.BajasActivo.Add(new BajaActivo
            {
                ActivoFijoId = activo.Id,
                FechaBaja = DateTime.Now,
                MotivoBaja = vm.MotivoBaja,
                CostoHistorico = costo,
                DepreciacionAcumuladaAlRetiro = depAcum,
                ValorNetoEnBaja = vnl,
                UsuarioId = usuarioId
            });

            // Contrato de Lavado: Estado = "DadoDeBaja" (no guarda)
            await _activos.MarcarActivoDadoDeBaja(activo.Id);

            // Se omiten líneas en cero (ej. activo sin depreciar o totalmente depreciado)
            var lineas = new List<(string cuenta, decimal debe, decimal haber)>();
            if (depAcum > 0) lineas.Add((Cuentas.DepreciacionAcumulada, depAcum, 0m));
            if (vnl > 0) lineas.Add((Cuentas.CostoNetoBajaActivo, vnl, 0m));
            lineas.Add((Cuentas.MaquinariaEquipo, 0m, costo));

            await _contabilidad.RegistrarAsiento(
                "BajaActivo",
                activo.Id,
                $"Baja de activo {activo.Codigo} - {activo.Nombre} por {vm.MotivoBaja}",
                usuarioId,
                lineas.ToArray());

            await _db.SaveChangesAsync();
            return null;
        }
        catch (NotImplementedException ex) { return ex.Message; }
        catch (Exception ex) { return ex.InnerException?.Message ?? ex.Message; }
    }
}
