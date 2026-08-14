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

## Agenda movil (plan aprobado por TEAM LEAD, 2026-08-13)

No confundir con V2/V3 del Drawer. Es el plan priorizado para la Agenda movil.

P1 - Vista movil (agenda por Recurso en tarjetas):
- CONSOLIDADO (2026-08-13, confirmacion visual de TEAM LEAD) como acordeon por Recurso (`_AgendaMobile.cshtml`, `<details>/<summary>`, sin JS): cada recurso es una tarjeta desplegable; al expandir muestra sus turnos del dia (reutiliza `.jk-agenda-time-block` y el MISMO Drawer de detalle) y los HORARIOS LIBRES (franjas de 30 min desocupadas 08:00-19:30 fusionadas, chips "Libre HH:mm - HH:mm"). El primero nace expandido. En <768px la grilla desktop se oculta via media query (jk-agenda.css). Fondo de cada tarjeta teñido suavemente con el color del recurso (color-mix + `--jk-block-color`). Decision D-019 (reemplaza a D-018 en la vista movil).
- El filtro de recursos del toolbar es un dropdown profesionalizado (`<details>` en `_AgendaToolbar`, con titulo "Mostrar recursos" y filas alineadas). Decidido (2026-08-13): la opcion (a) — se CONECTA a las tarjetas del acordeon movil (JS oculta/muestra cada tarjeta segun su checkbox) y se OCULTA en desktop hasta definir el filtro de la grilla. El filtro de grilla queda pendiente (Docs/16 fuera de alcance).
- El calculo de horarios libres vive hoy en la vista (coherente con "el controller entrega datos, la vista renderiza").

P2 - FAB "Nuevo Turno" (ELIMINADO 2026-08-13):
- El plan de un Drawer de Quick Create para el FAB y el boton del PageHeader (D-017) fue ELIMINADO por TEAM LEAD: sin relevancia por ahora. D-017 CANCELADA.
- El boton "Nuevo Turno" del PageHeader de la Agenda navega a /Turnos/Create (como el resto del sistema).

---

## Fuera de alcance de V2/V3
- Drag & drop para mover bloques y resize de duracion (AGENDA_UX seccion 4).
- Walk-in desde la Agenda.
- Vista Semana y Mes (navegacion temporal).
- Notificaciones y recordatorios al cliente.
- Navegacion temporal simplificada de mobile (resto de la seccion mobile de AGENDA_UX). El filtro de recursos se resolvio con un dropdown en el toolbar (2026-08-13), no Bottom Sheet.