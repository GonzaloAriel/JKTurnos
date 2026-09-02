# PLAN DE MEJORAS Y MODULOS FUNDAMENTALES

FORMATO: 1.0
Estado del documento: PLAN (no implementado salvo lo indicado).

## Contexto
JKTurnos es un SaaS multi-tenant de gestion de turnos adaptable a cualquier
actividad basada en tiempo. NO tendra sistema de cobros (decision del TEAM LEAD).

Este archivo consolida las mejoras y modulos fundamentales propuestos,
priorizados, para no perder el contexto. Cada item indica prioridad, alcance,
riesgo y estado.

## Prioridad corta (implementar primero)

### M-01 HORARIOS LABORABLES CONFIGURABLES POR RECURSO  [PRIORIDAD ALTA]
- Problema: la jornada laboral esta hardcodeada a 08:00-19:30 (D-012) en
  `DashboardService` y `HomeService` (MinutosJornada = 690). No se adapta a
  cualquier actividad (canchas, consultorios, salones) con horarios distintos.
- Alcance: CRUD de horario por Recurso (apertura/cierre por dia de semana) +
  uso de esa jornada en la Agenda (columnas/horas) y en el calculo de ocupacion.
- Habilitante: es el ladrillo para el "Re agendar" inteligente y una agenda real.
- Riesgo: MEDIO. Toca la Agenda (core) y agrega modelo de datos + migracion.
- Estado: EN PROGRESO (2026-08-31).

### M-02 BUSQUEDA GLOBAL (LUPA DEL TOPBAR)  [PRIORIDAD ALTA]
- Problema: el roadmap lista "Buscador Global" del topbar como pendiente; hoy no
  existe una busqueda que encuentre cliente/servicio/turno rapidamente.
- Alcance: buscador en el topbar que consulta Clientes y Turnos (por cliente,
  servicio, recurso) con autocomplete y redireccion al detalle.
- Riesgo: BAJO. Endpoint de lectura + UI en topbar.
- Estado: EN PROGRESO (2026-08-31).
- Avance (2026-09-01, pendiente confirmacion TEAM LEAD): los turnos de la
  busqueda ya NO navegan a la Agenda. El autocomplete del topbar y las cards de
  la pagina de resultados abren el drawer de detalle del turno (`GetDetailDrawer`)
  en la misma pagina via `Scripts/jk-turnos-drawer.js` (compartido, expone
  `window.openTurnDrawer(id)`; drawer global en `_Layout`, drawer local en
  Busqueda). Se agrego ademas un icono de detalle (ojo, `data-ver-turno`) en la
  grilla de Turnos (desktop y mobile) que abre el mismo drawer en esa pagina
  (`Views/Turnos/Index.cshtml`). MSBuild Debug 0 errores.

### M-03 CLIENTES: HISTORIAL + CRM LIGERO + BUSQUEDA INTELIGENTE  [PRIORIDAD ALTA]
- Problema: el roadmap marca en Clientes (pendiente): Historial, ultimo turno,
  proximo turno, CRM ligero. Hoy el cliente es un registro disperso.
- Alcance: ficha de cliente con historial de turnos, ultimo/proximo turno y
  contacto; busqueda inteligente.
- Riesgo: BAJO-MEDIO. Blend de consultas existentes + vista.
- Estado: EN PROGRESO (2026-08-31).

## Agenda del medio plazo

### M-07 AGENDA DRAG & DROP + MOVER TURNO  [PRIORIDAD ALTA - Fase 3 del roadmap]
- El corazon del sistema: mover el timeblock arrastrandolo y/o "Mover turno"
  desde el drawer. Conecta con el "Re agendar" mejorado.
- Riesgo: MEDIO. UX de arrastre + persistencia + validacion de disponibilidad.

AVANCE (2026-09-01): EN PROGRESO. Backend + frontend implementados:
- `TurnoService.Mover(turnoId, recursoId, nuevaFechaHoraInicio)`: recalcula fin
  con `CalcularFin`, valida disponibilidad excluyendo el propio turno
  (`ValidarDisponibilidad`), solo mueve turnos Pendiente/En curso (D-040) y
  rechaza mover al pasado. Reusa `_repo.Update` + auditoria.
- `TurnosController.Mover`: POST ajax + anti-forgery (patron QuickCreate D-013).
- Drag & Drop HTML5 (native) en `_AgendaGrid` (snap 30 min por celda) y
  `_AgendaGridSemanal` (snap 5 min por posicion, conserva el recurso del
  timeblock). Solo los bloques movibles son draggable=true.
- `jk-agenda.js`: `shift+dragover/drop`, flag `dragJustEnded` que suprime el
  click sintetico del drop (no abre drawer ni Quick Create),
  `body.jk-dragging` desactiva pointer-events de bloques durante el arrastre,
  toast + recarga de la agenda al mover.
- `_TurnoDetailDrawer`: boton "Mover turno" con formulario inline (recurso/
  fecha/hora) que POSTea a Mover (alternativa para mobile). Reemplaza el link
  "Re agendar" que navegaba a Edit.
- `TurnoDetailViewModel`: +`RecursoId` y +`Recursos` (dropdown).
- **Refresco suave sin F5** (pedido del usuario): `TurnosController.GetAgendaGrid`
  devuelve el partial de la grilla (Dia/Semana) por AJAX; `Agenda.cshtml` agrega
  `data-agenda-content-url` y envuelve la grilla en `<div id="jk-agenda-grid-container">`;
  `jk-agenda.js.refreshAgendaGrid()` reemplaza solo el inner del wrapper via
  DOMParser (conservando scroll + listeners de teclado). Mover por drag y por
  "Mover turno" ya no recarga la pagina completa (sin parpadeo F5 ni salto al inicio).
- Compila 0 errores; `node --check` OK. Refresco suave confirmado por TEAM LEAD
  (2026-09-01): al mover el timeblock (drag y "Mover turno") ya no hay F5 ni salto
  al inicio, el bloque aparece en su nueva posicion y el drawer sigue abriendo.
  CONSOLIDADO por TEAM LEAD (2026-09-01).

### M-08 PORTAL DE RESERVAS ONLINE + WHATSAPP  [PRIORIDAD ALTA - Fase 6]
- Portal publico por subdominio: el cliente elige recurso/fecha y reserva.
- Reserva via WhatsApp con link precargado. Cierra clientes para el SaaS.
- Riesgo: MEDIO-ALTO. Requiere tenant publico (ya existe subdominio).
- Avance (2026-09-01): migracion `AddPortalPublico` (+`PortalHabilitado`),
  `PortalReservaService`, `AuditService.RegistrarPublico`, `PortalController`
  [AllowAnonymous], `PortalViewModel` (con `FechaLegible`/`FechaHoraLegible`),
  layout publico `_PortalLayout`, `jk-portal.css`, vistas `Inicio/FechaHora/
  Datos/Confirmacion/NoDisponible`, checkbox `PortalHabilitado` en DatosNegocio,
  appSetting `JKPortalDominioRaiz`, rewritepath raiz en `Global.asax`.
  MSBuild `Debug` y `/p:MvcBuildViews=true` = 0 errores.
- D-0xx (2026-09-01): en este proyecto las vistas precompiladas usan la variable
  de modelo `Model` (mayuscula, NO `model`) y los bloques de control (`if`/
  `foreach`/`using`) dentro de un bloque `@using(){ }` van SIN prefijo `@`.
  Por eso las vistas del portal usan `@Model.X` y `@using(...) { if(...) }`.
- D-0xx (2026-09-01): las fechas `DateTime?`/`string` no se formatean dentro de
  las vistas; el controlador pre-armo `FechaLegible` y `FechaHoraLegible` para
  evitar llamar `.ToString(fmt)` sobre valores nullable/string en Razor
  (falla la compilacion de vistas con `MvcBuildViews=true`).

### M-09 CONFIGURACION PROGRESIVA + ONBOARDING  [PRIORIDAD MEDIA - Fase 4]
- Wizard inicial que guie el alta (logo, horarios, servicio, recurso) sin obligar.
- Riesgo: BAJO-MEDIO.

## Otras mejoras (diferibles)

- M-04 Notificaciones/recordatorios automaticos programados (base D-033 ya existe;
  falta disparo X horas antes en lugar de solo envio manual).
- M-05 Confirmacion de turnos (estado "Confirmar") cerrando el loop con
  recordatorios y dashboard de "sin confirmar".
- M-06 Reportes y exportacion (Fase 5): exportar agenda/clientes a CSV/Excel +
  metricas operativas (ocupacion por recurso, turnos por servicio, no-show).
- M-10 Roles/permisos mas finos por modulo (hoy AdminGlobal/Admin/User gruesos).
- M-11 Notas en turno (el roadmap lista "Notas" como accion del turno).

## EXPLICITAMENTE DESCARTADO
- M-COBROS: JKTurnos NO tendra sistema de cobros (decision del TEAM LEAD).

## Orden sugerido de ejecucion
1. M-01 horarios configurables  (habilita M-07 re-agendar real)
2. M-02 busqueda global
3. M-03 clientes historial/CRM
4. M-07 mover turno / drag & drop
5. M-08 portal de reservas online
6. M-09 onboarding
