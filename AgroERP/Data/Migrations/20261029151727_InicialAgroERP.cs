using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialAgroERP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.CreateTable(
                name: "Almacenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Almacenes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Articulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articulos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsientosContables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaContable = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Glosa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenciaModulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenciaId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientosContables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaniasAgricolas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HectareasActivas = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MesProyectado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaniasAgricolas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RucDni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LimiteCredito = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GastosLogisticos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Concepto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosLogisticos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndicadoresKPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreMetrica = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetaObjetivo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ToleranciaAlarma = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicadoresKPI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanCuentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoCuenta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsImputable = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanCuentas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ruc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NivelConfiabilidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequerimientosCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioSolicitanteId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequerimientosCompra", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZonasUbicacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    CodigoUbicacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonasUbicacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZonasUbicacion_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    NumeroLote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoCalidad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    StockSeguridad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LoteEconomico = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParametrosInventario_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecetasAgricolasBOM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloFinalId = table.Column<int>(type: "int", nullable: false),
                    ArticuloInsumoId = table.Column<int>(type: "int", nullable: false),
                    MesAplicacion = table.Column<int>(type: "int", nullable: false),
                    CantidadPorJaba = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetasAgricolasBOM", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecetasAgricolasBOM_Articulos_ArticuloFinalId",
                        column: x => x.ArticuloFinalId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecetasAgricolasBOM_Articulos_ArticuloInsumoId",
                        column: x => x.ArticuloInsumoId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PronosticosDemanda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaniaId = table.Column<int>(type: "int", nullable: false),
                    FechaCalculo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VolumenCosechaEstimado = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PronosticosDemanda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PronosticosDemanda_CampaniasAgricolas_CampaniaId",
                        column: x => x.CampaniaId,
                        principalTable: "CampaniasAgricolas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaPromesaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntregaReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EstadoLogistico = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosVenta_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanesAccionMejora",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndicadorId = table.Column<int>(type: "int", nullable: false),
                    AnalisisCausaRaiz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsableId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesAccionMejora", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesAccionMejora_IndicadoresKPI_IndicadorId",
                        column: x => x.IndicadorId,
                        principalTable: "IndicadoresKPI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesAsiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsientoId = table.Column<int>(type: "int", nullable: false),
                    CuentaId = table.Column<int>(type: "int", nullable: false),
                    Debe = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Haber = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesAsiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesAsiento_AsientosContables_AsientoId",
                        column: x => x.AsientoId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesAsiento_PlanCuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "PlanCuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CotizacionesProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequerimientoId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    PrecioOfertado = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiasEntrega = table.Column<int>(type: "int", nullable: false),
                    EsGanador = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionesProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedor_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CotizacionesProveedor_RequerimientosCompra_RequerimientoId",
                        column: x => x.RequerimientoId,
                        principalTable: "RequerimientosCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AjustesInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    ZonaUbicacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoAjuste = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoAsumido = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventario_ZonasUbicacion_ZonaUbicacionId",
                        column: x => x.ZonaUbicacionId,
                        principalTable: "ZonasUbicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspeccionesCalidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioInspectorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsConforme = table.Column<bool>(type: "bit", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspeccionesCalidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspeccionesCalidad_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventariosExistencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    ZonaUbicacionId = table.Column<int>(type: "int", nullable: false),
                    CantidadFisica = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadReservada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventariosExistencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventariosExistencia_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventariosExistencia_ZonasUbicacion_ZonaUbicacionId",
                        column: x => x.ZonaUbicacionId,
                        principalTable: "ZonasUbicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosKardex",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ZonaOrigenId = table.Column<int>(type: "int", nullable: true),
                    ZonaDestinoId = table.Column<int>(type: "int", nullable: true),
                    ReferenciaDocumento = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosKardex", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosKardex_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosKardex_ZonasUbicacion_ZonaDestinoId",
                        column: x => x.ZonaDestinoId,
                        principalTable: "ZonasUbicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosKardex_ZonasUbicacion_ZonaOrigenId",
                        column: x => x.ZonaOrigenId,
                        principalTable: "ZonasUbicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NecesidadesMrp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PronosticoId = table.Column<int>(type: "int", nullable: false),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    CantidadNetaSugerida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NecesidadesMrp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NecesidadesMrp_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NecesidadesMrp_PronosticosDemanda_PronosticoId",
                        column: x => x.PronosticoId,
                        principalTable: "PronosticosDemanda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoVentaId = table.Column<int>(type: "int", nullable: false),
                    TipoCPE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesPago_PedidosVenta_PedidoVentaId",
                        column: x => x.PedidoVentaId,
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPedidoVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedidoVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesPedidoVenta_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesPedidoVenta_PedidosVenta_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DevolucionesCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoVentaId = table.Column<int>(type: "int", nullable: false),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    CantidadRechazada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesCliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucionesCliente_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevolucionesCliente_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevolucionesCliente_PedidosVenta_PedidoVentaId",
                        column: x => x.PedidoVentaId,
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuiasRemisionRemitente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoVentaId = table.Column<int>(type: "int", nullable: false),
                    PlacaVehiculo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Chofer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotivoTraslado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuiasRemisionRemitente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuiasRemisionRemitente_PedidosVenta_PedidoVentaId",
                        column: x => x.PedidoVentaId,
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesPicking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoVentaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioOperarioId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesPicking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesPicking_PedidosVenta_PedidoVentaId",
                        column: x => x.PedidoVentaId,
                        principalTable: "PedidosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesOrdenCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenCompraId = table.Column<int>(type: "int", nullable: false),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    CantidadPedida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesOrdenCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesOrdenCompra_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesOrdenCompra_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesMercaderia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenCompraId = table.Column<int>(type: "int", nullable: false),
                    GuiaProveedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioReceptorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesMercaderia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercaderia_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesRequerimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequerimientoId = table.Column<int>(type: "int", nullable: false),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    NecesidadMrpId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesRequerimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesRequerimiento_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesRequerimiento_NecesidadesMrp_NecesidadMrpId",
                        column: x => x.NecesidadMrpId,
                        principalTable: "NecesidadesMrp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesRequerimiento_RequerimientosCompra_RequerimientoId",
                        column: x => x.RequerimientoId,
                        principalTable: "RequerimientosCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReservasInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoVentaDetalleId = table.Column<int>(type: "int", nullable: false),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    ZonaUbicacionId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservasInventario_DetallesPedidoVenta_PedidoVentaDetalleId",
                        column: x => x.PedidoVentaDetalleId,
                        principalTable: "DetallesPedidoVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservasInventario_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservasInventario_ZonasUbicacion_ZonaUbicacionId",
                        column: x => x.ZonaUbicacionId,
                        principalTable: "ZonasUbicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotaCredito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevolucionId = table.Column<int>(type: "int", nullable: false),
                    ComprobantePagoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotaCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotaCredito_ComprobantesPago_ComprobantePagoId",
                        column: x => x.ComprobantePagoId,
                        principalTable: "ComprobantesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotaCredito_DevolucionesCliente_DevolucionId",
                        column: x => x.DevolucionId,
                        principalTable: "DevolucionesCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesOrdenPicking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenPickingId = table.Column<int>(type: "int", nullable: false),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    CantidadRecolectada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesOrdenPicking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesOrdenPicking_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesOrdenPicking_OrdenesPicking_OrdenPickingId",
                        column: x => x.OrdenPickingId,
                        principalTable: "OrdenesPicking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesRecepcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecepcionId = table.Column<int>(type: "int", nullable: false),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    CantidadPedida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadGuia = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesRecepcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesRecepcion_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesRecepcion_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesRecepcion_RecepcionesMercaderia_RecepcionId",
                        column: x => x.RecepcionId,
                        principalTable: "RecepcionesMercaderia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_LoteId",
                table: "AjustesInventario",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_ZonaUbicacionId",
                table: "AjustesInventario",
                column: "ZonaUbicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_PedidoVentaId",
                table: "ComprobantesPago",
                column: "PedidoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesProveedor_ProveedorId",
                table: "CotizacionesProveedor",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionesProveedor_RequerimientoId",
                table: "CotizacionesProveedor",
                column: "RequerimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesAsiento_AsientoId",
                table: "DetallesAsiento",
                column: "AsientoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesAsiento_CuentaId",
                table: "DetallesAsiento",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenCompra_ArticuloId",
                table: "DetallesOrdenCompra",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenCompra_OrdenCompraId",
                table: "DetallesOrdenCompra",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenPicking_LoteId",
                table: "DetallesOrdenPicking",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenPicking_OrdenPickingId",
                table: "DetallesOrdenPicking",
                column: "OrdenPickingId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedidoVenta_ArticuloId",
                table: "DetallesPedidoVenta",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedidoVenta_PedidoId",
                table: "DetallesPedidoVenta",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRecepcion_ArticuloId",
                table: "DetallesRecepcion",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRecepcion_LoteId",
                table: "DetallesRecepcion",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRecepcion_RecepcionId",
                table: "DetallesRecepcion",
                column: "RecepcionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRequerimiento_ArticuloId",
                table: "DetallesRequerimiento",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRequerimiento_NecesidadMrpId",
                table: "DetallesRequerimiento",
                column: "NecesidadMrpId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRequerimiento_RequerimientoId",
                table: "DetallesRequerimiento",
                column: "RequerimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCliente_ArticuloId",
                table: "DevolucionesCliente",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCliente_LoteId",
                table: "DevolucionesCliente",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesCliente_PedidoVentaId",
                table: "DevolucionesCliente",
                column: "PedidoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_GuiasRemisionRemitente_PedidoVentaId",
                table: "GuiasRemisionRemitente",
                column: "PedidoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_InspeccionesCalidad_LoteId",
                table: "InspeccionesCalidad",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosExistencia_LoteId",
                table: "InventariosExistencia",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosExistencia_ZonaUbicacionId",
                table: "InventariosExistencia",
                column: "ZonaUbicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ArticuloId",
                table: "Lotes",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosKardex_LoteId",
                table: "MovimientosKardex",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosKardex_ZonaDestinoId",
                table: "MovimientosKardex",
                column: "ZonaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosKardex_ZonaOrigenId",
                table: "MovimientosKardex",
                column: "ZonaOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_NecesidadesMrp_ArticuloId",
                table: "NecesidadesMrp",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_NecesidadesMrp_PronosticoId",
                table: "NecesidadesMrp",
                column: "PronosticoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotaCredito_ComprobantePagoId",
                table: "NotaCredito",
                column: "ComprobantePagoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotaCredito_DevolucionId",
                table: "NotaCredito",
                column: "DevolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_ProveedorId",
                table: "OrdenesCompra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPicking_PedidoVentaId",
                table: "OrdenesPicking",
                column: "PedidoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosInventario_ArticuloId",
                table: "ParametrosInventario",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosVenta_ClienteId",
                table: "PedidosVenta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanesAccionMejora_IndicadorId",
                table: "PlanesAccionMejora",
                column: "IndicadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PronosticosDemanda_CampaniaId",
                table: "PronosticosDemanda",
                column: "CampaniaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercaderia_OrdenCompraId",
                table: "RecepcionesMercaderia",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasAgricolasBOM_ArticuloFinalId",
                table: "RecetasAgricolasBOM",
                column: "ArticuloFinalId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasAgricolasBOM_ArticuloInsumoId",
                table: "RecetasAgricolasBOM",
                column: "ArticuloInsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasInventario_LoteId",
                table: "ReservasInventario",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasInventario_PedidoVentaDetalleId",
                table: "ReservasInventario",
                column: "PedidoVentaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasInventario_ZonaUbicacionId",
                table: "ReservasInventario",
                column: "ZonaUbicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ZonasUbicacion_AlmacenId",
                table: "ZonasUbicacion",
                column: "AlmacenId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AjustesInventario");

            migrationBuilder.DropTable(
                name: "CotizacionesProveedor");

            migrationBuilder.DropTable(
                name: "DetallesAsiento");

            migrationBuilder.DropTable(
                name: "DetallesOrdenCompra");

            migrationBuilder.DropTable(
                name: "DetallesOrdenPicking");

            migrationBuilder.DropTable(
                name: "DetallesRecepcion");

            migrationBuilder.DropTable(
                name: "DetallesRequerimiento");

            migrationBuilder.DropTable(
                name: "GastosLogisticos");

            migrationBuilder.DropTable(
                name: "GuiasRemisionRemitente");

            migrationBuilder.DropTable(
                name: "InspeccionesCalidad");

            migrationBuilder.DropTable(
                name: "InventariosExistencia");

            migrationBuilder.DropTable(
                name: "MovimientosKardex");

            migrationBuilder.DropTable(
                name: "NotaCredito");

            migrationBuilder.DropTable(
                name: "ParametrosInventario");

            migrationBuilder.DropTable(
                name: "PlanesAccionMejora");

            migrationBuilder.DropTable(
                name: "RecetasAgricolasBOM");

            migrationBuilder.DropTable(
                name: "ReservasInventario");

            migrationBuilder.DropTable(
                name: "AsientosContables");

            migrationBuilder.DropTable(
                name: "PlanCuentas");

            migrationBuilder.DropTable(
                name: "OrdenesPicking");

            migrationBuilder.DropTable(
                name: "RecepcionesMercaderia");

            migrationBuilder.DropTable(
                name: "NecesidadesMrp");

            migrationBuilder.DropTable(
                name: "RequerimientosCompra");

            migrationBuilder.DropTable(
                name: "ComprobantesPago");

            migrationBuilder.DropTable(
                name: "DevolucionesCliente");

            migrationBuilder.DropTable(
                name: "IndicadoresKPI");

            migrationBuilder.DropTable(
                name: "DetallesPedidoVenta");

            migrationBuilder.DropTable(
                name: "ZonasUbicacion");

            migrationBuilder.DropTable(
                name: "OrdenesCompra");

            migrationBuilder.DropTable(
                name: "PronosticosDemanda");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "PedidosVenta");

            migrationBuilder.DropTable(
                name: "Almacenes");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "CampaniasAgricolas");

            migrationBuilder.DropTable(
                name: "Articulos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
