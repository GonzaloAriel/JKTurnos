# SESION 2026-08-14: PLAN ROLES, CUENTAS Y GESTION DE TENANTS

## Plan: Roles, Cuentas y Gestion de Tenants

### 1. Decisiones de arquitectura (a registrar en `AI/02_DECISIONES.md`)

NOTA (2026-08-14): D-020 y D-021 fueron tomadas por el Command Center (ver AI/02_DECISIONES.md). Este plan pasa a reservar **D-022** y **D-023**.

**D-022 — Acceso global de plataforma (excepcion controlada a D-008).**
`AdminGlobal` no pertenece a ningun negocio: se autentica con claim `TenantId = 0`. Como `BaseRepository<T>` filtra por TenantId (y `Tenant` ni siquiera implementa `ITenantEntity`), se crea `GlobalRepository<TEntity>` (`TEntity : BaseEntity`) que filtra **solo por IsActive**, usado exclusivamente por servicios de plataforma (`TenantService`, y la via global de `UserService`). Efecto colateral deseado: un AdminGlobal nunca ve datos de negocio a traves de `BaseRepository` (le devuelve vacio), lo que blinda los modulos operativos.

**D-023 — Roles como constantes + atributos, sin tabla de permisos.**
`Role` sigue siendo string. Clase `Roles` con constantes. Autorizacion con `[Authorize(Roles = ...)]` y `User.IsInRole(...)` en vistas. RBAC completo queda a futuro (D-013 no aplica; es nuevo).

**Roles**
- `AdminGlobal` — Dueno del sistema. Gestiona Tenants y usuarios de cualquier tenant.
- `Admin` — Administra su tenant: usuarios (rol `User`), configuracion.
- `User` — Empleado. Acceso operativo a los modulos actuales (turnos, agenda, clientes, servicios, recursos). Sin gestion de usuarios.

### 2. Cambios de datos

- **Sin migracion de schema.** `Role` y `Tenant` ya existen.
- **Seed** (`Migrations/Configuration.cs`): agregar `global@jkturnos.com` / `123456`, `Role = AdminGlobal`, `TenantId = 0`, `IsActive = true`. Mantener el `Admin` demo existente.

### 3. Archivos NUEVOS

| Archivo | Responsabilidad |
|---|---|
| `Models/Entities/Roles.cs` | Constantes `AdminGlobal`, `Admin`, `User` |
| `Data/GlobalRepository.cs` | `GetAll/GetById/Add/Update/Delete` filtrando solo `IsActive` (sin TenantId) |
| `Services/TenantService.cs` | CRUD de Tenants via `GlobalRepository<Tenant>` + regla: al crear, subdominio unico y opcion de crear el 1er Admin |
| `Services/UserService.cs` | CRUD usuarios con scope por rol (lee claims): Admin → solo su tenant y solo rol `User`; AdminGlobal → cualquier tenant, roles `Admin`/`User`/`AdminGlobal`. Asigna `TenantId` y hashea password server-side (nunca bindear `PasswordHash`). Protege: no auto-desactivar, no tocar AdminGlobal desde Admin |
| `Controllers/TenantsController.cs` | `[Authorize(Roles = Roles.AdminGlobal)]`, patron CRUD estandar (`Populate*`, ModelState) |
| `Controllers/UsuariosController.cs` | `[Authorize(Roles = "AdminGlobal,Admin")]`; decide tenant/rol segun claim |
| `Models/ViewModels/UsuarioViewModel.cs` | `User` + `Password` (plain, no `PasswordHash`) + dropdowns (Tenants para AdminGlobal, Roles permitidos segun rol) |
| `Views/Tenants/{Index,Create,Edit,Delete}.cshtml` | Usando `_CrudPage`, `_FormLayout`, `_FormActions`, `_TableActions`, `_EmptyState` |
| `Views/Usuarios/{Index,Create,Edit,Delete}.cshtml` | Idem. Index: columna Tenant solo para AdminGlobal; filtro por tenant |

### 4. Archivos MODIFICADOS

| Archivo | Cambio |
|---|---|
| `Controllers/BaseController.cs` | Agregar `CurrentRole`, `IsAdminGlobal`, `IsAdmin` (lee claims) |
| `Migrations/Configuration.cs` | Seed AdminGlobal demo |
| `Views/Shared/_Sidebar.cshtml` | Grupo "Sistema" solo AdminGlobal (`Tenants`, `Usuarios`) + item `Usuarios` en "Gestion" para Admin |
| `JKTurnos.csproj` | Includes de archivos nuevos |
| `AI/02_DECISIONES.md` | D-022, D-023 |
| `AI/01_ESTADO.md` | Estado del modulo (EN PROGRESO hasta confirmacion del TEAM LEAD) |

`AccountController` y `Login.cshtml`: **sin cambios** (sin registro publico por decision).

### 5. Matriz de permisos

| Accion | AdminGlobal | Admin | User |
|---|---|---|---|
| Login/Logout | Si | Si | Si |
| Tenants CRUD | Si | No | No |
| Usuarios CRUD (global) | Si | No | No |
| Usuarios CRUD (su tenant, rol User) | Si | Si | No |
| Resetear password | Si | Si (su tenant) | No |
| Modulos operativos (turnos/agenda/clientes/servicios/recursos) | — (vacio) | Si | Si |

### 6. Flujos clave

- **Crear Tenant** (AdminGlobal): `Tenants/Create` → Nombre + Subdominio (unico) + alta del primer Admin del tenant (Nombre, Email, Password). Nace `Tenant` + `User(Role=Admin, TenantId=tenant)`.
- **Crear usuario**: `Usuarios/Create`. AdminGlobal elige Tenant + rol (`Admin`/`User`); Admin ve solo su tenant y rol `User`. Password requerido, se hashea con `PasswordHasher.Hash`.
- **Login**: sin cambios. Los claims ya incluyen `Role` y `TenantId` (`AuthService.SignIn`).

### 7. Verificacion

- `MSBuild Debug` → 0 errores.
- Prueba manual: login `global@jkturnos.com` → crear tenant+admin; login del nuevo admin → crear empleado `User`; login empleado → operar turnos y NO ver Tenants/Usuarios en sidebar; confirmar que Admin no puede crear otro Admin ni ver otros tenants.

---
ESTADO: PLAN GUARDADO. NO IMPLEMENTADO. Esperando confirmacion del TEAM LEAD para iniciar.