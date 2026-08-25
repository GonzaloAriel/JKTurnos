# Plan de Implementación: Expansión SaaS - Módulos Plataforma (AdminGlobal)

## Objetivo
Implementar los módulos de gestión de plataforma necesarios para escalar JKTurnos a un modelo SaaS multi-tenant robusto, permitiendo al AdminGlobal gestionar tenants, planes y auditoría global.

## Fases de Implementación

### Fase 1: Estructura de Base de Datos y Servicios Core (Backend)
- [ ] **Migración Inicial de Plataforma:**
    - Crear tabla `Planes` (definición de límites).
    - Crear tabla `AuditoriaGlobal` (logs inmutables).
    - Modificar tabla `Tenants`: agregar `PlanId` (FK) y `FechaVencimientoSuscripcion`.
- [ ] **Implementación Servicios de Plataforma:**
    - `PlanService`: CRUD para definir planes.
    - `AuditoriaService`: Servicio inmutable de registro de logs (`LogAccion(userId, accion, tenantIdAfectado, detalleJSON)`).
    - `TenantService` (actualización): lógica para vincular nuevo tenant a un plan por defecto.

### Fase 2: Módulo de Gestión de Tenants y Seguridad (AdminGlobal)
- [ ] **Area de Administración Global:**
    - Configurar área `Global` (MVC Areas o estructura de carpetas).
    - Implementar `GlobalController` (landing del AdminGlobal).
    - CRUD de `Tenants` (con vista para asignar plan y estado).
- [ ] **Seguridad:**
    - Asegurar filtros de acceso: `AdminGlobal` (TenantId=0) -> acceso exclusivo a Area Global.
    - Asegurar que `BaseRepository` y métodos operativos (Agenda, Turnos) denieguen el acceso a `AdminGlobal`.

### Fase 3: Auditoría y Dashboard de Plataforma
- [ ] **Auditoría Global:**
    - Registrar acciones críticas en la creación/edición de tenants.
    - Vista de listado de Auditoría para el AdminGlobal.
- [ ] **Dashboard de Plataforma (Command Center):**
    - Implementar KPIs: Tenants activos, nuevos, salud del sistema (queries agregadas sobre tablas existentes).

## Notas de Seguridad y Estabilidad
- **Aislamiento:** La separación entre AdminGlobal y Tenants debe ser estricta a nivel de capa de servicio.
- **Inmutabilidad:** La tabla `AuditoriaGlobal` no debe tener operaciones de UPDATE ni DELETE en la aplicación.
- **Compatibilidad:** Los tenants existentes recibirán un Plan por defecto en la migración para no romper el acceso.
