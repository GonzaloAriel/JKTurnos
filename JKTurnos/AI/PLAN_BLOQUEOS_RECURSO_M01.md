# M-01 — Gestión de Disponibilidad por Recurso en Dos Conceptos (Plan Formal)

> Decisión: **D-046** (aprobada por TEAM LEAD el 2026-09-03).
> Alcance de este documento: fase de implementación **Phase 1 (Concepto A — BloqueoRecurso)** y **Phase 2 (Concepto B — Franja Formal)**.

---

## 1. Resumen

La gestión de disponibilidad por recurso se resuelve con **dos conceptos complementarios**:

- **Concepto A — `BloqueoRecurso`**: módulo nuevo de **indisponibilidad por Recurso**. Se
  comporta como **ocupado** dentro de la agenda (segunda fuente de ocupación junto a los
  turnos) y se gestiona desde un módulo de Gestión dedicado.
- **Concepto B — Franja Formal**: horario de trabajo **informativo y no bloqueante** en la
  Configuración del Negocio. El Admin **siempre** puede dar turnos fuera del rango.

No se introduce ningún algoritmo de disponibilidad nuevo: `TurnoService.ValidarDisponibilidad`
sigue siendo la única fuente de verdad y simplemente suma a `BloqueoRecurso` como segunda
fuente de ocupación.

---

## 2. Concepto A — `BloqueoRecurso`

### 2.1 Entidad

`Models/Entities/BloqueoRecurso.cs`

```csharp
public enum TipoBloqueo
{
    Unico,          // un día concreto (usa FechaInicio, el día completo o la franja horaria)
    RangoFechas,    // período continuo (FechaInicio..FechaFin)
    RecurrenteSemanal  // día de semana fijo (se ignora FechaFin)
}

public class BloqueoRecurso : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }

    [Required] public int RecursoId { get; set; }

    [Required] public TipoBloqueo Tipo { get; set; }

    // Unico: día puntual. RangoFechas: inicio del rango. RecurrenteSemanal: usa solo el
    // DayOfWeek de esta fecha como "día fijo de la semana".
    [Required] public DateTime FechaInicio { get; set; }

    // Solo para RangoFechas (extremo inclusivo). Null en Unico / RecurrenteSemanal.
    public DateTime? FechaFin { get; set; }

    // Franja horaria OPCIONAL dentro del día. Ambas null = todo el día.
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }

    [StringLength(500)] public string Descripcion { get; set; }

    [ForeignKey("RecursoId")] public virtual Recurso Recurso { get; set; }
}
```

Modelo de recurrencia **híbrido**: cubre los tres casos reales sin necesidad de tablas de
excepciones. `RecurrenciaMensual` queda **fuera de v1** (v2).

### 2.2 Pautas del dominio

- **Admin** gestiona (crear / editar / eliminar). **Operadores** solo **ven** los bloques
  grises en la agenda.
- **Colisión con turnos existentes** (al crear o editar): el bloqueo **se crea igual**
  (decisión del Admin). **NO** se cancelan turnos. Se muestra un aviso amable de que el
  recurso no podrá brindar el servicio en ese horario, con opción de ver/reagendar el turno
  (reagendado manual; **notificación automática FUERA de v1**).
- **NO** se crean bloqueos desde la Agenda (no hay botón ni acceso en la grilla).
- **Visual**: bloque **gris** `.jk-bloqueo-recurso` sobre la grilla (día / semana / móvil).
  Sin cliente, sin acciones de pago, **sin Drawer**.

### 2.3 Integración con disponibilidad

En `TurnoService.ValidarDisponibilidad` (línea ~155) se agrega una segunda consulta a
`BloqueoRecurso` que evalúa solapamiento con `[inicio, fin)` del turno en evaluación:

- `Unico`: si `FechaInicio.Date` coincide con `inicio.Date` (considerando la franja horaria).
- `RangoFechas`: si `inicio.Date` cae dentro de `[FechaInicio.Date, FechaFin.Date]`.
- `RecurrenteSemanal`: si `inicio.DayOfWeek` == `FechaInicio.DayOfWeek`.

Cuando el bloqueo tiene franja horaria, el solapamiento se evalúa sobre la franja (por
ejemplo 08:00–12:00); si no tiene franja, ocupa el día completo.

La lógica del día/p franja se concentra en `BloqueoRecursoService` (método `EstaBloqueado`),
para que el `TurnoService` solo consulte y delegue.

### 2.4 UI — Módulo de Gestión

- `Controllers/BloqueosController.cs` — `[Authorize(Roles = Roles.Admin)]` (solo Admin).
- Vistas `Views/Bloqueos/{Index,Create,Edit,Delete}.cshtml`.
- Item de navegación **"Bloqueos"** en el sidebar (grupo Gestión, solo Admin).

---

## 3. Concepto B — Franja Formal

### 3.1 Modelo

En `ConfiguracionTenant` se agregan:

```csharp
[Required]
public TimeSpan HoraApertura { get; set; }   // default 08:00

[Required]
public TimeSpan HoraCierre { get; set; }     // default 19:30
```

Se inicializan con **08:00 / 19:30** (= 690 min, equivalente al `MinutosJornada` actual
hardcodeado). La migración es **aditiva** (auto-migración EF6, patrón D-043).

### 3.2 Pautas

- **Informativa y NO bloqueante**: el Admin siempre puede dar turnos fuera del rango
  (ej. 22hs en Nochebuena). La UI muestra la aclaración: *"Este horario es informativo y
  no impide dar un turno más allá del rango. Sirve para que los clientes y recursos conozcan
  las formalidades"*.
- El **KPI de Ocupación** (`HomeService` y `DashboardService`) reemplaza el `690` hardcodeado
  por `(HoraCierre - HoraApertura)` de la franja formal.
- Turnos fuera de la franja **suman minutos** y el KPI **se topa a 100%** (igual que hoy).
  El reporte de "exceso de ocupación" queda a **v2**.
- **Referencia visual** de la franja en la grilla: **fuera de v1** (Opción A; el default
  coincide con la grilla actual).

---

## 4. Migración

- **Aditiva** vía **AUTO-MIGRACIÓN EF6 en startup** (`AutomaticMigrationsEnabled = true` +
  `MigrateDatabaseToLatestVersion`, ya configurado en `Global.asax.cs` y
  `Migrations/Configuration.cs`).
- Se agrega el `DbSet<BloqueoRecurso>` + configuración en `OnModelCreating` y las columnas en
  `ConfiguracionTenant`. Nada manual.

---

## 5. Concesión de turnos y agenda

- El comportamiento actual de la grilla que **se expande para turnos tardíos** se mantiene
  intacto (los mínimos/máximos de turnos siguen ajustando `gridInicio`/`gridFin`).
- Los bloqueos grises se posicionan sobre la grilla con el mismo sistema de coordenadas de
  los time blocks; no desplazan celdas.

---

## 6. Fases de implementación

### Phase 1 — Concepto A (`BloqueoRecurso`) ✅ (completada 2026-09-03)

1. `Módulo` entidad + enum.
2. `DbSet` + `OnModelCreating` en `JKContext`.
3. `BloqueoRecursoService` (CRUD + `EstaBloqueado` + `TurnosColisionan`).
4. Integración en `TurnoService.ValidarDisponibilidad`.
5. `BloqueoRecursoViewModel` + `BloqueosController` (Admin).
6. Vistas `Index / Create / Edit / Delete`.
7. Render de bloques grises en las tres grillas + `AgendaViewModel.Bloqueos`.
8. CSS + item de sidebar.
9. Registro en `.csproj`.
10. Build MSBuild (0 errores) + `MvcBuildViews`.

### Phase 2 — Concepto B (Franja Formal) ✅ (completada 2026-09-03)

1. `HoraApertura`/`HoraCierre` en `ConfiguracionTenant` (nullable; fallback 08:00 - 19:30 = 690 min).
2. Edición + aclaración en `Configuracion/DatosNegocio` (defaults de vista 08:00 / 19:30).
3. `ConfiguracionService.GetMinutosJornada()` como denominador del KPI en `HomeService` + `DashboardService`.

---

## 7. Fuera de alcance (v2)

- `RecurrenciaMensual`.
- Notificación automática al reagendar por colisión.
- Reporte de "exceso de ocupación" del KPI.
- Referencia visual de la Franja Formal en la grilla (Opción A).
