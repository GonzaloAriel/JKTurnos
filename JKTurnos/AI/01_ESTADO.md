FORMATO: 1.0

## Regla de estados
CONSOLIDADO - probado y confirmado explicitamente por el TEAM LEAD como terminado, aprobado y funcionando.

EN PROGRESO - trabajo activo, aun no terminado ni confirmado.

PENDIENTE - No evaluado.

## Estado actual
- Vinculacion Recurso-Servicio (N:M, checkbox bidireccional + drawers de quick-create en Recursos y Servicios): CONSOLIDADO (2026-08-12, aprobado por TEAM LEAD).
- Quick Create en Turnos/Create y Edit (drawers de Servicio y Recurso en modo select; Servicio opcional; filtro de recursos unificado): CONSOLIDADO (2026-08-12, aprobado por TEAM LEAD).
- Drawer de detalle del Time Block (V1 solo lectura; seccion Pagos/Seña SIEMPRE visible con placeholder "Sin senia" hasta V3): CONSOLIDADO (2026-08-13, aprobado por TEAM LEAD). V2 y V3 a futuro en Docs/16_DRAWER_TURNO_FUTURO.md.
- Agenda: EN PROGRESO. P-003 CERRADO por TEAM LEAD (2026-08-13). Vista movil P1 (acordeon por Recurso + horarios libres + filtro de recursos conectado, D-019): CONSOLIDADO por TEAM LEAD (2026-08-13). P2 FAB "Nuevo Turno" (D-017): ELIMINADO por TEAM LEAD (2026-08-13), sin relevancia por ahora; el boton "Nuevo Turno" de la Agenda navega a /Turnos/Create. Pendiente: filtro de grilla (Docs/16).
- Home / Command Center: CONSOLIDADO (2026-08-14, aprobado por TEAM LEAD). Saludo contextual real; acciones rapidas conectadas (Turnos/Create, Turnos/Agenda, Clientes/Create); "Mi dia" con datos reales en carriles por Recurso (chips compactos, D-020) + empty state con CTA; KPIs conectados (Turnos Hoy, Ingresos = proyeccion del dia D-021, Ocupacion, Clientes). Pendiente (placeholders): sugerencias y actividad reciente (dependen del modulo de configuracion) y el filtro de grilla de Agenda.
- Modulo Roles/Cuentas/Tenants (plan AI/SESION_20260814_PLAN_ROLES.md, Fases 0-2): CONSOLIDADO (2026-08-18, aprobado por TEAM LEAD). Fase 0: `Roles` (constantes), `GlobalRepository` (D-022), seed `global@jkturnos.com` (AdminGlobal, TenantId=0), helpers `CurrentRole/IsAdminGlobal/IsAdmin` en `BaseController` (D-023). Fase 1: `UserService` y `TenantService` (scope por claims, hasheo server-side, subdominio unico, alta opcional del 1er Admin). Fase 2: `UsuarioViewModel`, `TenantsController`, `UsuariosController`, vistas Index/Create/Edit/Delete de ambos + sidebar (grupo Sistema solo AdminGlobal; item Usuarios en Gestion para Admin).
- Reparacion de BD del modulo Roles (2026-08-18, aprobado por TEAM LEAD): corregido `__MigrationHistory` (ContextKey -> `JKTurnos.Migrations.Configuration` + Model de la ultima migracion regenerado), eliminado el FK `Users.TenantId -> Tenants` para permitir `TenantId=0`, y creado `global@jkturnos.com` en la BD. `Update-Database` vuelve a ser consistente (verificado con `ef6.exe database update --script`).
- Alta de tenant operativo + fixes (2026-08-18, aprobado por TEAM LEAD): `TenantService.Create` siembra Estados de Turno base + `ConfiguracionTenant` (D-024); guard defensivo en `TurnoService.Crear` cuando no hay estados; creada `Views/Recursos/Delete.cshtml` (faltaba la vista).