namespace AgroERP.ViewModels.Cobranzas;

public class CuentaPorCobrarItemVM
{
    public int Id { get; set; }
    public int ComprobantePagoId { get; set; }
    public int PedidoVentaId { get; set; }
    public string Cliente { get; set; } = null!;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = null!;
    public int? UltimaCobranzaId { get; set; }
    public int? UltimaNotaCreditoId { get; set; }
}

public class ReciboCobranzaPdfVM
{
    public int CobranzaId { get; set; }
    public int CuentaPorCobrarId { get; set; }
    public int ComprobantePagoId { get; set; }
    public string Cliente { get; set; } = "";
    public string ClienteRuc { get; set; } = "";
    public string CuentaBancaria { get; set; } = "";
    public DateTime FechaCobro { get; set; }
    public string MedioPago { get; set; } = "";
    public string NumeroOperacion { get; set; } = "";
    public decimal MontoTotalFactura { get; set; }
    public decimal MontoCobrado { get; set; }
    public decimal SaldoRestante { get; set; }
    public string EstadoCxC { get; set; } = "";
    public string Usuario { get; set; } = "";
}

public class AntiguedadSaldosItemVM
{
    public int CuentaPorCobrarId { get; set; }
    public int ComprobantePagoId { get; set; }
    public string Cliente { get; set; } = "";
    public string ClienteRuc { get; set; } = "";
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int DiasAtraso { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Tramo { get; set; } = "";
}

public class AntiguedadSaldosPdfVM
{
    public DateTime FechaCorte { get; set; }
    public decimal TotalVigente { get; set; }
    public decimal TotalVencido1a30 { get; set; }
    public decimal TotalVencidoMas30 { get; set; }
    public decimal TotalCarteraPendiente { get; set; }
    public List<AntiguedadSaldosItemVM> Items { get; set; } = new();
}