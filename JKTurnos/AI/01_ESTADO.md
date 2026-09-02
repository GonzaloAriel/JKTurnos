FORMATO: 1.0

## Regla de estados
CONSOLIDADO - probado y confirmado explicitamente por el TEAM LEAD como terminado, aprobado y funcionando.

EN PROGRESO - trabajo activo, aun no terminado ni confirmado.

PENDIENTE - No evaluado.

## Estado actual
- Vinculacion Recurso-Servicio (N:M, checkbox bidireccional + drawers de quick-create en Recursos y Servicios): CONSOLIDADO (2026-08-12, aprobado por TEAM LEAD).
- Quick Create en Turnos/Create y Edit (drawers de Servicio y Recurso en modo select; Servicio opcional; filtro de recursos unificado): CONSOLIDADO (2026-08-12, aprobado por TEAM LEAD).
- Drawer de detalle del Time Block (V1 solo lectura; seccion Pagos/Seña SIEMPRE visible con placeholder "Sin senia" hasta V3): CONSOLIDADO (2026-08-13, aprobado por TEAM LEAD). V2 y V3 a futuro en Docs/16_DRAWER_TURNO_FUTURO.md.
- Agenda: EN PROGRESO. P-003 CERRADO por TEAM LEAD (2026-08-13). Vista movil P1 (acordeon por Recurso + horarios libres + filtro de recursos conectado, D-019): CONSOLIDADO por TEAM LEAD (2026-08-13). P2 FAB "Nuevo Turno" (D-017): ELIMINADO por TEAM LEAD (2026-08-13), sin relevancia por ahora; el boton "Nuevo Turno" de la Agenda navega a /Turnos/Create. Filtro de recursos en la grilla desktop (2026-08-19): CONSOLIDADO por TEAM LEAD. Desmarcar un recurso colapsa su columna en la vista Dia (_AgendaGrid) y oculta sus Time Blocks tambien en la vista Semana (_AgendaGridSemanal); el filtro es visible en desktop y el contador refleja los recursos visibles. M-07 Agenda Drag & Drop + Mover Turno (D-040, revisar AI/PLAN_MEJORAS.md): CONSOLIDADO por TEAM LEAD (2026-09-01). Backend `TurnoService.Mover` (recalcula fin con `CalcularFin`, valida disponibilidad excluyendo el propio turno, solo mueve Pendiente/En curso, rechaza mover al pasado) + `TurnosController.Mover` (POST ajax + anti-forgery). Drag & Drop HTML5 (native) en _AgendaGrid (snap 30 min por celda) y _AgendaGridSemanal (snap 5 min por posicion, conserva recurso); solo bloques movibles son draggable=true. `jk-agenda.js`: shift+dragover/drop, flag `dragJustEnded` suprime el click sintetico del drop, `body.jk-dragging` desactiva pointer-events durante el arrastre. `_TurnoDetailDrawer`: boton "Mover turno" con formulario inline (recurso/fecha/hora) que POSTea a Mover (alternativa mobile), reemplaza el link "Re agendar". `TurnoDetailViewModel` +RecursoId/+Recursos. Refresco suave sin F5: `TurnosController.GetAgendaGrid` devuelve el partial de grilla por AJAX; `Agenda.cshtml` agrega `data-agenda-content-url` y envuelve la grilla en `<div id="jk-agenda-grid-container">`; `jk-agenda.js.refreshAgendaGrid()` reemplaza solo el inner del wrapper via DOMParser (conservando scroll + listeners de teclado). Al mover por drag y por "Mover turno" ya no hay parpadeo F5 ni salto al inicio; el bloque aparece en su nueva posicion y el drawer sigue abriendo. MSBuild Debug 0 errores; node --check OK.
- Home / Command Center: CONSOLIDADO (2026-08-14, aprobado por TEAM LEAD). Saludo contextual real; acciones rapidas conectadas (Turnos/Create, Turnos/Agenda, Clientes/Create); "Mi dia" con datos reales en carriles por Recurso (chips compactos, D-020) + empty state con CTA; KPIs conectados (Turnos Hoy, Ingresos = proyeccion del dia D-021, Ocupacion, Clientes). Pendiente (placeholders): sugerencias y actividad reciente (dependen del modulo de configuracion) y el filtro de grilla de Agenda.
- Modulo Roles/Cuentas/Tenants (plan AI/SESION_20260814_PLAN_ROLES.md, Fases 0-2): CONSOLIDADO (2026-08-18, aprobado por TEAM LEAD). Fase 0: `Roles` (constantes), `GlobalRepository` (D-022), seed `global@jkturnos.com` (AdminGlobal, TenantId=0), helpers `CurrentRole/IsAdminGlobal/IsAdmin` en `BaseController` (D-023). Fase 1: `UserService` y `TenantService` (scope por claims, hasheo server-side, subdominio unico, alta opcional del 1er Admin). Fase 2: `UsuarioViewModel`, `TenantsController`, `UsuariosController`, vistas Index/Create/Edit/Delete de ambos + sidebar (grupo Sistema solo AdminGlobal; item Usuarios en Gestion para Admin).
- Reparacion de BD del modulo Roles (2026-08-18, aprobado por TEAM LEAD): corregido `__MigrationHistory` (ContextKey -> `JKTurnos.Migrations.Configuration` + Model de la ultima migracion regenerado), eliminado el FK `Users.TenantId -> Tenants` para permitir `TenantId=0`, y creado `global@jkturnos.com` en la BD. `Update-Database` vuelve a ser consistente (verificado con `ef6.exe database update --script`).
- Alta de tenant operativo + fixes (2026-08-18, aprobado por TEAM LEAD): `TenantService.Create` siembra Estados de Turno base + `ConfiguracionTenant` (D-024); guard defensivo en `TurnoService.Crear` cuando no hay estados; creada `Views/Recursos/Delete.cshtml` (faltaba la vista).
- Modulo Configuracion de Negocio (D-025, 2026-08-18, aprobado por TEAM LEAD): `ConfiguracionService` + `ConfiguracionController` (solo rol Admin) con Datos del negocio (edicion de ConfiguracionTenant con ayuda para logo gratis por URL) y CRUD de Estados de Turno (nombre + color, baja logica). Vistas `Views/Configuracion/{Index,DatosNegocio,EstadoCrear,EstadoEditar,EstadoEliminar}.cshtml`. Sidebar item "Negocio" y accion rapida "Configurar" del Command Center apuntan al modulo.
- Logo del negocio por URL (D-026, 2026-08-18, aprobado por TEAM LEAD): `BaseController.OnActionExecuting` expone `ViewBag.ConfiguracionLogo/Nombre` (un query por request); el sidebar muestra el logo (100px, fallback al icono) y la bienvenida del Command Center lo muestra arriba del saludo (120px, sin recuadro, centrado en movil). Se removio el nombre comercial de la bienvenida (redundante).
- Deploy con auto-migracion (D-027, 2026-08-18, aprobado por TEAM LEAD): cadena de migraciones reescrita en una unica `Inicial` (modelo actual completo, incluye ServicioId nullable, sin FK Users.TenantId, Turno->Recurso sin cascade via OnModelCreating). `Global.asax.cs` usa `MigrateDatabaseToLatestVersion` (la app actualiza sola la BD al arrancar: migraciones pendientes + Seed). BD local regenerada desde cero y verificada (admin/global/tenant/config/estados correctos). El hosting se vacia UNA vez (borrar tablas) y luego cada deploy = Add-Migration + publicar.
- Modulo Notificaciones al Cliente (D-033): CONSOLIDADO (2026-08-19, aprobado por TEAM LEAD). Fase 1 CONSOLIDADA: entidad `NotificacionService` (genera el mensaje desde la plantilla con placeholders {nombre}/{servicio}/{fecha}/{hora} y el link `wa.me` con el telefono del cliente normalizado), boton "Recordar por WhatsApp" en `_TurnoDetailDrawer` (abre WhatsApp en otra pestana con el mensaje prellenado; se oculta si el cliente no tiene telefono), plantilla editable por rol Admin en Configuracion (campo `ConfiguracionTenant.WhatsAppTemplate`, form DatosNegocio + card Notificaciones en Index), migracion `AddNotificacionesWhatsApp` aplicada a la BD local. Fase 2 email SMTP CONSOLIDADA (implementada 2026-08-19; aprobado por TEAM LEAD, 2026-08-25): `NotificacionService.EnviarEmail(turnoId)` con asunto/cuerpo desde plantillas editables (EmailAsunto/EmailTemplate), credenciales SMTP en `ConfiguracionTenant` (Servidor, Puerto, Usuario, Password, EmailDesde) + switch `SmtpHabilitado`, accion `Turnos/EnviarRecordatorioEmail` (POST ajax) y boton "Enviar recordatorio por email" en el Drawer (solo si el cliente tiene email; handler global en `jk-layout.js` para Agenda/Pagos/Dashboard); la password no se re-renderiza en el form y en blanco conserva la actual; migracion `AddNotificacionesEmail` aplicada y verificada en la BD local. Seccion propia en Configuracion (2026-08-19): la seccion Notificaciones NO tiene item en el sidebar (decisión del TEAM LEAD: en el grupo Configuracion solo queda "Negocio"); se accede desde la card "Configuracion de notificaciones" del `Configuracion/Index` (link "Ver seccion" y lapices). La vista `Notificaciones.cshtml` tiene las cards "Recordatorio por WhatsApp" y "Recordatorio por email (SMTP)" con boton Editar. Las ediciones estan SEPARADAS: `NotificacionesWhatsApp.cshtml` (solo plantilla WhatsApp) y `NotificacionesEmail.cshtml` (solo SMTP), con acciones y metodos de servicio propios (`UpdateNotificacionWhatsApp`/`UpdateNotificacionEmail`). `ConfiguracionService.UpdateNotificacionWhatsApp` y `UpdateNotificacionEmail` son independentes y actualizan SOLO sus campos. Migracion `AddNotificacionesEmail` aplicada. MSBuild Debug 0 errores.
- Dashboard / Command Center v2 (D-034): CONSOLIDADO (2026-08-19, aprobado por TEAM LEAD), SECCION APARTE (el TEAM LEAD decidio que no se toca el Command Center). Fase 1 CONSOLIDADA: `DashboardController` + `Views/Dashboard/Index.cshtml` + `DashboardService` + `DashboardViewModel` + `Content/Pages/jk-dashboard-section.css`; sidebar item "Dashboard" en grupo Principal. Muestra KPIs del mes (turnos, ingresos por pagos, ocupacion de hoy, clientes), graficas de ocupacion semanal e ingresos del mes por dia (CSS puro), proximos turnos y turnos sin confirmar (click abre el drawer de detalle en la MISMA seccion Dashboard via `jk-dashboard.js`, sin navegar a la Agenda; incluye registrar pago en el drawer), ranking de servicios, cobrado por recurso y saldo pendiente. Fase 2 CONSOLIDADA (2026-08-19, aprobado por TEAM LEAD): KPI de ingresos con variacion % vs mes anterior, card "Evolucion de ingresos (12 meses)" (barras verticales), card "Ingresos por recurso (mes)" con barras horizontales con %, card "Pagos del mes por tipo" (desglose Senia/PagoParcial/PagoTotal con montos y cantidades). Fase 3 exportacion a futuro.
- Evolucion visual (D-035): CONSOLIDADO (2026-08-25, aprobado por TEAM LEAD tras smoke test por posta). Archivo fuente unico de la identidad: `Content/DesignSystem/jk-tokens.css`. Postas verificadas: (1) paleta calida crema/marfil/beige + sage accent `#5F7A5D` cambiando SOLO valores de tokens existentes (sin taxonomia nueva); (1.B) aliases legacy unificados a la nueva paleta (`--jk-primary/-hover/-bg-app/-text-main/-text-muted` declarados en jk-tokens.css) + botones primario/danger/success core migrados a tokens (WhatsApp intacto como marca externa); (2) tipografia: titulos en Nunito via nuevo token `--jk-font-display` (@import en jk-tokens.css), cuerpo sigue Inter; (3) radios amables 8/14/18/22px (solo tokens); (4) sombras calidas tinte marron `rgba(74,64,56,...)` + anillos de foco migrados del azul viejo a `color-mix(var(--jk-accent))`; (5) componentes tanda 1 (badges e iconos de notificacion con texto oscurecido via color-mix, AA cumplido; `:active` de botones) y tanda 2 (KPI trends y toasts AA; cards ya heredaban todo, sin cambios); (6) espaciado dirigido: separacion minima entre notificaciones consecutivas (campana y Actividad reciente) via `.jk-notif-item + .jk-notif-item`. Deuda registrada (NO corregir sin decision): hardcodes de `Content/Pages/`, fallbacks `#2563eb`/`#aeaeb2` de Recurso/Estado en Views (son datos, no UI), `_ColorSelector.cshtml` paleta Apple, backdrops negros, CSS muerto (`jk-ds.css`, `jk-sidebar.css`, `jk-topbar.css`), SVG select arrow gris.
- Evolucion visual segunda ola (D-036 + ajustes aprobados): CONSOLIDADO (2026-08-25, aprobado por TEAM LEAD tras verificacion visual por posta). Incluye: (1) PALETA VINTAGE DE RECURSOS en `_ColorSelector.cshtml` manteniendo nombres: Azul `#7295B5`, Verde `#9CAF88`, Rojo `#C97B6D`, Naranja `#E8A87C`, Amarillo `#E6C868`, Violeta `#A594C5`, Rosa `#D9A5B3`, Turquesa `#6FB3B8`, Gris `#A89F91`, Marron `#B08968`, Indigo `#8784B8`, Cian `#92C7AF`; default y fallbacks de Recurso en Views cambiados de `#2563eb` a `#7295B5` (los recursos ya creados conservan su color guardado hasta editarse). (2) DRAWER: redondeo solo lado izquierdo (`--jk-radius-lg 0 0 var(--jk-radius-lg)`), backdrops calidos `rgba(74,64,56,.40)` drawer y `.45` modal, header con hairline `--jk-border-light`, close circular, seccion economica como mini-card `.jk-drawer-section-economica`. (3) COMMAND CENTER alineado: residuos frios de `Content/jk-dashboard.css` migrados a tokens (`#f0f5ff`->accent-light, `#ecfdf5`->success-light, `#fffbeb`->warning-light, `#d1d5db`->border, blancos+radius hardcodeados->surface/tokens en quick-actions, mi-dia y suggestion cards), KPIs con Nunito (`.kpi-value`), fecha en el saludo como eyebrow (`.contextual-date`), separadores punteados entre secciones. (4) SUPERFICIES Y NAV EN MENTA PALIDA: `--jk-surface #F1FAFA`, `--jk-sidebar-bg #D7F0EF` (fondo de pagina sigue crema). (5) LOGIN RENOVADO: gradiente marron->sage reemplazado por menta->crema con blobs pastel vivos, logo en chip flotante, firma tricolor bajo el titulo, features en chips circulares de color. (6) LAVADO PASTEL GLOBAL: `background-image` con halos fijos al viewport en `.jk-app-shell` y `.jk-login`; `.welcome-section` transparente (sin blobs locales que dupliquen). Deuda menor registrada: los Estados de Turno base siembran colores Bootstrap (D-024), ajenos a la paleta vintage.
- Plan AG - Suscripciones, Pagos de Plataforma y Admin Global (D-037/D-038; plan AI/SESION_20260826_PLAN_SUSCRIPCIONES.md): EN PROGRESO. AG-0 CONGELADA. AG-1..AG-5, AG-7 IMPLEMENTADAS (pendiente confirmacion TEAM LEAD). AG-6 EXCLUIDA (D-038). AG-8 IMPLEMENTADA (2026-08-26): PagosSuscripcionController con Index (filtros tenant/rango fechas) y Registrar (model binding via RegistrarPagoViewModel). AG-9 IMPLEMENTADA (2026-08-26): Dashboard Admin Global minimo (shell con contadores de tenants por estado). AG-10 IMPLEMENTADA (2026-08-26): Avatares de Plataforma — entidad AvatarPlataforma (global, BaseModel), CRUD AdminGlobal (AvataresController + 4 vistas), selector en Configuracion/DatosNegocio, avatar priorizado en BaseController (AvatarUrl > LogoUrl > icono), sidebar "Avatares" en grupo Sistema. Sidebar branding: texto "JKTurnos" eliminado, subtitulo muestra NombreComercial, logo centrado, collapsed reduce logo a 38x38. Command Center: "Configura horarios" reemplazado por "Crear recurso". Ciclo vida Tenant: DELETE cascade soft-delete (Users, ConfiguracionTenant, EstadosTurno), Reactivar (restaura Tenant + Users), Desactivar con pantalla de confirmacion, login bloqueado para tenants inactivos, filtros de busqueda y estado en grilla. MSBuild Debug 0 errores. Proximo: confirmacion TEAM LEAD.
- D-040 (Estados predefinidos, refactor 2026-08-28, pendiente confirmacion TEAM LEAD): `EstadosTurnoBase.cs` define catalogo fijo PENDIENTE/EN_CURSO/FINALIZADO/CANCELADO con Codigo estable. `EstadoTurno.cs` agrega columna Codigo (via migracion `AddCatalogoEstados`). `TurnoEstadoMotor.cs` centraliza matriz de transiciones por Codigo. `SituacionTemporal.cs` usa Codigo para calculo. Seed de TenantService siembra los 4 estados base con Codigo. Ultimos 3 filtros residuales `EstadoTurno.Nombre != "Cancelados"` corregidos a `Codigo != EstadosTurnoBase.Cancelado` (DashboardService, RecordatorioService). Compila MSBuild Debug 0 errores. Pendiente: eliminar CRUD de Estados de Turno de Configuracion (parcialmente SUPERADO por D-025).
- D-041 (Agenda Quick Create Drawer v2, 2026-08-28, pendiente confirmacion TEAM LEAD): al abrir desde una celda de recurso, el Recurso se preselecciona Y el dropdown de Servicios se filtra a los vinculados (GetServiciosPorRecurso). Si recurso tiene 1 servicio vinculado, se preselecciona automaticamente. Cambio de Recurso re-filtra Servicios; cambio de Servicio preserva Recurso si sigue valido (data-prev-value). `jk-agenda.js` modificado (openQuickCreate + handlers), `Agenda.cshtml` agrega `data-servicios-por-recurso-url`. Verificado con node --check y 9 checks logicos. Sin cambios en modelo de datos ni servidor. Reduce flujo de 3 selecciones a 1-2.
- VIDRIERA PUBLICA M-13 (Postas 0 a 5, 2026-09-02, aprobadas por TEAM LEAD; plan AI/CONTRATO_VIDRIERA_M13.md + AI/DISENO_VIDRIERA_M13_POSTA3.md; decisiones D-042..D-045): CONSOLIDADO. Modulo: vista publica `/vidriera/{slug}` (AllowAnonymous) read-only y aislada por TenantId del tenant resuelto por slug + panel de gestion. Resumen por posta:
  - Posta 0-2 (modelo): entidad `VidrieraPublica` (1:1 Tenant, Slug unico global, Habilitada independiente del contenido, Titulo/Descripcion/MensajePrincipal/OfertaTitulo/OfertaDescripcion + flags MostrarServicios/MostrarPrecios/MostrarDisponibilidad/MostrarOfertas/MostrarMensaje). Tabla creada por AUTO-MIGRACION de EF6 al arranque (D-043), NO migracion explicita (el mecanismo Add-Migration esta fuera de alcance; fabricar el snapshot EDMX a mano fue lo que rompio RemovePortalHabilitado).
  - Posta 3 (3A+3B): `VidrieraService.GetVidrieraPublica(slug)` consulta JKContext directo (NO BaseRepository/GlobalRepository) filtrado por TenantId desde el slug; identidad desde ConfiguracionTenant, servicios activos del MISMO tenant, reglas de 404 (slug inexistente / Habilitada=false / tenant inactivo → HttpNotFound). Layout publico `_VidrieraLayout.cshtml` (standalone, sin sidebar/login).
  - Posta 4 (disponibilidad real): `GetAvailableSlots(tenantId)` por RECURSO y SOLO HOY (jornada 08:00-19:30, D-012), franjas de 30 min; regla de ocupacion IDENTICA a `TurnoService.ValidarDisponibilidad` (turno NO Cancelado ocupa; Cancelado libera). Vista renderiza seccion solo si `MostrarDisponibilidad`; estado vacio "Sin turnos disponibles hoy" siempre presente.
  - Posta 5 (panel admin): `VidrieraAdminController` ([Authorize(Admin)], Index GET + Guardar POST), `VidrieraAdminViewModel` (Slug/owsRequired 100 centenares + flags + YaExiste + UrlPublica), `VidrieraService` admin methods (GetVidrieraAdmin/GuardarVidriera/NormalizarSlug/GetUrlPublica/MapAdmin). Upsert por TenantId; slug editable UNICO GLOBAL (rechaza el ya usado por OTRO tenant con "Ese slug ya esta en uso. Elegi otro."); publish/unpublish (Habilitada=false → 404 publico); auditoria `E_Vidriera` (Crear/Editar); sidebar item "Vidriera" (bi-globe2, grupo Configuracion). Como crear/guardar la fila desde el panel. Fix quirk EF6 (TargetException) resolviendo `actualId` a variable local antes del query.
  - Extension a la vista publica (2026-09-02): `VidrieraPublicaViewModel` ahora expone Titulo/Descripcion/MensajePrincipal/OfertaTitulo/OfertaDescripcion + flags MostrarServicios/MostrarPrecios/MostrarOfertas/MostrarMensaje; `GetVidrieraPublica` los mapea. La vista renderiza banner de Bienvenida (MensajePrincipal, flag MostrarMensaje), seccion Oferta (OfertaTitulo/Descripcion, bandera MostrarOfertas) y gatea Servicios con MostrarServicios. Antes la vista solo renderizaba identidad+servicios+disponibilidad (los datos de mensaje/oferta se guardaban desde el panel pero no se desplegaban). Build 0 errores, verificado runtime (/vidriera/admin con MensajePrincipal "Con la mejor atencion!!" y OfertaTitulo "2x1 en corte" renderizados).
  - REDISENO MONITOR (2026-09-02, aprobada por TEAM LEAD): la vista publica paso de una unica columna apiladas a un layout TIPO MONITOR/TV de dos columnas que entra en una pantalla LED de 17" (min 1024x768, verified via CDP sin scroll page: `docScrollH==clientH`, lists caben sin clip interno). `jk-vidriera.css` reescrito: `.jk-vidriera` como `height:100vh` + `overflow:hidden` (grid `1.35fr 1fr`; left Servicios, right Oferta + Disponibilidad), header con marca circular + nombre (Nunito display) + titulo + descripcion + contacto en chips, banner de bienvenida con gradiente tricolor, tarjeta de oferta con borde/accent ambar, slots de disponibilidad en teal, blob decorativo de marca, listas con scroll interno que nunca rompen el marco fijo. Responsive a columna simple <840px.
  - LOGO/AVATAR (2026-09-02): la marca de la vidriera resuelve en `GetVidrieraPublica` con la MISMA precedencia que `BaseController.VMbag.ConfiguracionLogo` (D-026/D-036): LogoUrl propio > AvatarUrl precargado > fallback marca circular con inicial sobre gradiente tricolor. Antes solo se mapeaba LogoUrl (ignoraba AvatarUrl). Verificado runtime en 3 casos (logo / solo avatar / ninguno).
  - Deudas/notas: textos visibles sin acentos en el panel (encabezados/vista), persistencia de datos de prueba limpiada (filas VidrieraPublicas en 0, tenant 3 sin logo/avatar), IIS y procesos detenidos. Pendientes M-13 Postas 6 (refresh periodico), 7 (modo monitor interactivo) y 8 (seguridad/cache/aislamiento formal + vista "no disponible" opcional).

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
