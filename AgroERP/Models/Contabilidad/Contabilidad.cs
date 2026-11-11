namespace AgroERP.Models.Contabilidad;

public class PlanCuenta
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;          // único, 10
    public string Nombre { get; set; } = null!;          // 150
    public string TipoCuenta { get; set; } = null!;
    public bool EsImputable { get; set; } = true;
    public bool Activa { get; set; } = true;
}

public class AsientoContable
{
    public int Id { get; set; }
    public DateTime FechaContable { get; set; }
    public string Glosa { get; set; } = null!;           // 250
    public string ReferenciaModulo { get; set; } = null!; // "Recepcion", "Merma", "Factura", "Cobranza", "PagoProveedor", etc.
    public int? ReferenciaId { get; set; }
    public string UsuarioId { get; set; } = null!;       // FK AspNetUsers
    public List<AsientoDetalle> Detalles { get; set; } = new();
}

public class AsientoDetalle
{
    public int Id { get; set; }
    public int AsientoId { get; set; }
    public AsientoContable Asiento { get; set; } = null!;
    public int CuentaId { get; set; }
    public PlanCuenta Cuenta { get; set; } = null!;
    public decimal Debe { get; set; }                    // (18,2)
    public decimal Haber { get; set; }                   // (18,2)
}

// Constantes para que ningún módulo escriba códigos a mano
public static class Cuentas
{
    // Fase 1: Logística
    public const string Mercaderias = "2011";
    public const string MaterialesAux = "2511"; // agroquímicos
    public const string Envases = "2611"; // jabas
    public const string ProductoTerminado = "2111";
    public const string IgvPorPagar = "4011";
    public const string FacturasPorPagar = "4212";
    public const string FacturasPorCobrar = "1212";
    public const string VentasProductos = "7011";
    public const string CostoVentas = "6921";
    public const string MermasGasto = "6598";
    public const string FletesGasto = "6311";

    // Fase 2: Tesorería y Activos Fijos
    public const string Bancos = "1041"; // Efectivo y equivalentes - Cuentas corrientes
    public const string MaquinariaEquipo = "3331"; // Inmuebles, maquinaria y equipo
    public const string DepreciacionAcumulada = "3913"; // Depreciación acumulada IME
    public const string PasivoActivoFijo = "4654"; // Cuentas por pagar diversas - Activo fijo
    public const string MantenimientoGasto = "6341"; // Mantenimiento y reparaciones
    public const string GastosBancarios = "6391"; // Gastos y comisiones bancarias
    public const string CostoNetoBajaActivo = "6551"; // Costo neto de enajenación / baja de activos
    public const string DepreciacionGasto = "6814"; // Depreciación de propiedad, planta y equipo
}

public static class TiposCuenta
{
    public const string Activo = "Activo", Pasivo = "Pasivo", Patrimonio = "Patrimonio",
                        Ingreso = "Ingreso", Gasto = "Gasto";
}