# AgroERP – Guía del equipo (Mi Paraíso Gramadal)

## 1. Cómo empezar (todos, una sola vez)
1. `git clone <repo>` y `git checkout main` → `git pull origin main`
2. Crea tu rama: `git checkout -b feature/<tu-modulo>`
3. Crea tu cadena de conexión en **User Secrets** (clic derecho al proyecto → Administrar secretos de usuario). No la subas a Git.
4. Consola del Administrador de paquetes: `Update-Database`
5. F5. El seed carga el plan de cuentas la primera vez.

## 2. Qué es tuyo
| Persona | Controller / DAO / Vistas | Rama |
|---|---|---|
| Quito | PlanificacionController, DAPlanificacion, DAContabilidad, Views/Planificacion, Dashboard BI | feature/core-ia-bi |
| Changa | ComprasController, DACompras, Views/Compras | feature/compras |
| Lavado | CalidadController, DACalidad, Views/Calidad | feature/recepcion-calidad |
| Méndez | AlmacenController, DAAlmacen, Views/Almacen | feature/almacenes |
| Díaz | DistribucionController, DADistribucion, Views/Distribucion | feature/distribucion |

Tablas que escribe cada dueño: ver comentario al inicio de su DA*.cs.

## 3. Reglas
1. **Lectura libre, escritura solo del dueño.** Puedes consultar cualquier tabla, pero solo modificas las tuyas.
2. **No tocar:** `ApplicationDbContext`, `Models/`, `_LayoutLogistica.cshtml`, `Program.cs`, `Migrations/`. Pídeselo a Quito.
3. **Solo Quito ejecuta `Add-Migration`.** Los demás solo `Update-Database`.
4. **Un solo `SaveChangesAsync` por operación.** Stock + estado + asiento se guardan juntos al final del flujo. Los DAO de contratos NO llaman SaveChanges.
5. **ViewModels:** las vistas nunca reciben entidades directas. Crea tus ViewModels en `ViewModels/<Modulo>/`.
6. **Estados = texto exacto**, sin variantes:
   - Requerimiento: `Borrador` `PendienteAprobacion` `Aprobado` `Rechazado`
   - OrdenCompra: `Emitida` `RecepcionadaParcial` `Cerrada`
   - Recepcion: `EnProceso` `Cerrada` `Anulada`
   - Lote: `Cuarentena` `Liberado` `Rechazado`
   - Reserva: `Activa` `Consumida` `Liberada`
   - Pedido: `Pendiente` `Picking` `Empacado` `EnRuta` `Entregado`
   - PlanAccion: `EnProgreso` `Completado`
7. **Contabilidad:** siempre `DAContabilidad.RegistrarAsiento(...)` con `Cuentas.Xxx`. Nunca códigos a mano ni insertar en tablas contables.

## 4. Contratos entre módulos (ya declarados, con `NotImplementedException`)
| Método | Lo implementa | Lo llama |
|---|---|---|
| `DACompras.CrearRequerimientoDesdeMrp(necesidadIds, usuarioId)` | Changa | Quito |
| `DACompras.MarcarOrdenRecepcionada(ocId, parcial)` | Changa | Lavado |
| `DAAlmacen.RegistrarIngreso(loteId, zonaId, cantidad, ref)` | Méndez | Lavado |
| `DAAlmacen.ReservarStock(detalleId, articuloId, cantidad)` | Méndez | Díaz |
| `DAAlmacen.LiberarReserva(pedidoId)` | Méndez | Díaz |
| `DAAlmacen.ConfirmarSalida(ordenPickingId)` | Méndez | Díaz |
| `DAAlmacen.ReingresarDevolucion(devolucionId)` | Méndez | Díaz |

Cada dueño implementa los suyos en los **primeros días**: los demás dependen de ellos. Mientras no estén, prueba con datos insertados a mano en SQL.

## 5. Asientos contables por evento
| Evento | Quién | Debe | Haber |
|---|---|---|---|
| Recepción conforme | Lavado | MaterialesAux / Envases | FacturasPorPagar |
| Merma / ajuste negativo | Méndez | MermasGasto | MaterialesAux / ProductoTerminado |
| Factura | Díaz | FacturasPorCobrar | VentasProductos + IgvPorPagar |
| Nota de crédito | Díaz | VentasProductos + IgvPorPagar | FacturasPorCobrar |
| Flete | Changa | FletesGasto | FacturasPorPagar |

## 6. Flujo diario de Git
- Mañana: `git pull origin main` en tu rama (traes lo de los demás).
- Trabajas solo en tu zona. Commits pequeños y frecuentes.
- Cada 2–3 días: Pull Request a `main`. Quito revisa y une. **Nadie hace push directo a `main`.**
- ¿Conflicto? No lo resuelvas a ciegas: avisa a Quito.

## 7. Orden de dependencia (quién no debe atrasarse)
Compras → Recepción → Almacén → Distribución → BI.

## 8. Día 1 (sin código)
Entrega a Quito: lista de tus pantallas y 3 reglas de negocio clave de tu módulo.
