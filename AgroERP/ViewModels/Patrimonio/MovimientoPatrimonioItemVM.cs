using System.ComponentModel.DataAnnotations;

namespace AgroERP.ViewModels.Patrimonio;

// ===================== PANTALLA PRINCIPAL (CONTROL PATRIMONIAL) =====================
public class PatrimonioPanelVM
{
    // Tarjetas resumen
    public int TotalActivosVigentes { get; set; }
    public int TotalDadosDeBaja { get; set; }
    public int TransferenciasMes { get; set; }
    public decimal GastoMantenimientoMes { get; set; }
    public decimal MejorasCapitalizadasMes { get; set; }

    public List<ActivoPatrimonioItemVM> Activos { get; set; } = new();
    public List<TransferenciaItemVM> Transferencias { get; set; } = new();
    public List<MantenimientoItemVM> Mantenimientos { get; set; } = new();
    public List<BajaItemVM> Bajas { get; set; } = new();
    public List<MovimientoPatrimonioItemVM> UltimosMovimientos { get; set; } = new();

    // Combos de los formularios
    public List<OpcionVM> Zonas { get; set; } = new();
    public List<OpcionTextoVM> Usuarios { get; set; } = new();
    public List<OpcionVM> Proveedores { get; set; } = new();
}

public class ActivoPatrimonioItemVM
{
    public int Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Categoria { get; set; } = null!;
    public int ZonaUbicacionId { get; set; }
    public string Ubicacion { get; set; } = null!;
    public string UsuarioResponsableId { get; set; } = null!;
    public string Responsable { get; set; } = null!;
    public decimal CostoAdquisicion { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public decimal ValorNetoLibros { get; set; }
    public string Estado { get; set; } = null!;           // EnProceso | Activo | DadoDeBaja
}

/// Línea del historial unificado (traslados, mantenimientos y bajas).
public class MovimientoPatrimonioItemVM
{
    public int Id { get; set; }
    public string CodigoActivo { get; set; } = null!;
    public string TipoMovimiento { get; set; } = null!;   // Transferencia | Preventivo | Correctivo | MejoraCapitalizable | Baja
    public DateTime Fecha { get; set; }
    public string Detalle { get; set; } = null!;
    public decimal? Monto { get; set; }
}

public class TransferenciaItemVM
{
    public int Id { get; set; }
    public string CodigoActivo { get; set; } = null!;
    public string NombreActivo { get; set; } = null!;
    public DateTime FechaTransferencia { get; set; }
    public string ZonaOrigen { get; set; } = null!;
    public string ZonaDestino { get; set; } = null!;
    public string ResponsableAnterior { get; set; } = null!;
    public string ResponsableNuevo { get; set; } = null!;
    public string Motivo { get; set; } = null!;
}

public class MantenimientoItemVM
{
    public int Id { get; set; }
    public string CodigoActivo { get; set; } = null!;
    public string NombreActivo { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = null!;             // Preventivo | Correctivo | MejoraCapitalizable
    public string Proveedor { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public decimal Costo { get; set; }
    public string Usuario { get; set; } = null!;
}

public class BajaItemVM
{
    public int Id { get; set; }
    public string CodigoActivo { get; set; } = null!;
    public string NombreActivo { get; set; } = null!;
    public DateTime FechaBaja { get; set; }
    public string MotivoBaja { get; set; } = null!;       // Desuso | Retiro | Siniestro
    public decimal CostoHistorico { get; set; }
    public decimal DepreciacionAcumuladaAlRetiro { get; set; }
    public decimal ValorNetoEnBaja { get; set; }
    public string Usuario { get; set; } = null!;
}

public class OpcionVM
{
    public int Id { get; set; }
    public string Texto { get; set; } = null!;
}

public class OpcionTextoVM
{
    public string Id { get; set; } = null!;
    public string Texto { get; set; } = null!;
}

// ===================== FORMULARIOS =====================
public class RegistrarTransferenciaVM
{
    [Required(ErrorMessage = "Seleccione el activo.")]
    public int ActivoFijoId { get; set; }

    [Required(ErrorMessage = "Seleccione la zona de destino.")]
    public int ZonaDestinoId { get; set; }

    [Required(ErrorMessage = "Seleccione el nuevo responsable.")]
    public string ResponsableNuevoId { get; set; } = null!;

    [Required(ErrorMessage = "Indique el motivo del traslado.")]
    [StringLength(250)]
    public string Motivo { get; set; } = null!;
}

public class RegistrarMantenimientoVM
{
    [Required(ErrorMessage = "Seleccione el activo.")]
    public int ActivoFijoId { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de mantenimiento.")]
    public string Tipo { get; set; } = "Preventivo";      // Preventivo | Correctivo | MejoraCapitalizable

    [Required(ErrorMessage = "Seleccione el proveedor del servicio.")]
    public int ProveedorId { get; set; }

    [Required(ErrorMessage = "Ingrese el número de factura del proveedor.")]
    [StringLength(30)]
    public string NumeroFactura { get; set; } = null!;

    [Required(ErrorMessage = "Describa el servicio realizado.")]
    [StringLength(250)]
    public string Descripcion { get; set; } = null!;

    [Range(0.01, 999999999, ErrorMessage = "El costo debe ser mayor a cero.")]
    public decimal Costo { get; set; }

    [Range(0, 365, ErrorMessage = "Los días de crédito deben estar entre 0 y 365.")]
    public int DiasVencimiento { get; set; } = 30;
}

public class RegistrarBajaVM
{
    [Required(ErrorMessage = "Seleccione el activo.")]
    public int ActivoFijoId { get; set; }

    [Required(ErrorMessage = "Seleccione el motivo de la baja.")]
    public string MotivoBaja { get; set; } = "Desuso";    // Desuso | Retiro | Siniestro
}
