using AgroERP.Models.Contabilidad;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data.Seed;

public static class PlanCuentaSeed
{
    public static async Task EjecutarAsync(ApplicationDbContext db)
    {
        if (await db.Set<PlanCuenta>().AnyAsync()) return; // solo la primera vez

        db.AddRange(
            new PlanCuenta { Codigo = Cuentas.FacturasPorCobrar, Nombre = "Facturas por cobrar", TipoCuenta = "Activo" },
            new PlanCuenta { Codigo = Cuentas.Mercaderias, Nombre = "Mercaderías", TipoCuenta = "Activo" },
            new PlanCuenta { Codigo = Cuentas.ProductoTerminado, Nombre = "Productos terminados (fresa)", TipoCuenta = "Activo" },
            new PlanCuenta { Codigo = Cuentas.MaterialesAux, Nombre = "Materiales auxiliares (agroquímicos)", TipoCuenta = "Activo" },
            new PlanCuenta { Codigo = Cuentas.Envases, Nombre = "Envases y embalajes (jabas)", TipoCuenta = "Activo" },
            new PlanCuenta { Codigo = Cuentas.IgvPorPagar, Nombre = "IGV por pagar", TipoCuenta = "Pasivo" },
            new PlanCuenta { Codigo = Cuentas.FacturasPorPagar, Nombre = "Facturas por pagar", TipoCuenta = "Pasivo" },
            new PlanCuenta { Codigo = Cuentas.FletesGasto, Nombre = "Fletes y transporte", TipoCuenta = "Gasto" },
            new PlanCuenta { Codigo = Cuentas.MermasGasto, Nombre = "Mermas", TipoCuenta = "Gasto" },
            new PlanCuenta { Codigo = Cuentas.CostoVentas, Nombre = "Costo de ventas", TipoCuenta = "Gasto" },
            new PlanCuenta { Codigo = Cuentas.VentasProductos, Nombre = "Ventas de productos terminados", TipoCuenta = "Ingreso" }
        );
        await db.SaveChangesAsync();
    }
}