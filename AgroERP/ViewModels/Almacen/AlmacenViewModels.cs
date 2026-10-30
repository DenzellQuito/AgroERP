using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Almacen;

// Las vistas reciben exclusivamente ViewModels, nunca entidades EF directamente.
public class AlmacenDashboardViewModel
{
    public List<AlmacenItemViewModel> Almacenes { get; set; } = new();
    public List<ZonaItemViewModel> Zonas { get; set; } = new();
    public List<ExistenciaItemViewModel> Existencias { get; set; } = new();
    public List<MovimientoItemViewModel> Movimientos { get; set; } = new();
    public List<PickingItemViewModel> OrdenesPicking { get; set; } = new();
    public decimal StockTotal => Existencias.Sum(x => x.CantidadFisica);
    public decimal StockReservado => Existencias.Sum(x => x.CantidadReservada);
    public int LotesPorVencer => Existencias.Count(x => x.FechaVencimiento.Date <= DateTime.Today.AddDays(30));
}
public record AlmacenItemViewModel(int Id, string Nombre, string Tipo, int CantidadZonas);
public record ZonaItemViewModel(int Id, int AlmacenId, string Almacen, string Codigo);
public record ExistenciaItemViewModel(int LoteId, string Articulo, string NumeroLote, DateTime FechaVencimiento, string EstadoCalidad, int ZonaId, string Zona, string Almacen, decimal CantidadFisica, decimal CantidadReservada, decimal Disponible);
public record MovimientoItemViewModel(DateTime Fecha, string Articulo, string Lote, string Tipo, decimal Cantidad, string? Origen, string? Destino, string? Referencia);
public record PickingItemViewModel(int Id, int PedidoId, string OperarioId, int Detalles, decimal CantidadTotal);

public class AlmacenFormViewModel
{
    [Required, StringLength(120)] public string Nombre { get; set; } = string.Empty;
    [Required] public string Tipo { get; set; } = "Insumos";
}
public class ZonaFormViewModel
{
    [Range(1, int.MaxValue)] public int AlmacenId { get; set; }
    [Required, StringLength(40)] public string CodigoUbicacion { get; set; } = string.Empty;
}
public class TrasladoFormViewModel
{
    [Range(1, int.MaxValue)] public int LoteId { get; set; }
    [Range(1, int.MaxValue)] public int ZonaOrigenId { get; set; }
    [Range(1, int.MaxValue)] public int ZonaDestinoId { get; set; }
    [Range(typeof(decimal), "0.0001", "999999999")] public decimal Cantidad { get; set; }
    [StringLength(100)] public string Referencia { get; set; } = "Traslado interno";
}
public class MermaFormViewModel
{
    [Range(1, int.MaxValue)] public int LoteId { get; set; }
    [Range(1, int.MaxValue)] public int ZonaId { get; set; }
    [Range(typeof(decimal), "0.0001", "999999999")] public decimal Cantidad { get; set; }
    [Range(typeof(decimal), "0", "999999999")] public decimal CostoUnitario { get; set; }
    [Required, StringLength(250)] public string Motivo { get; set; } = string.Empty;
}
