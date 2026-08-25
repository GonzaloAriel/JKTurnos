# 00_CONTEXT.md — Prompt de Contexto Canónico de JKTurnos

> Referencia maestra para toda implementación. No duplica Docs/; puntualiza el
> "qué" y el "por qué" del sistema para que nueva funcionalidad respete la
> filosofía, la arquitectura y las convenciones vigentes.

---

## 1. Filosofía del producto

JKTurnos es **el centro operativo diario del negocio**, no un CRUD de turnos.
Gestiona tiempo y disponibilidad para cualquier rubro (peluquería, clínica,
cancha, taller). La Agenda es el núcleo: desde ella se opera el día a día.
El Home es el "Command Center" de un *tenant*; el AdminGlobal administra la
plataforma completa (tenants, planes, auditoría).

Principio: **priorizar simplicidad sobre especialización** — un modelo único
de Recursos/Servicios/Turnos para cualquier negocio.

---

## 2. Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Presentation | ASP.NET MVC 5 |
| Runtime | .NET Framework 4.7 |
| ORM | Entity Framework 6 (*aplanar migraciones: una sola `Inicial` + `MigrateDatabaseToLatestVersion` al arranque*, D-027) |
| DB | SQL Server Express |
| Auth | OWIN Cookie Auth + Claims (`NameIdentifier`, `Name`, `Email`, `Role`, `TenantId`) |
| UI | Bootstrap + Design System v2 (`Content/DesignSystem/jk-tokens.css`, `jk-components.css`) |
| Arquitectura | Monolito MVC, Repository Pattern, multi-tenant por `TenantId` |

---

## 3. Arquitectura (flujo inviolable)

```
Usuario → Controller → Service → Repository → EF → SQL Server
```

- **Controller:** solo coordina (recibe requests, valida ModelState, llama Services,
  retorna Views). Nunca lógica de negocio.
- **Service:** contiene todas las reglas (validaciones, cálculos, procesos). Nunca
  accede a la interfaz.
- **Repository:** acceso a datos, filtrado automático por `TenantId` + `IsActive`.
  Nunca contiene reglas de negocio. Soft delete obligatorio.
- **Models:** entidades puras (sin lógica).
- Nunca SQL directo salvo necesidad acotada y encapsulada en Repository (D-008).

Convenciones de carpetas: `Models/`, `ViewModels/`, `Controllers/`, `Services/`,
`Data/`, `Infrastructure/`, `Helpers/`, `Views/`.

---

## 4. Multi-tenant & seguridad

### Principio
Todo acceso a datos se filtra por `TenantId`. Ningún *tenant* ve datos de otro.

### Repositorios
- **`BaseRepository<T>`** (D-008): filtra por `TenantId` + `IsActive`, soft delete.
  Para entidades de negocio (`ITenantEntity`).
- **`GlobalRepository<T>`** (D-022): filtra solo `IsActive` (sin `TenantId`),
  usado **únicamente** por servicios de plataforma (`TenantService`, `UserService`,
  `AuditoriaService`). Su `Add` NO asigna `TenantId`.

### Roles (constantes en `Roles.cs`, D-023)
| Rol | TenantId | Alcance |
|-----|----------|---------|
| `AdminGlobal` | 0 (sin tenant) | Plataforma: Tenants, Usuarios, Plan, Auditoría. **Nunca** datos de negocio (`BaseRepository` le devuelve vacío → blindaje automático). |
| `Admin` | tenant | Configuración, usuarios (rol `User`), negocio. |
| `User` | tenant | Operativo: turnos, agenda, clientes, servicios, recursos, pagos. |

### Claims
`Role` y `TenantId` via `AuthService.SignIn`. `BaseController` expone
`CurrentRole`, `IsAdminGlobal`, `IsAdmin` (D-022/D-023). Login público deshabilitado
(creación de cuenta solo por AdminGlobal vía CRM de Tenants, o 1er Admin en alta de tenant).

---

## 5. Dominio y modelo de datos (DER)

Ver `Docs/13_DER.md` (oficial). Resumen:

**Core Domain:**
- **Tenant** — raíz. Nombre + subdominio. Puede ligarse a un **Plan** (D-035).
- **User** — pertenece a un Tenant (o TenantId=0 para AdminGlobal).
- **Recurso** — elemento reservable (obligatorio en todo Turno; D-001). Agnóstico de rubro.
- **Servicio** — actividad ofrecida (opcional en Turno; D-014).
- **Turno** — reserva: 1 Tenant, 1 Cliente, 1 Recurso, 1 Servicio (opcional), 1 EstadoTurno.
- **EstadoTurno** — *operativo* (Pendiente/Confirmado/En curso/Finalizado/Cancelado/Reagendado).
- **Cliente** — contacto (Teléfono/Email) usado por Notificaciones.

**Planificado (no implementado):**
- **Pago** — estado *económico* CALCULADO desde pagos (no persistido en Turno; D-005/D-028). El precio del Servicio es solo referencia (sin tope de monto).

**Principios del modelo:**
- La Agenda NO se persiste (se calcula de turnos/bloqueos). Time Block y Drawer
  son pura UI.
- Estados operativos ≠ estados económicos.
- Ningún Turno sin Recurso.
- No duplicar información que pueda calcularse.

---

## 6. UX / UI (reglas de oro)

- **Agenda es el Home y el centro** (D-002). No listados de turnos.
- **Drawer de detalle por Time Block** se abre desde la izquierda (D-003), nunca
  pantalla nueva. Contenedor fijo en `Agenda.cshtml`, contenido vía fetch a
  `TurnosController.GetDetailDrawer`.
- **Color del Time Block = color del Recurso, nunca del estado** (D-004/D-011).
  Estado = punto (`--jk-status-color`).
- **Vista móvil** = acordeón de tarjetas por Recurso con horarios libres (D-019).
- **CSS:** solo variables del DS (`--jk-*`); clases de página solo en
  `Content/Pages/jk-{modulo}.css` (D-010).
- **"Nuevo Turno" de Agenda → `/Turnos/Create`** (el FAB de Quick Create está
  cancelado; D-017).
- **Command Center** (Home/Index del *tenant*): "Mi día" en carriles por recurso
  (D-020), KPIs de proyección/ingresos/pagos reales/ocupación (D-021/D-029).
- **Dashboard de negocio** en sección aparte (D-034): nada de gráficas en el Home.

---

## 7. Estado actual (consolidado por TEAM LEAD)

- Módulos **Clientes / Servicios / Recursos / Turnos / Estados** operativos.
- **Agenda** (desktop + móvil) con Time Blocks reales, Drawer V1 solo lectura
  (pago visible siempre, D-015).
- **Pagos** (D-028): listado + crear/anular/editar monto.
- **Notificaciones** (D-033): WhatsApp (vía `wa.me`, consolidado) + SMTP email (implementado, pend. confirmación).
- **Dashboard** (D-034): Fases 1 y 2 consolidadas.
- **Roles/Cuentas/Tenants** (D-022/D-023): AdminGlobal, GlobalRepository, UserService,
  TenantService, TenantsController, UsuariosController, vistas. BD reparada y seed
  `global@jkturnos.com`. **CONSOLIDADO.**
- **Configuración de negocio** (D-025): datos + CRUD de estados.
- **Logo por URL** (D-026): sidebar + bienvenida.
- **Auto-migración** (D-027): cadena aplanada en `Inicial`; `MigrateDatabaseToLatestVersion`.

---

## 8. Cómo trabajamos (proceso de memoria)

1. **Arranque obligatorio:** leer `00_MEMORIA.md` → `01_ESTADO.md` → `02_DECISIONES.md`.
2. **Decisiones** → `02_DECISIONES.md` (IDs D-XXX, VIGENTE/REEMPLAZADA). Nunca borrar;
   marcar la vieja como REEMPLAZADA.
3. **Problemas** → `03_PROBLEMAS.md` (IDs P-XXX).
4. **Estado** → `01_ESTADO.md`. Solo el **TEAM LEAD** confirma CONSOLIDADO. Nunca
   auto-consolidar.
5. **Cada tarea:** leer rutas por tipo (ver `00_MEMORIA.md` § rutas). Modular Nuevo/CRUD
   → leer `02_DECISIONES.md` + Docs (`01_AGENTS`, `JKTurnosDictionary`,
   `DashboardLayoutSystem`, `03_ARCHITECTURE`, `05_DESIGN_SYSTEM`, `13_DER`).
6. **Verificación:** `MSBuild Debug` → 0 errores (con `/p:MvcBuildViews=true`).
7. **Commit:** solo bajo pedido explícito del TEAM LEAD.
8. **Duda de arquitectura:** detenerse, explicar, proponer alternativas (regla 13 del
   prompt de inicio). No decidir sin confirmación.

---

## 9. Decisiones clave vigentes (índice rápido)

- D-001 Turno → Recurso obligatorio | D-002 Agenda es el Home | D-003 Drawer desde izquierda
- D-004 Color = recurso, nunca estado | D-005 Operativo ≠ económico
- D-008 Soft delete + TenantId (BaseRepository) | D-010 CSS del DS v2
- D-013 N:M Recurso-Servicio + QuickCreate en Drawer (fuera del form)
- D-014 Servicio opcional | D-015 Drawer V1 solo lectura
- D-019 Vista móvil = acordeón por recurso + horarios libres
- D-022 GlobalRepository (solo IsActive) | D-023 Roles constantes (sin tabla de permisos)
- D-024 Tenant nuevo nace operativo (estados + config)
- D-025 Configuración solo Admin | D-027 Auto-migración (una `Inicial`)
- D-028 Pago: estado económico calculado, precio = referencia
- D-029 Ingresos = pagos reales del día (no proyección)
- D-033 Notificaciones: WhatsApp (✓) + SMTP (✓)
- D-034 Dashboard sección aparte (nada en Home)
- D-035 Gestión de plataforma: `Planes` + `AuditoriaGlobal` + Tenants→Plan

---

*Documento de referencia. Cualquier implementación nueva verifica este contrato.*
