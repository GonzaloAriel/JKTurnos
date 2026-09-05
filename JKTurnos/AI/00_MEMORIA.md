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

Siguiente paso: pendiente definicion del TEAM LEAD.

M-13 VIDRIERA PUBLICA: COMPLETA (postas 0-6 y 8, 2026-09-04). Vista publica `/vidriera/{slug}` con disponibilidad semanal real, panel de gestion, refresh periodico (5 min) y vista "no disponible" dedicada. Posta 7 (Digital Signage / Pantalla de Contenido) redefinida como modulo separado futuro (D-047): requiere CMS basico para que el admin suba imagenes, configure secuencias de rotacion y gestione contenido promocional custom. No es extension de la vidriera.

Ciclo AG (2026-08-26): postas AG-0..AG-5 y AG-7 implementadas, AG-6 EXCLUIDA (D-038), AG-8 y AG-9 IMPLEMENTADAS. AG-8: PagosSuscripcionController reescrito (Index con filtros tenant/rango fechas + Registrar con model binding via RegistrarPagoViewModel + _FormLayout). Boton "Registrar pago" en grilla Tenants/Index. Vista dedicada Configuracion/Suscripcion (card estado suscripcion + historial de pagos). Sidebar "Pagos Plataforma" en grupo Sistema. AG-9: Dashboard Admin Global minimo (shell con contadores de tenants por estado). AG-10: Avatares de Plataforma — entidad AvatarPlataforma (global), CRUD AdminGlobal, selector de avatares en Configuracion/DatosNegocio, avatar priorizado en BaseController (AvatarUrl > LogoUrl > icono), sidebar subtit

Nuevo ciclo (2026-08-26, aprobado por TEAM LEAD): PLAN AG - Suscripciones, Pagos de Plataforma y Admin Global. Plan completo en AI/SESION_20260826_PLAN_SUSCRIPCIONES.md (decisiones D-037/D-038). AG-0 CONGELADA (2026-08-26): estados derivados solo para mostrar (SuspendidoManual > SinPlan > Vencido > EnGracia > PorVencer > Activo; aviso 7 dias / gracia 5 dias), Desactivar = informativo puro, tenants nacen/existen SIN PLAN, pago de plataforma = confirmado con un campo Observacion y recálculo de vencimiento (AddMonths desde vencimiento vigente si no vencio; desde hoy si vencio). Posta siguiente: AG-8 (gestion de pagos de plataforma UI), pendiente autorizacion. Posta AG-6 EXCLUIDA del alcance: suspension solo manual, sin bloqueo automatico (D-038). AG-1 IMPLEMENTADA (2026-08-26, pendiente confirmacion TEAM LEAD): Tenant con PlanCodigo/FechaInicioPeriodo/FechaVencimiento/SuspendidoManual; migracion aditiva `AddSuscripciones` aplicada a la BD local con backfill SIN PLAN verificado; catalogo estatico `PlanesSuscripcion` (Mensual/Trimestral/Semestral/Anual, precios sugeridos 0, DiasAvisoPorVencer=7, DiasGraciaPlataforma=5) y calculador puro `CalcularNuevoVencimiento` probado (20 casos OK). AG-2 IMPLEMENTADA (2026-08-26, pendiente confirmacion TEAM LEAD): `TenantService.Desactivar/Activar` (solo tocan SuspendidoManual; idempotentes con guard no-op) + endpoints POST `TenantsController/Desactivar|Activar` (patron DeleteConfirmed, anti-forgery, clase ya [Authorize(AdminGlobal)]); verificado con harness contra BD real (6 casos OK, tenant queda como estaba); sin UI todavia (la grilla con acciones llega en AG-7). AG-3 IMPLEMENTADA (2026-08-26, pendiente confirmacion TEAM LEAD): entidad `PagoSuscripcion` (TenantId/PlanCodigo/Monto/Fecha/Observacion nvarchar(500)/UsuarioId; autocontenida sin FKs ni navegaciones; NO implementa ITenantEntity: acceso via GlobalRepository D-022); DbSet + `MaxLength(500)` en JKContext; migracion aditiva `AddPagosSuscripciones` aplicada (tabla `dbo.PagoSuscripcions`, pluralizador consistente con Pagoes/Turnoes); `PagoSuscripcionService.Registrar(...)` con validaciones (tenant existe, plan valido, monto>=0, observacion<=500) y SOLO registra el pago (la actualizacion de la suscripcion es el flujo transaccional de AG-4); harness 14 casos OK y pago de prueba eliminado. AG-4 IMPLEMENTADA (2026-08-26, pendiente confirmacion TEAM LEAD): `PagoSuscripcionService.Registrar(...)` ahora es TRANSACCIONAL (pago + actualizacion de suscripcion en una sola `Database.BeginTransaction`): extiende desde vencimiento vigente si no vencio (renovacion anticipada, conserva dias) o parte de la fecha del pago si vencio/sin plan; `FechaInicioPeriodo`=fecha del pago y `PlanCodigo` del tenant = ultimo pago; NO toca SuspendidoManual ni IsActive. Para compartir conexion entre repos se agrego a `GlobalRepository` un ctor interno con contexto compartido (AG-4). Verificado con harness contra BD real: fase1 caso feliz 11 OK (sin plan/anticipada/ultimo-pago-manda), rollback atomico OK (CHECK temporal forzo error a mitad de transaccion: ni el pago ni el tenant quedaron escritos), fase3 vencido 3 OK (parte de hoy); BD restaurada al estado original (tenants SIN PLAN, tabla pagos en 0); MSBuild Debug 0 errores. AG-5 IMPLEMENTADA (2026-08-26, pendiente confirmacion TEAM LEAD): card "Suscripcion" solo lectura al inicio de `Configuracion/Index` (plan/estado/inicio/vencimiento/tiempo restante con badge del DS segun estado; CSS cero nuevo); `TenantService.GetSuscripcionTenant(tenantId)` arma `SuscripcionTenantViewModel` (nuevo) y la derivacion de estado vive como logica pura en `PlanesSuscripcion.CalcularEstado(...)` (constantes Estado*: SUSPENDIDO>SIN PLAN>VENCIDO>EN GRACIA>POR VENCER>ACTIVO, comparacion por dia calendario); `ConfiguracionController.Index` la carga via ViewBag usando TenantId del claim. Verificado: harness 19 casos OK (limites exactos de aviso/gracia + VM real ACTIVO/EN GRACIA/SIN PLAN), vistas precompiladas con aspnet_compiler (exit 0), MSBuild 0 errores. NOTA HARNESS: reusar una instancia de service entre UPDATEs externos devuelve entidades stale por identity resolution de EF; instanciar service nuevo por lectura. AG-7 IMPLEMENTADA (2026-08-26, pendiente confirmacion TEAM LEAD): `Views/Tenants/Index` es ahora el panel operativo del Admin Global (reusa _CrudPage/sidebar Sistema): strip de contadores con `_KpiCard` (Activos/PorVencer/EnGracia/Vencidos/Suspendidos/SinPlan) + grilla Tenant (estado badge/plan/vencimiento/tiempo restante) + acciones Activar/Desactivar con confirm y anti-forgery (endpoints AG-2) + Editar/Eliminar existentes; variante movil en cards. Nuevo `PanelTenantsViewModel` + `TenantService.GetSuscripciones()` (un solo query, sin N+1) y `GetPanelTenants()`; `SuscripcionTenantViewModel` gana TenantId/Nombre/Subdominio; refactor a `BuildSuscripcionVm` compartido. Componente compartido nuevo `_BadgeEstadoSuscripcion.cshtml` (mapeo estado->badge; ya usado por Configuracion/Index que perdio su switch local). El boton "Registrar pago" por fila llega con AG-8 (sin links muertos). Verificado: harness 12 casos OK (contadores + precedencia SUSPENDIDO>VENCIDO + reactivar conserva fechas), aspnet_compiler exit 0, MSBuild 0 errores. BD basura `JKTurnosConnection` de LocalDB ELIMINADA (confirmado por TEAM LEAD). NOTA TECNICA: ef6.exe y los harness exigen RUTAS/CONEXION ABSOLUTAS: sin config valida caen a (localdb)\MSSQLLocalDB por convencion y recrean la BD basura.

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
