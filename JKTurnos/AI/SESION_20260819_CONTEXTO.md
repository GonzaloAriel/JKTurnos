# SESION 2026-08-19: FILTRO GRILLA, NOTIFICACIONES FASE 1, DASHBOARD SECCION APARTE

## Objetivo
1. Filtro de recursos aplicado a la grilla desktop de la Agenda.
2. Modulo Notificaciones al Cliente (D-033), Fase 1: WhatsApp via link `wa.me` con plantilla editable.
3. Modulo Dashboard (D-034) en SECCION APARTE (nada en el Command Center), con drawer de turno sin salir de la seccion.

## Estado
- Filtro de recursos en grilla desktop: CONSOLIDADO (aprobado por TEAM LEAD, 2026-08-19).
- Notificaciones Fase 1 (D-033): CONSOLIDADA (aprobado por TEAM LEAD, 2026-08-19). Migracion `AddNotificacionesWhatsApp` aplicada a la BD local.
- Notificaciones Fase 2 (D-033, email SMTP): implementada 2026-08-19, pendiente de confirmacion del TEAM LEAD. Migracion `AddNotificacionesEmail` aplicada y verificada en la BD local.
- Dashboard seccion aparte (D-034): Fase 1 CONSOLIDADA (aprobado por TEAM LEAD, 2026-08-19). Fase 2 (pagos: variacion mensual, evolucion 12 meses, ingresos por recurso con %, pagos por tipo) CONSOLIDADA (aprobado por TEAM LEAD, 2026-08-19).
- Modulo Pagos (D-030/D-031/D-032) y encoding UTF-8: ya registrados en la memoria (01_ESTADO/02_DECISIONES), de la sesion previa del dia.

## Cambios

### Filtro de recursos en la grilla desktop
- `Views/Shared/Components/_AgendaGrid.cshtml`: `data-recurso-id` en headers, celdas y time blocks.
- `Views/Shared/Components/_AgendaGridSemanal.cshtml`: `data-recurso-id` en los bloques semanales.
- `Scripts/jk-agenda.js`: `applyDesktopResourceFilter()` — al desmarcar un recurso colapsa su columna en la vista Dia (recalcula `grid-column` y `--jk-resource-count`) y oculta sus bloques en la vista Semana (clase `is-filtered`). El contador del filtro refleja los recursos visibles.
- `Content/Pages/jk-agenda.css`: el filtro ahora es visible en desktop (`display: flex` base) + regla `.is-filtered { display:none }`.

### Modulo Notificaciones al Cliente (D-033, Fase 1)
- `Models/Entities/ConfiguracionTenant.cs`: campo `WhatsAppTemplate` (StringLength 500).
- `Services/NotificacionService.cs` (nuevo): `GenerarLinkWhatsApp(turno)` (link `wa.me` con telefono del cliente normalizado a digitos, null sin telefono) y `GenerarMensaje(turno)` (plantilla con placeholders {nombre}/{servicio}/{fecha}/{hora}, fallback a `DefaultTemplate`).
- `Controllers/TurnosController.cs` (`GetDetailDrawer`): puebla `WhatsAppLink` del modelo.
- `Models/ViewModels/TurnoDetailViewModel.cs`: propiedad `WhatsAppLink`.
- `Views/Shared/Components/_TurnoDetailDrawer.cshtml`: boton "Recordar por WhatsApp" (`jk-btn-whatsapp`, target=_blank) en la seccion fecha/hora, oculto si no hay telefono.
- `Views/Configuracion/DatosNegocio.cshtml`: textarea para la plantilla con ayuda de placeholders. `Views/Configuracion/Index.cshtml`: card "Notificaciones al cliente" con la plantilla configurada.
- `Services/ConfiguracionService.cs`: `UpdateConfiguracion` copia `WhatsAppTemplate`.
- `Migrations/Configuration.cs` (Seed) y `Services/TenantService.cs`: siembran `NotificacionService.DefaultTemplate`.
- `Content/DesignSystem/jk-components.css`: variante `.jk-btn-whatsapp` (#25D366).
- Migracion `AddNotificacionesWhatsApp` (columna `dbo.ConfiguracionTenants.WhatsAppTemplate nvarchar(500)`), generada con ef6.exe (`migrations add` con `--config` + `--connection-string-name JKTurnosConnection`) y aplicada con `database update`. Registrada en csproj.

### Modulo Notificaciones al Cliente (D-033, Fase 2: email SMTP)
- `Models/Entities/ConfiguracionTenant.cs`: `EmailAsunto`, `EmailTemplate`, `EmailDesde`, `SmtpServidor`, `SmtpPuerto`, `SmtpUsuario`, `SmtpPassword`, `SmtpHabilitado`.
- `Services/NotificacionService.cs`: refactor a `RellenarPlantilla` (reutilizada por WhatsApp y email); `GenerarAsuntoEmail`, `GenerarCuerpoEmail`, `EnviarEmail(turnoId)` (valida config/email, `SmtpClient` con puerto 587 por defecto, TLS, credenciales; mensajes claros via ArgumentException/InvalidOperationException).
- `Controllers/TurnosController.cs`: accion `EnviarRecordatorioEmail` (POST ajax + antiforgery, devuelve Json success/message); `GetDetailDrawer` puebla `PuedeEnviarEmail`.
- `Models/ViewModels/TurnoDetailViewModel.cs`: propiedad `PuedeEnviarEmail`.
- `Views/Shared/Components/_TurnoDetailDrawer.cshtml`: `@Html.AntiForgeryToken()` al inicio del partial + boton "Enviar recordatorio por email" (`data-enviar-email`, solo si el cliente tiene email).
- `Scripts/jk-layout.js`: handler global `[data-enviar-email]` (delegacion en document, postea token+turnoId, toast success/error, rehabilita boton) — cubre Agenda/Pagos/Dashboard sin duplicar.
- `Views/Configuracion/DatosNegocio.cshtml`: seccion SMTP (switch habilitar, remitente, servidor, puerto, usuario, password, asunto y cuerpo editables). `Views/Configuracion/Index.cshtml`: card Notificaciones muestra estado del email (habilitado/servidor/desde).
- `Services/ConfiguracionService.cs`: copia los nuevos campos; password en blanco conserva la actual.
- `Controllers/ConfiguracionController.cs`: `DatosNegocio` GET nullea `SmtpPassword` (nunca se re-renderiza la almacenada).
- `Migrations/Configuration.cs` (Seed) y `Services/TenantService.cs`: siembran `EmailAsunto`/`EmailTemplate` default + `SmtpHabilitado=false`.
- Migracion `AddNotificacionesEmail` (8 columnas, aplicada con ef6 y verificada en BD local). Registrada en csproj.

### Reorganizacion: seccion Notificaciones propia en Configuracion
- `Views/Configuracion/Notificaciones.cshtml` (nuevo): seccion con cards "Recordatorio por WhatsApp" y "Recordatorio por email (SMTP)" (estado + boton Editar).
- `Views/Configuracion/NotificacionesWhatsApp.cshtml` (nuevo): form SOLO de la plantilla de WhatsApp (via `_FormLayout`). `Views/Configuracion/NotificacionesEmail.cshtml` (nuevo): form SOLO de SMTP (switch, remitente, servidor, puerto, usuario, password, asunto, cuerpo).
- `Views/Configuracion/NotificacionesEditar.cshtml`: ELIMINADA (las ediciones quedaron separadas por canal).
- `Views/Configuracion/Index.cshtml`: nueva card "Configuracion de notificaciones" con filas WhatsApp y email (estado + lapiz propio a cada edicion) y link "Ver seccion"; se elimino la card "Notificaciones al cliente".
- `Views/Configuracion/DatosNegocio.cshtml`: se eliminan las secciones WhatsApp/SMTP (queda solo datos del negocio).
- `Views/Shared/_Sidebar.cshtml`: NO se agrego item al sidebar (decision del TEAM LEAD: en el grupo Configuracion solo queda "Negocio"); la seccion Notificaciones se accede desde la card "Configuracion de notificaciones" del Index.
- `Services/ConfiguracionService.cs`: `UpdateConfiguracion` se divide en `UpdateDatosNegocio`, `UpdateNotificacionWhatsApp` y `UpdateNotificacionEmail` (cada forma solo toca sus campos; la password en blanco conserva la actual).
- `Controllers/ConfiguracionController.cs`: acciones `Notificaciones` (GET), `NotificacionesWhatsApp` (GET/POST) y `NotificacionesEmail` (GET/POST); `DatosNegocio` POST usa `UpdateDatosNegocio`.

### Modulo Dashboard (D-034, seccion aparte)
- `Models/ViewModels/DashboardViewModel.cs` (nuevo): KPIs del mes, `OcupacionSemana`/`IngresosMesPorDia`/`IngresosUltimosMeses` (BarraGrafico), `ProximosTurnos`/`TurnosPendientes` (DashboardTurnoItem), `RankingServicios`/`IngresosPorRecurso` (RankingItem), `PagosPorTipo` (PagosPorTipoItem), `SaldoPendiente`/`TurnosConSaldoPendiente`, `IngresosMesAnterior`/`VariacionMensual`.
- `Services/DashboardService.cs` (nuevo): `GetDashboard(hoy)` — turnos/ingresos del mes, ocupacion del dia (690 min, D-012), ocupacion semanal (Lun-Dom), ingresos del mes por dia, proximos 6 turnos y pendientes (14 dias), ranking de servicios, saldo pendiente (precio - acumulado de pagos de turnos activos del mes). Fase 2: `EvolucionIngresos` (12 meses, etiquetas Ene-Dic), `IngresosPorRecurso` (top 6, barras con %), `PagosPorTipo` (desglose Senia/PagoParcial/PagoTotal) y `CalcularVariacion` (% vs mes anterior).
- `Services/PagoService.cs`: nuevo `GetPagosEnRango(desde, hasta)` con includes de Turno.Recurso/Servicio.
- `Controllers/DashboardController.cs` (nuevo): `Index()` con `[Authorize]`.
- `Views/Dashboard/Index.cshtml` (nuevo): KPIs (Ingresos con delta vs mes anterior), graficas CSS puras (ocupacion semanal, ingresos por dia, evolucion 12 meses), cards "Proximos turnos"/"Sin confirmar" con botones `data-ver-turno`, "Ingresos por recurso (mes)" con barras, "Pagos del mes por tipo" y saldo pendiente.
- `Scripts/jk-dashboard.js` (nuevo): drawer de detalle en la MISMA seccion (GetDetailDrawer + registrar pago + toast), sin navegar a la Agenda.
- `Content/Pages/jk-dashboard-section.css` (nuevo): graficas (`.jk-chart-bars/rows`, `.jk-chart-row-label-wide`), listas (`.jk-dash-*`), saldo, `.kpi-delta` (variacion mensual, up/down/neutral) y `.jk-pagos-tipo`. `.jk-dash-item` como boton.
- `Views/Shared/_Sidebar.cshtml`: item "Dashboard" (bi-graph-up-arrow) en grupo Principal.
- `JKTurnos.csproj`: DashboardController, DashboardService, DashboardViewModel, Views/Dashboard/Index.cshtml, jk-dashboard-section.css, jk-dashboard.js, NotificacionService.cs, migracion AddNotificacionesWhatsApp (.cs/.Designer/.resx).

## Decisiones nuevas
- D-033: Modulo Notificaciones al Cliente, canales por fases. Fase 1 WhatsApp via wa.me + plantillas configurables (CONSOLIDADA). Fase 2 email SMTP con plantillas y credenciales propias (implementada, pendiente de confirmacion). Fase 3 centro in-app + recordatorios programados.
- D-034: Modulo Dashboard en SECCION APARTE (TEAM LEAD: nada en el Command Center). Fase 1 con KPIs/graficas/rankings/saldo; Fase 2 dashboard de pagos; Fase 3 exportacion.

## Reglas de negocio aplicadas
- D-032: encoding UTF-8 habilitado (ya vigente desde la sesion previa del dia).
- D-028/D-029: Ingresos del dashboard = pagos reales (no proyeccion).
- D-008: soft delete y scope por TenantId en todas las consultas nuevas (BaseRepository).
- D-002: el Command Center NO se modifica.

## Pendientes
1. Confirmacion del TEAM LEAD de la Fase 2 de Notificaciones (email SMTP).
2. Siguiente paso a definir por TEAM LEAD: Dashboard Fase 3 (exportacion CSV/PDF) u otros.
3. Drawer V2 (acciones de estado, reagendar, auditoria TurnoHistorial) y Fase 3 de notificaciones (centro in-app + recordatorios programados): a futuro, no implementar sin confirmacion.

## Build
- MSBuild Debug: 0 errores.