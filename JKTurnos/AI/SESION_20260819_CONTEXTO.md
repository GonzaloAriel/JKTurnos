# SESION 2026-08-19: FILTRO GRILLA, NOTIFICACIONES FASE 1, DASHBOARD SECCION APARTE

## Objetivo
1. Filtro de recursos aplicado a la grilla desktop de la Agenda.
2. Modulo Notificaciones al Cliente (D-033), Fase 1: WhatsApp via link `wa.me` con plantilla editable.
3. Modulo Dashboard (D-034) en SECCION APARTE (nada en el Command Center), con drawer de turno sin salir de la seccion.

## Estado
- Filtro de recursos en grilla desktop: CONSOLIDADO (aprobado por TEAM LEAD, 2026-08-19).
- Notificaciones Fase 1 (D-033): CONSOLIDADA (aprobado por TEAM LEAD, 2026-08-19). Migracion `AddNotificacionesWhatsApp` aplicada a la BD local.
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
- D-033: Modulo Notificaciones al Cliente, canales por fases. Fase 1 WhatsApp via wa.me + plantillas configurables; Fase 2 email SMTP; Fase 3 centro in-app + recordatorios programados.
- D-034: Modulo Dashboard en SECCION APARTE (TEAM LEAD: nada en el Command Center). Fase 1 con KPIs/graficas/rankings/saldo; Fase 2 dashboard de pagos; Fase 3 exportacion.

## Reglas de negocio aplicadas
- D-032: encoding UTF-8 habilitado (ya vigente desde la sesion previa del dia).
- D-028/D-029: Ingresos del dashboard = pagos reales (no proyeccion).
- D-008: soft delete y scope por TenantId en todas las consultas nuevas (BaseRepository).
- D-002: el Command Center NO se modifica.

## Pendientes
1. Siguiente paso a definir por TEAM LEAD: Notificaciones Fase 2 (email SMTP) o Fase 3 de Dashboard (exportacion CSV/PDF).
2. Drawer V2 (acciones de estado, reagendar, auditoria TurnoHistorial) y Fase 3 de notificaciones (recordatorios programados): a futuro, no implementar sin confirmacion.

## Build
- MSBuild Debug: 0 errores.