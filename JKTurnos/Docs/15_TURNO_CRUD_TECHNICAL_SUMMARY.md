# DOCUMENTACIÓN TÉCNICA: MÓDULO TURNOS

## 1. PROPÓSITO DEL MÓDULO
El módulo de Turnos es el núcleo funcional de JKTurnos. Su objetivo es gestionar la reserva y el estado de tiempo de los recursos del negocio. Representa la intersección entre un Cliente, un Servicio, un Recurso reservable y un bloque de tiempo específico.

## 2. ENTIDADES RELACIONADAS
- **Turno**: Entidad principal que centraliza la relación.
- **Cliente**: Entidad reservante.
- **Servicio**: Define la duración y tipo de actividad.
- **Recurso**: Elemento reservable (profesional, espacio, equipo).
- **EstadoTurno**: Clasificación del ciclo de vida del turno (Pendiente, Cancelado, etc.).

Relaciones: `Turno` tiene FKs obligatorias hacia `Cliente`, `Servicio`, `Recurso` y `EstadoTurno`. Implementa `ITenantEntity` para aislamiento por Tenant.

## 3. MODELOS
- **Turno.cs**: Entidad principal en `Models/Entities`. Contiene `TenantId`, IDs de relaciones, `FechaHoraInicio`, `FechaHoraFin` y `Observaciones`.
- **TurnoViewModel.cs**: Ubicado en `Models/ViewModels`. Prepara los datos para la UI, incluyendo propiedades para dropdowns (`Clientes`, `Servicios`, `Recursos`, `EstadosTurno`) y atributos de validación (`Required`, `DataType.DateTime`).

## 4. SERVICIO (`TurnoService`)
Responsable de la lógica de negocio, reglas de persistencia y validaciones.
- **Validaciones**: `ValidarDisponibilidad` asegura que no existan solapamientos para el mismo recurso en el rango solicitado.
- **Cálculos**: `CalcularFin` utiliza `Servicio.DuracionMinutos` para determinar el fin del turno automáticamente si no se provee.
- **Reglas**: Asigna estado "Pendiente" por defecto si no se especifica.
- **Eager Loading**: Utiliza `.Include()` para cargar `Cliente`, `Servicio`, `Recurso` y `EstadoTurno` en `GetAll` y `GetById`, evitando errores de `NullReferenceException`.
- **Interacción**: Utiliza repositorios genéricos (`BaseRepository`) para filtrado automático por `TenantId` e `IsActive`.

## 5. CONTROLLER (`TurnosController`)
Gestiona el flujo de UI.
- **Acciones**: `Index`, `Create` (GET/POST), `Edit` (GET/POST), `Delete` (GET/POST).
- **Flujo**: Utiliza `PopulateDropdowns` para preparar el `TurnoViewModel` en las vistas. Valida el `ModelState` en todas las acciones POST antes de delegar al `TurnoService`.
- **Seguridad**: Decorado con `[Authorize]`.

## 6. VISTAS
- Ubicadas en `Views/Turnos/`.
- **Patrón Visual**: Reutiliza componentes del `Design System` (`_CrudPage`, `_FormLayout`, `_DeleteLayout`, `_TableActions`, `_EmptyState`).
- **Index**: Tabla con listado (vista escritorio) y lógica de estado (`jk-badge`).
- **Create/Edit**: Formulario centralizado mediante `_FormLayout` y `_FormActions`.

## 7. CRUD (Flujo)
UI (Razor) → Controller (Action) → Service (Validaciones) → BaseRepository (Filtrado Tenant) → DB (EF).

## 8. MULTI-TENANT
El sistema garantiza aislamiento mediante `BaseRepository` y `TenantProvider`. Toda consulta a `Turno` se filtra automáticamente por `TenantId` derivado de los claims del usuario autenticado.

## 9. BASE DE DATOS
- Tabla `Turnoes`.
- Relaciones: FKs con `DeleteCascade` desactivado (especialmente en `RecursoId` para prevenir borrados no deseados).
- Migración: `202608050227460_AddRecursoIdToTurno` (columna y FK inicial) + `20260806120000_FixCascadeDeleteTurnoRecurso` (corrección de cascade).

## 10. DECISIONES TÉCNICAS
- **D-001**: Turno requiere obligatoriamente un Recurso.
- **D-007**: La agenda no es una entidad, es una vista calculada desde Turnos.

## 11. LECCIONES IMPORTANTES
- **P-001 (Resuelto)**: Desalineación entre Entidad (falta `RecursoId`) y BD. Solución: actualizar clase `Turno`.
- **P-002 (Resuelto)**: Inconsistencia en `CascadeDelete`. Solución: nueva migración que fuerza `cascadeDelete: false`.
- **Eager Loading**: Fundamental incluir todas las relaciones (`.Include()`) en los servicios para evitar `NullReferenceException` en vistas.

## 12. RELACIÓN CON AGENDA
El módulo de Turnos proporciona todos los datos necesarios para la futura visualización en Agenda:
- `RecursoId`, `FechaHoraInicio`, `FechaHoraFin` (definición del bloque).
- `Cliente`, `Servicio`, `EstadoTurno`, `Observaciones` (detalle del bloque).
