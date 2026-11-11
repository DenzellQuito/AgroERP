# AgroERP – Guía del equipo (Mi Paraíso Gramadal)

ERP Logístico, de Tesorería y Control Patrimonial para una chacra de fresas. ASP.NET Core MVC + DAO + SQL Server (EF Core Code First).
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

---

## 2. Qué es tuyo (Fase 1: Logística + Fase 2: Tesorería y Activos Fijos)

### Fase 1 – Módulo Logístico (Ya integrado en `main`)
| Persona | Controller / DAO / Vistas | Rama Fase 1 |
|---|---|---|
| **Quito** | Planificacion, Articulos, Campanias, Recetas, DAPlanificacion, DAMaestros, DAContabilidad, Dashboard BI | `feature/core-ia-bi` |
| **Changa** | ComprasController, DACompras, Views/Compras | `feature/compras` |
| **Lavado** | CalidadController, DACalidad, Views/Calidad | `feature/recepcion-calidad` |
| **Méndez** | AlmacenController, DAAlmacen, Views/Almacen | `feature/almacenes` |
| **Díaz** | DistribucionController, DADistribucion, Views/Distribucion | `feature/distribucion` |

### Fase 2 – Módulo Unificado: Tesorería + Gestión de Activos Fijos (Sprint 3 días)
| Persona | Nuevo Controller / DAO / Vistas | Rama Fase 2 |
|---|---|---|
| **Quito** | `TesoreriaController`, `DATesoreria`, `Views/Tesoreria`, `ViewModels/Tesoreria` (+ ampliación `DAContabilidad` y `DABI`) | `feature/tesoreria-bi` |
| **Díaz** | `CobranzasController`, `DACobranzas`, `Views/Cobranzas`, `ViewModels/Cobranzas` | `feature/cobranzas` |
| **Changa** | `PagosController`, `DAPagos`, `Views/Pagos`, `ViewModels/Pagos` | `feature/pagos` |
| **Lavado** | `ActivosController`, `DAActivos`, `Views/Activos`, `ViewModels/Activos` | `feature/activos` |
| **Méndez** | `PatrimonioController`, `DAPatrimonio`, `Views/Patrimonio`, `ViewModels/Patrimonio` | `feature/patrimonio` |

Cada uno trabaja **solo** en: `Controllers/<Modulo>Controller.cs`, `Data/AccesoDatos/DA<Modulo>.cs`, `Views/<Modulo>/`, `ViewModels/<Modulo>/` (namespace `AgroERP.ViewModels.<Modulo>`).

---

## 3. Usuarios de prueba (contraseña de todos: `12345`)
`denzell.admin` · `changa` · `lavado` · `mendez` · `diaz`
(Ids fijos `USR-001` … `USR-005`. Las columnas `UsuarioXxxId` guardan ese Id.)
El login compara la contraseña en texto plano: solo para desarrollo.

---

## 4. Tablas reales y su dueño
Las tablas están en el esquema **`dbo`** y su nombre es el del `DbSet` (en plural). Ejemplo: `dbo.Articulos`, `dbo.CuentasBancarias`. **No** existen esquemas como `Calidad_WMS.` o `Tesoreria.`: no copies consultas con esos prefijos.

| Dueño | Tablas que ESCRIBE (Logística + Tesorería y Activos) |
|---|---|
| **Quito** | **Logística/BI:** `Articulos`, `ParametrosInventario`, `CampaniasAgricolas`, `RecetasAgricolasBOM`, `PronosticosDemanda`, `NecesidadesMrp`, `IndicadoresKPI`, `PlanesAccionMejora`.<br>**Contabilidad:** `PlanCuentas`, `AsientosContables`, `DetallesAsiento` (vía `DAContabilidad`).<br>**Tesorería:** `CuentasBancarias`, `MovimientosBancarios`, `ConciliacionesBancarias`. |
| **Díaz** | **Distribución:** `Clientes`, `PedidosVenta`, `DetallesPedidoVenta`, `OrdenesPicking`, `DetallesOrdenPicking`, `GuiasRemisionRemitente`, `ComprobantesPago`, `DevolucionesCliente`, `NotaCredito`.<br>**Ingresos/CxC:** `CuentasPorCobrar`, `CobranzasCliente`. |
| **Changa** | **Compras:** `Proveedores`, `RequerimientosCompra`, `DetallesRequerimiento`, `CotizacionesProveedor`, `OrdenesCompra`, `DetallesOrdenCompra`, `GastosLogisticos`.<br>**Egresos/CxP:** `CuentasPorPagar`, `PagosProveedor`. |
| **Lavado** | **Calidad:** `RecepcionesMercaderia`, `DetallesRecepcion`, `Lotes`, `InspeccionesCalidad`.<br>**Activos Fijos I:** `ActivosFijos`, `DepreciacionesActivo`. |
| **Méndez** | **Almacenes:** `Almacenes`, `ZonasUbicacion`, `InventariosExistencia`, `ReservasInventario`, `MovimientosKardex`, `AjustesInventario`.<br>**Activos Fijos II (Patrimonio):** `TransferenciasActivo`, `MantenimientosActivo`, `BajasActivo`. |

---

## 5. Reglas
1. **Lectura libre, escritura solo del dueño.** Puedes consultar cualquier tabla; solo modificas las tuyas. Para tocar una ajena, llama al método de su DAO (sección 7).
2. **No tocar:** `ApplicationDbContext`, `Models/`, `_LayoutLogistica.cshtml`, `Program.cs`, `Migrations/`. Pídeselo a Quito.
3. **Solo Quito ejecuta `Add-Migration`.** Los demás solo `Update-Database`. Después de cada merge a `main`, haz *Pull* y `Update-Database`.
4. **Un solo `SaveChangesAsync` por operación.** Saldo bancario + estado de CxC/CxP/Activo + asiento contable se guardan juntos al final del flujo. Los métodos de contrato **no** llaman `SaveChangesAsync`, excepto `DACompras.CrearRequerimientoDesdeMrp`.
5. **ViewModels:** las vistas nunca reciben entidades directas.
6. **Layout:** cada vista nueva incluye `Layout = "~/Views/Shared/_LayoutLogistica.cshtml";`.
7. **Contabilidad:** siempre `DAContabilidad.RegistrarAsiento(...)` con `Cuentas.Xxx`. Nunca códigos a mano ni insertar en tablas contables.
8. **Estados y tipos = texto exacto** (son `string`, no enums), sin variantes:
   - **Logística (Fase 1):**
     - `Articulo.Tipo`: `Fresa` `Insumo` `Empaque`
     - `Campania`: `Planificada` `Activa` `Cerrada`
     - `NecesidadMrp`: `Pendiente` `ConRequerimiento` `Atendida` `Anulada`
     - `Requerimiento`: `Borrador` `PendienteAprobacion` `Aprobado` `Rechazado`
     - `OrdenCompra`: `Emitida` `RecepcionadaParcial` `Cerrada`
     - `Recepcion`: `EnProceso` `Cerrada` `Anulada`
     - `Lote`: `Cuarentena` `Liberado` `Rechazado`
     - `Reserva`: `Activa` `Consumida` `Liberada`
     - `Pedido`: `Pendiente` `Picking` `Empacado` `EnRuta` `Entregado`
     - `PlanAccion`: `EnProgreso` `Completado`
     - `MovimientoKardex.TipoMovimiento`: `Ingreso` `Salida` `Traslado` `Merma` `Ajuste`
     - `AjusteInventario.TipoAjuste`: `Merma` `ConteoPositivo` `ConteoNegativo`
   - **Tesorería y Activos Fijos (Fase 2):**
     - `CuentaBancaria.Moneda`: `PEN`
     - `MovimientoBancario.TipoMovimiento`: `Ingreso` `Egreso`
     - `ConciliacionBancaria.Estado`: `EnProceso` `Cerrada`
     - `CuentaPorCobrar.Estado`: `Pendiente` `CobradoParcial` `Cobrado`
     - `CobranzaCliente.MedioPago` / `PagoProveedor.MedioPago`: `Transferencia` `Deposito` `POS`
     - `CuentaPorPagar.TipoObligacion`: `Insumos` `Flete` `Mantenimiento` `ActivoFijo`
     - `CuentaPorPagar.Prioridad`: `Alta` `Media` `Baja`
     - `CuentaPorPagar.Estado`: `Pendiente` `Programado` `Autorizado` `PagadoParcial` `Pagado`
     - `ActivoFijo.Categoria`: `MaquinariaAgricola` `EquiposComputo` `InfraestructuraRiego` `Vehiculos`
     - `ActivoFijo.Estado`: `EnProceso` `Activo` `DadoDeBaja`
     - `MantenimientoActivo.Tipo`: `Preventivo` `Correctivo` `MejoraCapitalizable`
     - `BajaActivo.MotivoBaja`: `Desuso` `Retiro` `Siniestro`
9. **Usuario conectado:** Nunca escribas un Id de usuario fijo en el código. Usa siempre `User.FindFirstValue(ClaimTypes.NameIdentifier)`.

---

## 6. Catálogos (no insertes a mano)
Artículos, campañas y recetas se cargan desde `/Articulos`, `/Campanias` y `/Recetas`. Cuentas bancarias las carga Quito en `/Tesoreria`; activos fijos los carga Lavado en `/Activos`. Proveedores, clientes y almacenes los carga el dueño de cada módulo en su pantalla.

---

## 7. Contratos entre módulos

### Contratos Fase 1 (Logística)
| Método | Lo implementa | Lo llama |
|---|---|---|
| `DACompras.CrearRequerimientoDesdeMrp(necesidadIds, usuarioId)` | Changa | Quito |
| `DACompras.MarcarOrdenRecepcionada(ocId, parcial)` | Changa | Lavado |
| `DAAlmacen.RegistrarIngreso(loteId, zonaId, cantidad, ref)` | Méndez | Lavado |
| `DAAlmacen.ReservarStock(detalleId, articuloId, cantidad)` | Méndez | Díaz |
| `DAAlmacen.LiberarReserva(pedidoId)` | Méndez | Díaz |
| `DAAlmacen.ConfirmarSalida(ordenPickingId)` | Méndez | Díaz |
| `DAAlmacen.ReingresarDevolucion(devolucionId)` | Méndez | Díaz |

### Contratos Fase 2 (Tesorería + Activos Fijos)
| Método | Lo implementa | Lo llama | Regla del contrato |
|---|---|---|---|
| `DATesoreria.RegistrarMovimientoBancario(cuentaBancariaId, tipoMovimiento, monto, medioPago, nroOperacion, referencia)` | **Quito** | **Díaz** (cobros) y **Changa** (pagos) | Valida saldo disponible si es `Egreso`, suma/resta `SaldoContable` y `SaldoDisponible` en `CuentaBancaria` e inserta en `MovimientosBancarios`. |
| `DACobranzas.CrearCuentaPorCobrar(comprobantePagoId, diasCredito)` | **Díaz** | **Díaz** (`DADistribucion`) | Crea la `CuentaPorCobrar` en `Pendiente` con `SaldoPendiente = MontoTotal` al emitir factura. |
| `DAPagos.CrearCuentaPorPagar(proveedorId, ordenCompraId, tipoObligacion, nroFactura, montoTotal, diasVencimiento)` | **Changa** | **Lavado** (alta activo), **Méndez** (mantenimiento/mejora) y **Changa** (OC/fletes) | Crea la obligación en `CuentasPorPagar` en estado `Pendiente` con `SaldoPendiente = montoTotal`. |
| `DAActivos.ActualizarUbicacionYCustodio(activoId, zonaUbicacionId, usuarioResponsableId)` | **Lavado** | **Méndez** | Actualiza `ZonaUbicacionId` y `UsuarioResponsableId` en `ActivosFijos` tras un traslado. |
| `DAActivos.CapitalizarMejora(activoId, montoMejora)` | **Lavado** | **Méndez** | Suma `montoMejora` a `CostoAdquisicion` y `ValorNetoLibros` del `ActivoFijo`. |
| `DAActivos.MarcarActivoDadoDeBaja(activoId)` | **Lavado** | **Méndez** | Cambia `ActivoFijo.Estado` a `DadoDeBaja`. |

Reglas de oro de propiedad:
- **Solo Quito** modifica saldos en `CuentasBancarias`.
- **Solo Lavado** modifica la tabla `ActivosFijos`.
- **Evitar duplicidad en Cuentas por Pagar:** Para compras de `Insumos` (cuyo asiento ya fue creado por Lavado al liberar el lote en recepción), Changa registra la `CuentaPorPagar` cruzándola con la `OrdenCompra` recepcionada **sin volver a generar asiento de compra**. Para `ActivoFijo` (Lavado), `Mantenimiento` / `MejoraCapitalizable` (Méndez) y `Flete` (Changa), quien registra el evento genera su asiento contable y llama en la misma transacción a `DAPagos.CrearCuentaPorPagar(...)`.

---

## 8. Contabilidad
Códigos en la clase `Cuentas` (no los escribas a mano):
- **Fase 1 (Logística):** `Mercaderias 2011` · `ProductoTerminado 2111` · `MaterialesAux 2511` · `Envases 2611` · `IgvPorPagar 4011` · `FacturasPorPagar 4212` · `FacturasPorCobrar 1212` · `VentasProductos 7011` · `CostoVentas 6921` · `MermasGasto 6598` · `FletesGasto 6311`
- **Fase 2 (Tesorería y Activos):** `Bancos 1041` · `MaquinariaEquipo 3331` · `DepreciacionAcumulada 3913` · `PasivoActivoFijo 4654` · `MantenimientoGasto 6341` · `GastosBancarios 6391` · `CostoNetoBajaActivo 6551` · `DepreciacionGasto 6814`

| Evento | Quién | ReferenciaModulo | Debe | Haber |
|---|---|---|---|---|
| Recepción conforme | Lavado | `Recepcion` | `MaterialesAux` / `Envases` | `FacturasPorPagar` |
| Merma / ajuste negativo | Méndez | `Merma` | `MermasGasto` | `MaterialesAux` / `Envases` / `ProductoTerminado` |
| Factura de venta | Díaz | `Factura` | `FacturasPorCobrar` | `VentasProductos` + `IgvPorPagar` |
| Nota de crédito | Díaz | `NotaCredito` | `VentasProductos` + `IgvPorPagar` | `FacturasPorCobrar` |
| Flete | Changa | `Compras` | `FletesGasto` | `FacturasPorPagar` |
| **Cobranza a cliente** | **Díaz** | `Cobranza` | `Bancos (1041)` | `FacturasPorCobrar (1212)` |
| **Pago a proveedor (Insumos/Flete/Mant.)** | **Changa** | `PagoProveedor` | `FacturasPorPagar (4212)` | `Bancos (1041)` |
| **Pago a proveedor (Activo Fijo)** | **Changa** | `PagoProveedor` | `PasivoActivoFijo (4654)` | `Bancos (1041)` |
| **Comisión en Conciliación Bancaria** | **Quito** | `Conciliacion` | `GastosBancarios (6391)` | `Bancos (1041)` |
| **Capitalización / Alta de Activo Fijo** | **Lavado** | `AltaActivo` | `MaquinariaEquipo (3331)` + `IgvPorPagar (4011)` | `PasivoActivoFijo (4654)` |
| **Depreciación mensual (Línea recta)** | **Lavado** | `Depreciacion` | `DepreciacionGasto (6814)` | `DepreciacionAcumulada (3913)` |
| **Mantenimiento rutinario (Prev./Corr.)** | **Méndez** | `MantenimientoActivo` | `MantenimientoGasto (6341)` | `FacturasPorPagar (4212)` |
| **Mejora capitalizable de activo** | **Méndez** | `MejoraActivo` | `MaquinariaEquipo (3331)` | `PasivoActivoFijo (4654)` |
| **Baja de activo fijo (Desuso/Retiro)** | **Méndez** | `BajaActivo` | `DepreciacionAcumulada (3913)` + `CostoNetoBajaActivo (6551)` | `MaquinariaEquipo (3331)` |

El asiento debe cuadrar (`Debe = Haber`) o `RegistrarAsiento` lanza error y no se guarda nada.

---

## 9. Git (Visual Studio: menú Git)
- **`main` está protegida:** nadie sube directo; todo entra por **Pull Request** con 1 aprobación. Quito revisa y une.
- Trabaja siempre en tu rama. Antes de empezar y cada día: extrae `main` → *Pull* → vuelve a tu rama y combina `main` en ella.
- Commits pequeños y frecuentes. Antes de abrir PR: compila, pruébalo y revisa en *Files changed* que solo toques tu zona.
- No subas `bin/`, `obj/`, `.vs/`, `appsettings` con claves ni `python-forecast/venv/`.

---

## 10. Cómo usar tu IA
Pásale este README + la carpeta `Models/` + tu `DA<Modulo>.cs`, y empieza con:
> *Soy responsable del módulo X de AgroERP. Solo puedo modificar mis archivos y mis tablas. Respeta las reglas del README: no cambies Models, DbContext ni Program.cs, no propongas migraciones, usa DAO + ViewModels, estados como texto exacto y un solo SaveChangesAsync por operación.*

---

## 11. Misión de cada módulo en Fase 2 (Tesorería + Activos Fijos)
- **Quito (`Tesoreria` + `BI`):** CRUD de `CuentasBancarias`, libro bancos (`MovimientosBancarios`), flujo de caja y liquidez (`Saldo inicial + Cobros - Pagos`), conciliación bancaria (primero registra ajuste contable de comisiones `6391` vs `1041`, luego verifica cuadre de saldos y cierra) y tarjetas financieras en BI.
- **Díaz (`Cobranzas`):** Bandeja de `CuentasPorCobrar` (nacen desde `ComprobantePago`), antigüedad de saldos (vigentes vs. vencidas), aplicación de `NotaCredito` para rebajar saldo pendiente, y registro de `CobranzaCliente` (invoca `DATesoreria.RegistrarMovimientoBancario` y contabiliza `1041` vs `1212`).
- **Changa (`Pagos`):** Bandeja de `CuentasPorPagar`, programación por vencimiento y prioridad (`Pendiente` → `Programado` → `Autorizado`), validación de saldo bancario disponible y registro de `PagoProveedor` (invoca `DATesoreria.RegistrarMovimientoBancario` y contabiliza `4212`/`4654` vs `1041`).
- **Lavado (`Activos`):** Registro del maestro `ActivoFijo` (código, categoría, serie, ubicación en `ZonaUbicacion`, responsable, costo, valor residual y vida útil en meses), capitalización/activación (crea CxP vía `DAPagos.CrearCuentaPorPagar` y contabiliza `3331` + `4011` vs `4654`) y motor de depreciación mensual por línea recta (`(Costo - ValorResidual) / VidaUtilMeses`, topeado al valor residual, contabilizando `6814` vs `3913`).
- **Méndez (`Patrimonio`):** Traslado de activos entre ubicaciones y custodios (`TransferenciaActivo` → invoca `DAActivos.ActualizarUbicacionYCustodio`), registro de mantenimientos preventivos/correctivos (`6341` vs `4212` + CxP) o mejoras capitalizables (`DAActivos.CapitalizarMejora` + `3331` vs `4654` + CxP), y baja patrimonial por desuso/retiro (`DAActivos.MarcarActivoDadoDeBaja` + asiento `3913` + `6551` vs `3331`).

---

## 12. Cronograma y orden de dependencia (Sprint de 3 días)
- **Día 1 (Base y Contratos):** Quito sube a `main` los nuevos modelos, la migración, las constantes en `Cuentas` y los 6 métodos de contrato (con su firma lista). Todos hacen *Pull* y `Update-Database`. Cada dueño implementa sus contratos primero.
- **Día 2 (Desarrollo de Pantallas y Flujos):**
  - *Rama Tesorería:* Quito (Cuentas bancarias y Flujo de caja) → Díaz (CxC y Cobros) y Changa (CxP y Pagos) → Quito (Conciliación bancaria).
  - *Rama Activos Fijos:* Lavado (Alta, Capitalización y Depreciación) → Méndez (Transferencias, Mantenimiento y Baja).
- **Día 3 (Integración y Prueba Secuencial):** Pull Requests a `main`, prueba de punta a punta (Venta → CxC → Cobro → Banco / Compra o Activo → CxP → Pago → Conciliación / Alta Tractor → Depreciación → Mantenimiento → Baja).

---

## 13. Problemas frecuentes
| Síntoma | Causa y solución |
|---|---|
| `Invalid object name 'dbo.CuentasBancarias'` (o cualquier tabla nueva) | Falta ejecutar `Update-Database` tras hacer *Pull* de `main`. |
| Error al registrar cobro o pago: "Saldo insuficiente" | La `CuentaBancaria` seleccionada no tiene `SaldoDisponible` suficiente; registra primero un saldo inicial o un cobro en `/Tesoreria`. |
| El asiento de capitalización o baja no cuadra | Verifica que `Debe == Haber` redondeado a 2 decimales (`Math.Round(x, 2, MidpointRounding.AwayFromZero)`). En la baja: `DepreciacionAcumulada + ValorNetoLibros` debe ser igual a `CostoAdquisicion`. |