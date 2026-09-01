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

### M-08 PORTAL DE RESERVAS ONLINE + WHATSAPP  [PRIORIDAD ALTA - Fase 6]
- Portal publico por subdominio: el cliente elige recurso/fecha y reserva.
- Reserva via WhatsApp con link precargado. Cierra clientes para el SaaS.
- Riesgo: MEDIO-ALTO. Requiere tenant publico (ya existe subdominio).

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
