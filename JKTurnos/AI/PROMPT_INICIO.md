# PROMPT DE INICIO - JKTurnos

Eres un agente de desarrollo del proyecto JKTurnos, un SaaS de gestion de turnos
(ASP.NET MVC 5, .NET Framework 4.7, EF6, SQL Server Express, OWIN Cookie Auth,
Repository Pattern, multi-tenant por TenantId).

Root del proyecto: C:\Users\Gonzo\Desktop\JKTurnos\JKTurnos\JKTurnos\JKTurnos

=====================================================
ARRANQUE (lectura minima obligatoria, siempre)
=====================================================
Lee en orden estos 2 archivos. Con ellos tenes contexto actualizado al minimo costo:
1. AI\00_MEMORIA.md        -> indice, stack, arquitectura, plan activo, rutas de lectura por tarea
2. AI\01_ESTADO.md         -> estado real por modulo (que esta CONSOLIDADO y que falta)

No leas mas archivos de arranque. Los SESION_*.md son resumenes ya consolidados
en 01_ESTADO/02_DECISIONES/03_PROBLEMAS: solo leerlos si la tarea lo pide.

=====================================================
RUTAS DE LECTURA POR TIPO DE TAREA
=====================================================
Despues del arranque, lee segun corresponda:

- MODULO NUEVO / CRUD nuevo:
    AI\02_DECISIONES.md        (reglas de arquitectura D-001 a D-021)
    AI\03_PROBLEMAS.md         (problemas resueltos: patrones a no repetir)
    Docs\01_AGENTS.md          (reglas del proyecto y convenciones)
    Docs\JKTurnosDictionary.md (patron CRUD oficial, secciones 3 y 6, flujo de componentes seccion 4)
    Docs\DashboardLayoutSystem.md (sistema visual y reglas de composicion)
    Docs\03_ARCHITECTURE.md    (stack y arquitectura general)
    Docs\05_DESIGN_SYSTEM.md   (design system / identidad visual)

- FRONTEND / UI / diseno de pantalla:
    Docs\DashboardLayoutSystem.md  (sistema visual y reglas de composicion)
    Docs\05_DESIGN_SYSTEM.md       (design system, paleta, tipografia)
    Docs\06_MIGRACION_DS.md        (mapa de clases v1->v2, reglas del DS)
    Docs\JKTurnosDictionary.md     (componentes existentes y slots ViewBag, secciones 3.1-3.20)
    Content\DesignSystem\jk-tokens.css, jk-components.css  (verificar clases que ya existen)

- MODULO TURNOS / AGENDA:
    Docs\11_ARQUITECTURA_FUNCIONAL.md (flujo funcional de Agenda y Turnos)
    Docs\13_AGENDA_UX.md              (manual de UX de la Agenda)
    Docs\14_TURNO_LIFECYCLE.md        (ciclo de vida del turno y reglas)
    Docs\15_TURNO_CRUD_TECHNICAL_SUMMARY.md (resumen tecnico del modulo Turnos)
    Docs\16_DRAWER_TURNO_FUTURO.md    (Drawer V2/V3 y agenda movil)

- MODELO DE DATOS / ENTIDADES / MIGRACIONES:
    Docs\13_DER.md          (modelo entidad-relacion oficial)
    Docs\12_CORE_DOMAIN_MODEL.md (modelo conceptual del nucleo)
    AI\02_DECISIONES.md     (decisiones que afectan el schema, ej. D-013/D-014)

- NEGOCIO / REGLAS / PRODUCTO / UX:
    Docs\04_BUSINESS_RULES.md (reglas de negocio)
    Docs\00_VISION.md         (vision y filosofia del producto)
    Docs\13_AGENDA_UX.md      (UX de la Agenda)

- ROADMAP / ESTADO / PRIORIDADES:
    Docs\02_ROADMAP.md        (roadmap por fases)

- BUG / DIAGNOSTICO:
    AI\03_PROBLEMAS.md     (completo)
    AI\01_ESTADO.md        (estado del modulo afectado)
    AI\02_DECISIONES.md    (decisiones que afectan al modulo)

- DEPLOY / PRODUCCION / CONEXION BD:
    Docs\DEPLOYMENT_NOTES.md  (configuracion de publicacion y BD)
    Docs\ENVIRONMENT_TOGGLE.md (cambio local <-> produccion)

- AUTORIZACION / ROLES / TENANTS:
    AI\SESION_20260814_PLAN_ROLES.md (plan de Roles/Cuentas/Tenants, D-022/D-023, CONSOLIDADO)
    AI\SESION_20260826_PLAN_SUSCRIPCIONES.md (plan AG Suscripciones/Pagos/Admin Global, D-037/D-038, aprobado NO implementado)

=====================================================
REGLAS OBLIGATORIAS (aplicar siempre)
=====================================================
1. Respetar el flujo Controller -> Service -> Repository -> EF. Nunca acceder a BD desde un Controller.
2. Todo CRUD usa BaseRepository (filtra TenantId + IsActive). Soft delete obligatorio.
3. NO usar SQL directo salvo necesidad acotada y encapsulada en Repository.
4. Textos visibles SIN ACENTOS (compatibilidad de encoding).
5. CSS: usar siempre variables del DS (--jk-*). No hardcodear colores ni estilos inline.
6. Reutilizar componentes de Views\Shared\Components\ (no duplicar HTML, no crear componentes sin necesidad real).
7. Los color del Time Block representa el Recurso, NUNCA el estado.
8. Estados operativos y economicos son independientes (no mezclarlos).
9. Un Turno siempre tiene Recurso obligatorio (D-001); Servicio opcional (D-014).
10. El Home es Command Center, la Agenda es el centro del sistema (no crear "Listado de Turnos").
11. Los Controllers solo coordinan; las reglas viven en Services; las vistas solo renderizan.
12. Al terminar una tarea: compilar (MSBuild Debug) y verificar 0 errores.
13. Si hay duda de arquitectura: NO decidir; detenerse, explicar el problema y proponer alternativas.
14. No auto-consolidar etapas: solo el TEAM LEAD confirma CONSOLIDADO. No modificar AI\01_ESTADO.md sin confirmacion.