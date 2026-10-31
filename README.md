# AgroERP – Guía del equipo (Mi Paraíso Gramadal)

ERP logístico para una chacra de fresas. ASP.NET Core MVC + DAO + SQL Server (EF Core Code First).
**Este archivo es la única fuente de verdad del equipo.** Si usas una IA, pásasela junto con la carpeta `Models/` y tu `DA<Modulo>.cs` (ver sección 10).

---

## 1. Cómo empezar (todos, una sola vez)
1. Clona el repo y trae `main`: `git clone <repo>` → `git checkout main` → `git pull`.
2. Crea tu rama desde `main` (Git → Nueva rama): `feature/<tu-modulo>` (ver sección 2).
3. Cadena de conexión en **User Secrets** (clic derecho al proyecto → *Administrar secretos de usuario*). **No** la subas a Git.
4. Consola del Administrador de paquetes: `Update-Database`.
5. En SSMS, **sobre la base del proyecto** (no `master`), ejecuta en este orden:
   1. `Database/seed_usuarios.sql`
   2. `Database/seed_datos_base.sql`
6. F5 y entra con tu usuario (sección 3). El plan de cuentas se carga solo al arrancar.

Los scripts se pueden ejecutar varias veces sin duplicar datos.

## 2. Qué es tuyo
| Persona | Controller / DAO / Vistas | Rama |
|---|---|---|
| **Quito** | Planificacion, Articulos, Campanias, Recetas, DAPlanificacion, DAMaestros, DAContabilidad, Dashboard BI | `feature/core-ia-bi` |
| **Changa** | ComprasController, DACompras, Views/Compras | `feature/compras` |
| **Lavado** | CalidadController, DACalidad, Views/Calidad | `feature/recepcion-calidad` |
| **Méndez** | AlmacenController, DAAlmacen, Views/Almacen | `feature/almacenes` |
| **Díaz** | DistribucionController, DADistribucion, Views/Distribucion | `feature/distribucion` |

Cada uno trabaja **solo** en: `Controllers/<Modulo>Controller.cs`, `Data/AccesoDatos/DA<Modulo>.cs`, `Views/<Modulo>/`, `ViewModels/<Modulo>/` (namespace `AgroERP.ViewModels.<Modulo>`).

## 3. Usuarios de prueba (contraseña de todos: `12345`)
`denzell.admin` · `changa` · `lavado` · `mendez` · `diaz`
(Ids fijos `USR-001` … `USR-005`. Las columnas `UsuarioXxxId` guardan ese Id.)
El login compara la contraseña en texto plano: solo para desarrollo.

## 4. Tablas reales y su dueño
Las tablas están en el esquema **`dbo`** y su nombre es el del `DbSet` (en plural). Ejemplo: `dbo.Articulos`, `dbo.InventariosExistencia`. **No** existen esquemas como `Calidad_WMS.`: no copies consultas con esos prefijos.

| Dueño | Tablas que ESCRIBE |
|---|---|
| Quito | `Articulos`, `ParametrosInventario`, `CampaniasAgricolas`, `RecetasAgricolasBOM`, `PronosticosDemanda`, `NecesidadesMrp`, `IndicadoresKPI`, `PlanesAccionMejora`, y (vía `DAContabilidad`) `PlanCuentas`, `AsientosContables`, `DetallesAsiento` |
| Changa | `Proveedores`, `RequerimientosCompra`, `DetallesRequerimiento`, `CotizacionesProveedor`, `OrdenesCompra`, `DetallesOrdenCompra`, `GastosLogisticos` |
| Lavado | `RecepcionesMercaderia`, `DetallesRecepcion`, `Lotes`, `InspeccionesCalidad` |
| Méndez | `Almacenes`, `ZonasUbicacion`, `InventariosExistencia`, `ReservasInventario`, `MovimientosKardex`, `AjustesInventario` |
| Díaz | `Clientes`, `PedidosVenta`, `DetallesPedidoVenta`, `OrdenesPicking`, `DetallesOrdenPicking`, `GuiasRemisionRemitente`, `ComprobantesPago`, `DevolucionesCliente`, `NotaCredito` |

## 5. Reglas
1. **Lectura libre, escritura solo del dueño.** Puedes consultar cualquier tabla; solo modificas las tuyas. Para tocar una ajena, llama al método de su DAO (sección 7).
2. **No tocar:** `ApplicationDbContext`, `Models/`, `_LayoutLogistica.cshtml`, `Program.cs`, `Migrations/`. Pídeselo a Quito.
3. **Solo Quito ejecuta `Add-Migration`.** Los demás solo `Update-Database`. Después de cada merge a `main`, haz *Pull* y `Update-Database`.
4. **Un solo `SaveChangesAsync` por operación.** Stock + estado + asiento se guardan juntos al final del flujo. Los métodos de contrato **no** llaman `SaveChanges`.
5. **ViewModels:** las vistas nunca reciben entidades directas.
6. **Layout:** cada vista nueva incluye `Layout = "~/Views/Shared/_LayoutLogistica.cshtml";`.
7. **Contabilidad:** siempre `DAContabilidad.RegistrarAsiento(...)` con `Cuentas.Xxx`. Nunca códigos a mano ni insertar en tablas contables.
8. **Estados y tipos = texto exacto** (son `string`, no enums), sin variantes:
   - Articulo.Tipo: `Fresa` `Insumo` `Empaque`
   - Campaña: `Planificada` `Activa` `Cerrada`
   - Necesidad MRP: `Pendiente` `ConRequerimiento` `Atendida` `Anulada`
   - Requerimiento: `Borrador` `PendienteAprobacion` `Aprobado` `Rechazado`
   - OrdenCompra: `Emitida` `RecepcionadaParcial` `Cerrada`
   - Recepcion: `EnProceso` `Cerrada` `Anulada`
   - Lote: `Cuarentena` `Liberado` `Rechazado`
   - Reserva: `Activa` `Consumida` `Liberada`
   - Pedido: `Pendiente` `Picking` `Empacado` `EnRuta` `Entregado`
   - PlanAccion: `EnProgreso` `Completado`
   - MovimientoKardex.TipoMovimiento: `Ingreso` `Salida` `Traslado` `Merma` `Ajuste`
   - AjusteInventario.TipoAjuste: `Merma` `ConteoPositivo` `ConteoNegativo` (todo conteo debe contener la palabra "Conteo")
   - Entrega de pedidos: al entregar, Díaz debe llenar `PedidoVenta.FechaEntregaReal` (el OTIF solo cuenta pedidos con esa fecha)
   - Recepción: Lavado debe llenar `CantidadPedida` y `CantidadRecibida` en cada `DetalleRecepcion` (de ahí sale el cumplimiento de proveedores)

## 6. Catálogos (no insertes a mano)
Artículos, campañas y recetas se cargan desde las pantallas `/Articulos`, `/Campanias` y `/Recetas`. Proveedores, clientes y almacenes los carga el dueño de cada módulo en su propia pantalla; mientras tanto, usa inserts en SQL para probar.

## 7. Contratos entre módulos (declarados con `NotImplementedException` hasta que el dueño los implemente)
| Método | Lo implementa | Lo llama |
|---|---|---|
| `DACompras.CrearRequerimientoDesdeMrp(necesidadIds, usuarioId)` | Changa | Quito |
| `DACompras.MarcarOrdenRecepcionada(ocId, parcial)` | Changa | Lavado |
| `DAAlmacen.RegistrarIngreso(loteId, zonaId, cantidad, ref)` | Méndez | Lavado |
| `DAAlmacen.ReservarStock(detalleId, articuloId, cantidad)` | Méndez | Díaz |
| `DAAlmacen.LiberarReserva(pedidoId)` | Méndez | Díaz |
| `DAAlmacen.ConfirmarSalida(ordenPickingId)` | Méndez | Díaz |
| `DAAlmacen.ReingresarDevolucion(devolucionId)` | Méndez | Díaz |

Cada dueño implementa los suyos **en los primeros días**: los demás dependen de ellos. Mientras no estén, prueba con datos insertados en SQL. Reglas gemelas: **solo Méndez** modifica el stock y **solo Lavado** cambia `Lote.EstadoCalidad`.

**`DACompras.CrearRequerimientoDesdeMrp`** (Changa): crea un `RequerimientoCompra` en `Borrador`, un detalle por necesidad con su `NecesidadMrpId`, marca esas necesidades como `ConRequerimiento` y devuelve el Id del requerimiento.

## 8. Contabilidad
Códigos en la clase `Cuentas` (no los escribas a mano):
`Mercaderias 2011` · `ProductoTerminado 2111` · `MaterialesAux 2511` · `Envases 2611` · `IgvPorPagar 4011` · `FacturasPorPagar 4212` · `FacturasPorCobrar 1212` · `VentasProductos 7021` · `CostoVentas 6921` · `MermasGasto 6598` · `FletesGasto 6311`

| Evento | Quién | Debe | Haber |
|---|---|---|---|
| Recepción conforme | Lavado | MaterialesAux / Envases | FacturasPorPagar |
| Merma / ajuste negativo | Méndez | MermasGasto | MaterialesAux / ProductoTerminado |
| Factura | Díaz | FacturasPorCobrar | VentasProductos + IgvPorPagar |
| Nota de crédito | Díaz | VentasProductos + IgvPorPagar | FacturasPorCobrar |
| Flete | Changa | FletesGasto | FacturasPorPagar |

El asiento debe cuadrar (Debe = Haber) o `RegistrarAsiento` lanza error y no se guarda nada.

## 9. Git (Visual Studio: menú Git)
- **`main` está protegida:** nadie sube directo; todo entra por **Pull Request** con 1 aprobación. Quito revisa y une.
- Trabaja siempre en tu rama. Cada mañana: extrae `main` → *Pull* → vuelve a tu rama y combina `main` en ella.
- Commits pequeños y frecuentes. **Confirmar todo e insertar** para subirlos.
- Cada 2–3 días abre un Pull Request en GitHub (*Compare & pull request*). Antes de abrirlo: compila, pruébalo y revisa en *Files changed* que solo toques tu zona.
- ¿Conflicto? No lo resuelvas a ciegas: avisa a Quito.
- No subas `bin/`, `obj/`, `.vs/`, `appsettings` con claves ni `python-forecast/venv/`.

## 10. Cómo usar tu IA
Pásale este README + la carpeta `Models/` + tu `DA<Modulo>.cs`, y empieza con:
> *Soy responsable del módulo X de AgroERP. Solo puedo modificar mis archivos y mis tablas. Respeta las reglas del README: no cambies Models, DbContext ni Program.cs, no propongas migraciones, usa DAO + ViewModels, estados como texto exacto y un solo SaveChanges por operación.*

## 11. Misión de cada módulo
- **Quito:** MRP con IA (microservicio Python XGBoost), catálogos (artículos, campañas, recetas), `DAContabilidad`, KPIs y dashboard con Chart.js, revisión de Pull Requests.
- **Changa:** requerimiento → cotización ganadora → orden de compra en PDF. CRUD de Proveedor y GastoLogistico. Pantallas de Plan de Cuentas y Libro Diario (solo lectura).
- **Lavado:** 3-way matching (pedida vs. guía vs. contada), crear el lote al recibir, inspección que libera o rechaza el lote y asiento contable al liberar.
- **Méndez:** FEFO, kardex, ubicaciones, inventario cíclico y ajustes de merma. Es el único que modifica el stock.
- **Díaz:** pedido con validación de crédito, control ATP, picking, guía de remisión, factura, devolución y nota de crédito.

## 12. Cronograma y orden de dependencia
Compras → Recepción → Almacén → Distribución → BI. **Quien está al inicio de la cadena no debe atrasarse.**
- **Día 1:** lista de tus pantallas y 3 reglas de negocio clave de tu módulo (sin código) a Quito.
- **Semanas 1–2:** desarrollo en paralelo; Pull Request cada 2–3 días.
- **Semana 3:** prueba del flujo completo (MRP → Compra → Recepción → Almacén → Pedido → Despacho → Factura) y dashboards con datos reales.

## 13. Problemas frecuentes
| Síntoma | Causa y solución |
|---|---|
| `fatal: not a git repository` | Estás en una carpeta sin `.git`. Entra a la carpeta del repo. |
| No puedo entrar al login | No ejecutaste `Database/seed_usuarios.sql` en tu base. |
| `Invalid object name 'dbo.X'` | Falta `Update-Database`, o la consulta usa un nombre que no existe (ver sección 4). |
| Error de conexión | Falta tu cadena en User Secrets. |
| Error de tabla al compilar tras un *Pull* | Quito cambió el esquema: ejecuta `Update-Database`. |
| MRP: "No se pudo conectar con el microservicio" | Solo Quito: levantar `python-forecast` (`uvicorn app:app --port 8000`). |
