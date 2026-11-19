using AgroERP.Models.Calidad_WMS;
using AgroERP.Models.Contabilidad;
using AgroERP.Models.Maestros;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.Seed;

public static class PlanCuentaSeed
{
    public static async Task EjecutarAsync(ApplicationDbContext db)
    {
        var codigosExistentes = await db.Set<PlanCuenta>()
            .Select(c => c.Codigo)
            .ToListAsync();

        var todasLasCuentas = new List<PlanCuenta>
        {
            // Fase 1: Logística
            new() { Codigo = Cuentas.FacturasPorCobrar, Nombre = "Facturas por cobrar", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.Mercaderias, Nombre = "Mercaderías", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.ProductoTerminado, Nombre = "Productos terminados (fresa)", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.MaterialesAux, Nombre = "Materiales auxiliares (agroquímicos)", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.Envases, Nombre = "Envases y embalajes (jabas)", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.IgvPorPagar, Nombre = "IGV por pagar", TipoCuenta = "Pasivo" },
            new() { Codigo = Cuentas.FacturasPorPagar, Nombre = "Facturas por pagar", TipoCuenta = "Pasivo" },
            new() { Codigo = Cuentas.FletesGasto, Nombre = "Fletes y transporte", TipoCuenta = "Gasto" },
            new() { Codigo = Cuentas.MermasGasto, Nombre = "Mermas", TipoCuenta = "Gasto" },
            new() { Codigo = Cuentas.CostoVentas, Nombre = "Costo de ventas", TipoCuenta = "Gasto" },
            new() { Codigo = Cuentas.VentasProductos, Nombre = "Ventas de productos terminados", TipoCuenta = "Ingreso" },

            // Fase 2: Tesorería y Activos Fijos
            new() { Codigo = Cuentas.Bancos, Nombre = "Cuentas corrientes operativas (Bancos)", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.MaquinariaEquipo, Nombre = "Inmuebles, maquinaria y equipo", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.DepreciacionAcumulada, Nombre = "Depreciación acumulada IME", TipoCuenta = "Activo" },
            new() { Codigo = Cuentas.PasivoActivoFijo, Nombre = "Cuentas por pagar diversas - Activo fijo", TipoCuenta = "Pasivo" },
            new() { Codigo = Cuentas.MantenimientoGasto, Nombre = "Mantenimiento y reparaciones", TipoCuenta = "Gasto" },
            new() { Codigo = Cuentas.GastosBancarios, Nombre = "Gastos y comisiones bancarias", TipoCuenta = "Gasto" },
            new() { Codigo = Cuentas.CostoNetoBajaActivo, Nombre = "Costo neto de baja de activos", TipoCuenta = "Gasto" },
            new() { Codigo = Cuentas.DepreciacionGasto, Nombre = "Depreciación de propiedad, planta y equipo", TipoCuenta = "Gasto" }
        };

        var faltantes = todasLasCuentas
            .Where(c => !codigosExistentes.Contains(c.Codigo))
            .ToList();

        if (faltantes.Count > 0)
        {
            db.AddRange(faltantes);
            await db.SaveChangesAsync();
        }

        await SembrarFresaInicialAsync(db);
    }

    private static async Task SembrarFresaInicialAsync(ApplicationDbContext db)
    {
        var fresa = await db.Articulos.FirstOrDefaultAsync(a => a.Tipo == "Fresa");
        if (fresa == null)
        {
            fresa = new Articulo
            {
                Codigo = "FRE-001",
                Nombre = "Fresa fresca",
                Tipo = "Fresa",
                UnidadMedida = "Jaba"
            };
            db.Articulos.Add(fresa);
            await db.SaveChangesAsync();
        }

        var almacen = await db.Almacenes.FirstOrDefaultAsync(a => a.Tipo == "AcopioFresa");
        if (almacen == null)
        {
            almacen = new Almacen
            {
                Nombre = "Cámara de Acopio de Fresa",
                Tipo = "AcopioFresa"
            };
            db.Almacenes.Add(almacen);
            await db.SaveChangesAsync();
        }

        var zona = await db.ZonasUbicacion.FirstOrDefaultAsync(z => z.AlmacenId == almacen.Id);
        if (zona == null)
        {
            zona = new ZonaUbicacion
            {
                AlmacenId = almacen.Id,
                CodigoUbicacion = "ZN-ACOPIO-01"
            };
            db.ZonasUbicacion.Add(zona);
            await db.SaveChangesAsync();
        }

        if (!await db.InventariosExistencia.AnyAsync(i => i.Lote.Articulo.Tipo == "Fresa"))
        {
            var lote = await db.Lotes.FirstOrDefaultAsync(l => l.ArticuloId == fresa.Id && l.NumeroLote == "LOT-FRESA-INI");
            if (lote == null)
            {
                lote = new Lote
                {
                    ArticuloId = fresa.Id,
                    NumeroLote = "LOT-FRESA-INI",
                    FechaVencimiento = DateTime.Today.AddDays(30),
                    EstadoCalidad = "Liberado"
                };
                db.Lotes.Add(lote);
                await db.SaveChangesAsync();
            }

            db.InventariosExistencia.Add(new InventarioExistencia
            {
                LoteId = lote.Id,
                ZonaUbicacionId = zona.Id,
                CantidadFisica = 500m,
                CantidadReservada = 0m
            });

            db.MovimientosKardex.Add(new MovimientoKardex
            {
                LoteId = lote.Id,
                Fecha = DateTime.Now,
                TipoMovimiento = "Ingreso",
                Cantidad = 500m,
                ZonaDestinoId = zona.Id,
                ReferenciaDocumento = "COSECHA-INICIAL"
            });

            await db.SaveChangesAsync();
        }

        if (!await db.Clientes.AnyAsync())
        {
            db.Clientes.AddRange(
                new Cliente { RucDni = "20501234567", RazonSocial = "Supermercados Peruanos S.A.", LimiteCredito = 25000m },
                new Cliente { RucDni = "20609876543", RazonSocial = "Agroexportadora del Norte S.A.C.", LimiteCredito = 35000m }
            );
            await db.SaveChangesAsync();
        }
    }
}