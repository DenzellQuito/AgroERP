using System;

namespace AgroERP.ViewModels.Pagos;

public class CuentaPorPagarItemVM
{
    public int Id { get; set; }
    public string Proveedor { get; set; } = null!;
    public string TipoObligacion { get; set; } = null!;
    public string NumeroFactura { get; set; } = null!; // Agregado
    public DateTime FechaEmision { get; set; }         // Agregado
    public DateTime FechaVencimiento { get; set; }
    public string Prioridad { get; set; } = null!;     // Agregado
    public decimal MontoTotal { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = null!;
}