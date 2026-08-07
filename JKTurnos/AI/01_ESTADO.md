# 01_ESTADO.md

FORMATO: 1.0

## Regla de estados

CONSOLIDADO - probado y confirmado explicitamente por el TEAM LEAD como terminado, aprobado y funcionando.

EN PROGRESO - trabajo activo, aun no terminado ni confirmado.

PENDIENTE - todavia no realizado.

PROBLEMA CONOCIDO - implementacion existe pero se sabe que falla. Detalle en 03_PROBLEMAS.md.

Nunca marcar una etapa como CONSOLIDADO por cuenta propia.

Solo el TEAM LEAD puede confirmar que algo esta terminado, aprobado y funcionando.

## Indice por modulo

| Modulo | Estado | Ultima confirmacion |
|--------|--------|---------------------|
| Infraestructura | EN PROGRESO (base existente) | No - pendiente |
| Seguridad / Autenticacion | EN PROGRESO | No - pendiente |
| Clientes | EN PROGRESO (CRUD presente) | No - pendiente |
| Servicios | EN PROGRESO (CRUD presente) | No - pendiente |
| Recursos | EN PROGRESO (CRUD presente) | No - pendiente |
| Turnos | CONSOLIDADO | 2026-08-06 - TEAM LEAD |
| Agenda | EN PROGRESO (maqueta visual) | No - pendiente |
| Home / Command Center | PENDIENTE (solo Index minimo) | No - pendiente |
| Design System | EN PROGRESO (DS v2 activo) | No - pendiente |
| Configuracion | PENDIENTE | No - pendiente |

* Estado registrado por inspeccion del codigo el 2026-08-06.

## Detalle por modulo

### Infraestructura

Estado: EN PROGRESO

Confirmado por TEAM LEAD: No

Notas: EF6 + migraciones + seed existen (ver Docs/08). Docs/03 y Docs/08 la describen como finalizada. Sin confirmacion TEAM LEAD registrada.

### Seguridad / Autenticacion

Estado: EN PROGRESO

Confirmado por TEAM LEAD: No

Notas: Login/Logout, cookie OWIN, claims y roles existen (AccountController, AuthService, entidad User). Sin confirmacion TEAM LEAD registrada.

### Clientes

Estado: EN PROGRESO

Confirmado por TEAM LEAD: No

Notas: CRUD presente (ClientesController, ClienteService, vistas Index/Create/Edit/Delete). Docs/02 y Docs/03 lo listan como completo. Sin confirmacion TEAM LEAD registrada.

### Servicios

Estado: EN PROGRESO

Confirmado por TEAM LEAD: No

Notas: CRUD presente (ServiciosController, ServicioService, vistas Index/Create/Edit/Delete). Entidad expone DuracionMinutos (usado por TurnoService). Sin confirmacion TEAM LEAD registrada.

### Recursos

Estado: EN PROGRESO

Confirmado por TEAM LEAD: No

Notas: CRUD presente (RecursosController, RecursoService, vistas Index/Create/Edit). Incluye componente ColorSelector. Sin vista Delete dedicada. Sin confirmacion TEAM LEAD registrada.

### Turnos

Estado: CONSOLIDADO

Confirmado por TEAM LEAD: Sí (2026-08-06)

Notas: CRUD completo implementado (Index, Create, Edit, Delete). Modelo, Service, Controller y Vistas alineados con patrones del proyecto. P-001 y P-002 resueltos.

### Agenda

Estado: EN PROGRESO

Confirmado por TEAM LEAD: No

Notas: Maqueta visual (_AgendaToolbar + _AgendaGrid + Turnos/Agenda.cshtml con jk-agenda.js/css). PrimaryAction="#" y sin datos reales (el controller solo pasa fecha actual). No evaluada. Siguiente paso: lógica de time blocks.

### Home / Command Center

Estado: PENDIENTE

Confirmado por TEAM LEAD: No

Notas: Solo HomeController.Index minimo (ViewBag TenantId/UserId). El objetivo Command Center (Docs/02, Docs/13) no esta implementado. No evaluado TEAM.

### Design System

Estado: EN PROGRESO

Confirmado por TEAM LEAD: No

Notas: DS v2 activo en Content/DesignSystem/ (tokens, base, layout, components, utilities) y multiples partials en Views/Shared/Components/ (incluidas _AgendaToolbar y _AgendaGrid). Docs/06 describe migracion progresiva (fase 2/3 logada). Sin confirmacion TEAM LEAD registrada.

### Configuracion

Estado: PENDIENTE

Confirmado por TEAM LEAD: No

Notas: Sin implementacion. No evaluado.

## Como actualizar

Cuando el TEAM LEAD confirme una etapa, registrar aqui bajo su autorizacion:

- Estado: CONSOLIDADO
- Que termino
- Verificado por / fecha
- Referencias utiles (ids de decisiones D-XXX y problemas P-XXX)