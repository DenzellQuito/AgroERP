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
    public string ReferenciaModulo { get; set; } = null!; // "Recepcion", "Merma", "Factura"
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
    public const string Mercaderias       = "2011";
    public const string MaterialesAux     = "2511"; // agroquímicos
    public const string Envases = "2611"; // jabas
    public const string ProductoTerminado = "2111";
    public const string IgvPorPagar       = "4011";
    public const string FacturasPorPagar  = "4212";
    public const string FacturasPorCobrar = "1212";
    public const string VentasProductos   = "7011";
    public const string CostoVentas = "6921";
    public const string MermasGasto       = "6598";
    public const string FletesGasto = "6311";
}

public static class TiposCuenta
{
    public const string Activo = "Activo", Pasivo = "Pasivo", Patrimonio = "Patrimonio",
                        Ingreso = "Ingreso", Gasto = "Gasto";
}