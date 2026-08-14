# 16_DRAWER_TURNO_FUTURO.md

## Drawer de Time Block - Trabajo futuro (V2 y V3)

A una vista de: 2026-08-12, TEAM LEAD aprobo la V1 (Drawer de detalle de Time Block en solo lectura). Este archivo registra las iteraciones siguientes, que quedan a futuro. No implementar hasta que el TEAM LEAD lo confirme.

Referencias: Docs/13_AGENDA_UX.md (secciones 5 y 14), Docs/14_TURNO_LIFECYCLE.md (mapa de transiciones, auditoria, matriz de permisos), D-005 (estados operativos y economicos independientes).

---

## V2 - Acciones de estado y edicion desde el Drawer

Objetivo: pasar del Drawer solo lectura a un Inspector operable, sin abandonar la Agenda (D-003).

### Alcance
- Botones de accion segun el estado actual (render condicional):
  - Pendiente: Confirmar, Cancelar.
  - Confirmado: Iniciar, Cancelar.
  - En curso: Finalizar.
  - Finalizado / Cancelado: sin acciones (solo lectura, bloqueado por lifecycle).
- Reagendar (cambiar fecha/hora) respetando disponibilidad, motivo obligatorio y estado Reagendado.
- Editar datos del turno.
- Confirmacion de cancelacion con motivo (panel inline del Drawer, nunca pantalla nueva).
- Auditoria por cada accion (Docs/14 seccion 7: fecha, responsable, accion, estado anterior/posterior, detalle).

### Reglas a respetar (Docs/14)
- Transiciones explicitas segun la matriz.
- Un turno cancelado no se reactiva; un finalizado no se modifica.
- Recurso no puede tener dos turnos en el mismo horario (ValidarDisponibilidad ya existe en TurnoService).
- Solo administrador puede cancelar un turno En curso.

### Notas
- `TurnoService` ya tiene `CambiarEstado`, `Cancelar`, `Update` y `ValidarDisponibilidad`; falta crear `Reagendar` y el registro de auditoria.
- No existe entidad de auditoria todavia: crear `TurnoHistorial` (o equivalente) antes de implementar.

---

## V3 - Estado economico (senia / pago)

Objetivo: mostrar y operar el estado economico del turno en el Drawer, respetando D-005.

### Alcance
- Seccion "Pagos" en el Drawer, hoy oculta (slot reservado en `TurnoDetailViewModel.EstadoEconomico`).
- Estados economicos: Sin senia, Senia pagada, Pago parcial, Pago completo.
- Acciones: Pagar senia, Registrar pago (parcial / completo).
- Reglas economicas (Docs/14): independencia respecto del estado operativo; no superar el precio del servicio; senia una sola vez; monto acumulado coherente.
- (Futuro) registros en caja.

### Notas
- Todavia no existe modelo/entidad de pagos. Crear entidad/servicio antes de implementar (no adelantar la UI sin el modelo).
- La V1 del Drawer ya deja preparado el campo `EstadoEconomico` nullable y la seccion "Pagos" SIEMPRE visible (placeholder "Sin senia" hasta que exista el modelo), de modo que al llegar el modelo el diseno del Drawer no cambia.

---

## Agenda movil y FAB (plan aprobado por TEAM LEAD, 2026-08-13)

No confundir con V2/V3 del Drawer. Es el plan priorizado para la Agenda movil. Bloqueado por P-003 (P0).

P1 - Vista movil en tarjetas (un solo DOM):
- NO duplicar markup: `_AgendaGrid` se reestructura por CSS Grid en `Content/Pages/jk-agenda.css` (media query max-width 767px) a timeline de un carril con columna de hora.
- Altura proporcional a la duracion; banda = color de recurso (D-004/D-011); superposicion por horario con dot de recurso; clic/Enter abren el MISMO Drawer de detalle.
- Sin JS de resize (la media query lo resuelve). Los estilos van en Pages, no en el DS (D-010). Solo un componente generico (`.jk-fab`) al DS.

P2 - FAB "Nuevo Turno":
- El FAB (bottom-right) y el boton del PageHeader en Agenda DEBEN abrir un Drawer de Quick Create de Turno (D-017). NUNCA `/Turnos/Create`.
- La implementacion del Drawer de creacion es un paso posterior; mientras tanto la accion queda pendiente (dejar sin accion, no loquear a /Create ni href="#").
- Inspirado en AGENDA_UX seccion 7 (FAB) y regla responsive (seccion 11).

---

## Fuera de alcance de V2/V3
- Drag & drop para mover bloques y resize de duracion (AGENDA_UX seccion 4).
- Walk-in desde la Agenda.
- Vista Semana y Mes (navegacion temporal).
- Notificaciones y recordatorios al cliente.
- Filtro de recursos en Bottom Sheet y navegacion temporal simplificada de mobile (resto de la seccion mobile de AGENDA_UX, iteracion posterior a P1/P2).