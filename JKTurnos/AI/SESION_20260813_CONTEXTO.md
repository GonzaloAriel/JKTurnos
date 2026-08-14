# SESION 2026-08-13: RESUMEN DE ESTADO Y CONTEXTO

## Objetivo
Implementar el Drawer de detalle del Time Block (D-003) en la Agenda en su V1 SOLO LECTURA, y registrar las iteraciones V2 y V3 como trabajo a FUTURO en un archivo dedicado.

## Estado
- Drawer de detalle del Time Block (V1 solo lectura; seccion Pagos/Seña SIEMPRE visible con placeholder "Sin senia"): CONSOLIDADO (aprobado por TEAM LEAD, 2026-08-13).
- V2 (acciones de estado, reagendar, editar, auditoria) y V3 (estado economico / senia / pago): A FUTURO. Registrados en Docs/16_DRAWER_TURNO_FUTURO.md. No implementar hasta confirmacion del TEAM LEAD.
- Agenda: EN PROGRESO (P-003 abierto: no muestra turnos, posible mismatch de TenantId).

## Cambios

### Drawer de detalle del Time Block (V1, de esta sesion)
- `Models/ViewModels/TurnoDetailViewModel.cs` (nuevo): datos de cliente/servicio/recurso, estado (nombre y color), inicio/fin, observaciones, y `EstadoEconomico` nullable como slot para V3 (seccion Pagos SIEMPRE visible, placeholder "Sin senia" cuando es null).
- `Controllers/TurnosController.GetDetailDrawer(int id)`: [GET], devuelve `PartialView("~/Views/Shared/Components/_TurnoDetailDrawer.cshtml", model)` o HttpNotFound. Vive en TurnosController (no se crea AgendaController, D-016).
- CORRECCION DE BUG: `PartialView("_TurnoDetailDrawer")` no encontraba el partial porque `Views/Shared/Components/` no esta en las rutas default del view engine de MVC 5 (solo `Views/{controller}/` y `Views/Shared/`). Devolvia 500 "partial not found" y el fetch mostraba el toast "No se pudo cargar el detalle del turno". Se fijo con la ruta virtual completa.
- `Views/Shared/Components/_TurnoDetailDrawer.cshtml` (nuevo): solo CONTENIDO (secciones: cliente+estado, servicio/precio/duracion, recurso, fecha-hora, observaciones, Pagos/Seña siempre visible). No renderiza el drawer completo; se inyecta via fetch. Sin botones de accion (V1 lectura).
- `Views/Turnos/Agenda.cshtml`: contenedor fijo del drawer (`#drawer-turno-detalle` + backdrop + close), toast `#jk-toast-agenda` y atributo `data-turno-detail-url` en `.jk-agenda`.
- `Scripts/jk-agenda.js`: delegacion de click y de teclado (Enter/Space) sobre `.jk-agenda-time-block`, fetch a GetDetailDrawer, estado de carga, cierre por close/backdrop/Escape, limpieza del body y devolucion de foco al bloque. Toast de error si falla.
- `Content/DesignSystem/jk-components.css`: nuevas clases del drawer content (`.jk-drawer-section`, `.jk-drawer-subtitle`, `.jk-drawer-label`, `.jk-drawer-loading` reutilizando `jk-spin`).
- `JKTurnos.csproj`: includes de TurnoDetailViewModel, _TurnoDetailDrawer y Docs/16_DRAWER_TURNO_FUTURO.md.

### Documentacion de futuro
- `Docs/16_DRAWER_TURNO_FUTURO.md` (nuevo): V2 (acciones segun estado, reagendar, editar, auditoria; referencias a Docs/14) y V3 (senia/pago, independencia D-005), mas "fuera de alcance" (drag & drop, walk-in, vista semana/mes, notificaciones).

## Decisiones nuevas
- D-015: Drawer V1 solo lectura con slot de EstadoEconomico; seccion Pagos/Seña SIEMPRE visible (placeholder "Sin senia"); V2/V3 a futuro en Docs/16.
- D-016: Drawer con contenedor fijo + fetch inyecta contenido; GetDetailDrawer en TurnosController; _Drawer.cshtml (RenderBody) no se reutiliza.
- D-017: FAB y boton "Nuevo Turno" en Agenda abren un Drawer de Quick Create, NUNCA /Create; la implementacion del Drawer es posterior.
- D-018: vista movil = grilla compacta (REEMPLAZADA por D-019).
- D-019: vista movil = acordeon por Recurso (details/summary) + horarios libres por recurso; la grilla se oculta en <768px; reemplaza a D-018 en la vista movil.

## Reglas de negocio aplicadas
- D-009: textos visibles sin acentos (documentacion y UI).
- D-003: el detalle abre en Drawer, nunca pantalla nueva.
- D-005: el estado economico no se mezcla con el operativo (slot separado, seccion Pagos visible con placeholder).

## Pendientes
1. P-003 CERRADO por TEAM LEAD (2026-08-13). Cierre registrado en 03_PROBLEMAS (RESUELTOS).
2. P1 (vista movil) implementado: acordeon por Recurso con horarios libres (`_AgendaMobile.cshtml`, details/summary, D-019; reemplaza a D-018 en la vista movil). La grilla se oculta en <768px. El calculo de horarios libres vive en la vista (se extraera a servicio cuando D-017 lo necesite). Pendiente confirmacion visual de TEAM LEAD.
3. P2 FAB "Nuevo Turno": pendiente. D-017: debe abrir un Drawer de Quick Create (implementacion del Drawer posterior), nunca /Create.

## Build
- MSBuild Debug: 0 errores.