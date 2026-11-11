using AgroERP.Models.Contabilidad;
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
    }
}