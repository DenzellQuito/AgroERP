using AgroERP.Models.Calidad_WMS;
using AgroERP.Models.BI_Gestion;
using AgroERP.Models.Compras;
using AgroERP.Models.Contabilidad;
using AgroERP.Models.Distribucion;
using AgroERP.Models.Maestros;
using AgroERP.Models.MRP_IA;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgroERP.Data
{
    

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<IndicadorKPI> IndicadoresKPI { get; set; }
        public DbSet<PlanAccionMejora> PlanesAccionMejora { get; set; }
        public DbSet<GastoLogistico> GastosLogisticos { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<ZonaUbicacion> ZonasUbicacion { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<RecepcionMercaderia> RecepcionesMercaderia { get; set; }
        public DbSet<RecepcionDetalle> DetallesRecepcion { get; set; }
        public DbSet<InspeccionCalidad> InspeccionesCalidad { get; set; }
        public DbSet<InventarioExistencia> InventariosExistencia { get; set; }
        public DbSet<ReservaInventario> ReservasInventario { get; set; }
        public DbSet<MovimientoKardex> MovimientosKardex { get; set; }
        public DbSet<AjusteInventario> AjustesInventario { get; set; }
        public DbSet<RequerimientoCompra> RequerimientosCompra { get; set; }
        public DbSet<RequerimientoDetalle> DetallesRequerimiento { get; set; }
        public DbSet<CotizacionProveedor> CotizacionesProveedor { get; set; }
        public DbSet<OrdenCompra> OrdenesCompra { get; set; }
        public DbSet<OrdenCompraDetalle> DetallesOrdenCompra { get; set; }
        public DbSet<PlanCuenta> PlanCuentas { get; set; }
        public DbSet<AsientoContable> AsientosContables { get; set; }
        public DbSet<AsientoDetalle> DetallesAsiento { get; set; }
        public DbSet<PedidoVenta> PedidosVenta { get; set; }
        public DbSet<PedidoVentaDetalle> DetallesPedidoVenta { get; set; }
        public DbSet<OrdenPicking> OrdenesPicking { get; set; }
        public DbSet<OrdenPickingDetalle> DetallesOrdenPicking { get; set; }
        public DbSet<GuiaRemisionRemitente> GuiasRemisionRemitente { get; set; }
        public DbSet<ComprobantePago> ComprobantesPago { get; set; }
        public DbSet<DevolucionCliente> DevolucionesCliente { get; set; }
        public DbSet<NotaCredito> NotaCredito { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ParametroInventario> ParametrosInventario { get; set; }
        public DbSet<CampaniaAgricola> CampaniasAgricolas { get; set; }
        public DbSet<PronosticoDemanda> PronosticosDemanda { get; set; }
        public DbSet<RecetaAgricolaBOM> RecetasAgricolasBOM { get; set; }
        public DbSet<NecesidadMrp> NecesidadesMrp { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Obligatorio para que funcione el Login/Identity

            // Desactivar borrado en cascada para evitar que alguien borre un Lote y desaparezca el Kardex
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            // Esto le dice a EF Core que TODOS los campos decimal del sistema usarán 4 decimales
            builder.Properties<decimal>().HavePrecision(18, 4);
        }
    }
    
}
