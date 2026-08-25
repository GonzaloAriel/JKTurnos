# 00_MEMORIA.md

FORMATO: 1.0

## Proyecto

JKTurnos

SaaS de gestion de turnos y disponibilidad para cualquier negocio basado en tiempo.

No es un CRUD de turnos: es el centro operativo diario del negocio.

## Stack

ASP.NET MVC 5

.NET Framework 4.7

Entity Framework 6

SQL Server Express

OWIN Cookie Authentication

Claims (NameIdentifier, Name, Email, Role, TenantId)

Bootstrap

Repository Pattern

Monolito MVC

Multi Tenant por TenantId

## Arquitectura

Controller

↓

Service

↓

Repository

↓

Entity Framework

↓

SQL Server

Nunca romper este flujo.

Los Controllers solo coordinan.

Los Services contienen las reglas de negocio.

Los Repositories consultan y filtran por TenantId e IsActive.

Soft Delete obligatorio.

## Objetivo actual

Vinculacion Recurso-Servicio implementada (N:M bidireccional con checkbox y drawers de quick-create) y quick-create de Servicio + Recurso integrado en el formulario de Crear Turno (Servicio opcional, D-014).

Modulo Turnos CRUD consolidado.

Drawer de detalle del Time Block V1 (solo lectura, seccion Pagos/Seña visible) consolidado (2026-08-13, D-015/D-016). V2/V3 a futuro en Docs/16.

Vista movil de Agenda P1 (acordeon por Recurso + horarios libres + filtro de recursos, D-019) consolidada por TEAM LEAD (2026-08-13). P2 FAB "Nuevo Turno" (Drawer de Quick Create, D-017) ELIMINADO (2026-08-13): sin relevancia por ahora; D-017 CANCELADA y el boton "Nuevo Turno" de la Agenda navega a /Turnos/Create.

Referencia de la proxima etapa (Fase 3 del roadmap, Docs/02): Agenda, Turnos y Calendario. Time Blocks de Agenda con datos reales implementados y verificados visualmente (2026-08-11).

## Plan activo

Estado del ciclo: IMPLEMENTACION

Registrar aqui unicamente el paso en curso. Actualizar bajo confirmacion del TEAM LEAD.

Siguiente paso: P1 (vista movil) CONSOLIDADO (acordeon por Recurso con horarios libres, `_AgendaMobile.cshtml`, D-019; confirmado por TEAM LEAD 2026-08-13). P2 FAB "Nuevo Turno" (D-017) ELIMINADO por TEAM LEAD (2026-08-13), sin relevancia por ahora; el boton "Nuevo Turno" de la Agenda va a /Turnos/Create. P-003 CERRADO por TEAM LEAD (2026-08-13). Drawer de detalle del Time Block V1 (solo lectura, seccion Pagos/Seña visible) CONSOLIDADO (D-015/D-016); V2 V3 y demas futuros en Docs/16. Modulo Pagos (D-030/D-031) implementado 2026-08-19. Filtro de recursos en grilla desktop CONSOLIDADO (2026-08-19).

Ciclo en curso (2026-08-19, orden de prioridad definido por TEAM LEAD): 1) Modulo Notificaciones al Cliente (D-033) — Fase 1 WhatsApp via wa.me + plantillas configurables: CONSOLIDADA (aprobado por TEAM LEAD, 2026-08-19). Fase 2 email SMTP (boton "Enviar recordatorio por email" en el Drawer, plantillas y credenciales SMTP en Configuracion): CONSOLIDADA (aprobado por TEAM LEAD, 2026-08-25). 2) Modulo Dashboard en SECCION APARTE (D-034, nada en el Command Center) — Fase 1 CONSOLIDADA (KPIs del mes, ocupacion semanal, ingresos por dia, proximos turnos, sin confirmar, rankings, saldo pendiente; aprobado por TEAM LEAD, 2026-08-19). Fase 2 (pagos: variacion mensual, evolucion 12 meses, ingresos por recurso, pagos por tipo) CONSOLIDADA (aprobado por TEAM LEAD, 2026-08-19). Siguiente: Dashboard Fase 3 (exportacion CSV/PDF), segun TEAM LEAD.

Evolucion visual (D-035) CONSOLIDADA por TEAM LEAD (2026-08-25): identidad calida crema/sage/marron aplicada por postas seguras (1 paleta, 1.B aliases legacy + estados core, 2 tipografia Nunito titulos, 3 radios, 4 sombras calidas, 5 componentes badges/botones/toasts, 6 espaciado dirigido). Fuente unica de verdad de la identidad: `Content/DesignSystem/jk-tokens.css`. Detalle y deuda registrada en 01_ESTADO. Segunda ola CONSOLIDADA (2026-08-25): recursos vintage, drawer redondeado con seccion economica destacada, Command Center alineado a tokens, superficies/nav menta palida, login nuevo con capa decorativa y lavado pastel global; colores decorativos tokenizados (coral/butter/teal/firma tricolor) segun D-036.


## Fuentes de verdad

Docs/ contiene la documentacion funcional y de producto. No se duplica en AI/.

Docs/00_VISION.md - vision y filosofia del producto

Docs/01_AGENTS.md - reglas del proyecto para agentes

Docs/02_ROADMAP.md - roadmap y estado por fase

Docs/03_ARCHITECTURE.md - arquitectura tecnica oficial

Docs/04_BUSINESS_RULES.md - reglas de negocio

Docs/05_DESIGN_SYSTEM.md - design system

Docs/06_MIGRACION_DS.md - contexto de migracion del design system

Docs/07_MIGRACION_DS_FASE3.md - avance de migracion fase 3

Docs/08_SESION_20260722.md - resumen de sesion (EF/migrations, sidebar)

Docs/09_SESION_20260727.md - resumen de sesion (modulo Recursos)

Docs/11_ARQUITECTURA_FUNCIONAL.md - arquitectura funcional de Agenda y Turnos

Docs/12_CORE_DOMAIN_MODEL.md - modelo conceptual del nucleo

Docs/13_DER.md - modelo entidad-relacion oficial

Docs/13_AGENDA_UX.md - manual de UX de la Agenda

Docs/14_TURNO_LIFECYCLE.md - ciclo de vida del turno

Docs/16_DRAWER_TURNO_FUTURO.md - Drawer de Time Block V2/V3 (trabajo futuro)

Docs/JKTurnosDictionary.md - diccionario tecnico del proyecto

Docs/DashboardLayoutSystem.md - sistema visual y reglas de composicion (activa)

Nota: Docs/10_MIGRACION_RECURSOID.sql y Docs/generar_migracion.ps1 son assets de la migracion de RecurseoId (ver P-001/P-002).

## Archivos de esta memoria

Leer en este orden segun la tarea.

01_ESTADO.md - estado real por modulo. Solo el TEAM LEAD confirma CONSOLIDADO.

02_DECISIONES.md - decisiones tecnicas preservadas (IDs D-XXX).

03_PROBLEMAS.md - problemas conocidos (IDs P-XXX) y resueltos.

## Rutas de lectura por tarea

Toda tarea: leer este archivo y 01_ESTADO.md.

Tarea de implementacion: leer ademas 02_DECISIONES.md y las secciones de 03_PROBLEMAS.md que afecten al modulo.

Tarea de arquitectura o tecnologia: leer 02_DECISIONES.md completo.

Tarea de diagnostico o bug: leer 03_PROBLEMAS.md completo y 01_ESTADO.md del modulo.

Tarea de producto o UX: consultar Docs/ (AGENDA_UX, BUSINESS_RULES, DESIGN_SYSTEM, VISION).

## Reglas de la memoria

Solo el TEAM LEAD confirma que una etapa esta terminada, aprobada y funcionando.

Nunca auto-consolidar una etapa.

Una implementacion existente pero no confirmada no es CONSOLIDADO.

Lo que falla se registra en 03_PROBLEMAS.md.

Lo que se decide se registra en 02_DECISIONES.md.

Lo que se confirma se registra en 01_ESTADO.md.

No guardar conversaciones, logs, razonamientos internos ni opiniones temporales.

No duplicar Docs/.

La memoria permite a cualquier modelo continuar el trabajo sin conocer conversaciones anteriores.
