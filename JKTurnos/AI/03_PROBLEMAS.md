FORMATO: 1.0

## Reglas
Guardar problemas conocidos que no estan resueltos.

## Problemas

- P-006 (DEUDA / C5): migracion `AddAvatarUrlToConfiguracion` es no-op: la columna `AvatarUrl` de `ConfiguracionTenant` la agrega `AddAvataresPlataforma` (202608262243430). La migracion "fantasma" ya esta aplicada en la BD y no aporta cambios; solo rompe la trazabilidad (un `Down` no removeria la columna). No se corrige reescribiendo migraciones aplicadas (riesgo de corromper `__MigrationHistory`); queda como deuda de trazabilidad. Detalle en auditoria C5.

## RESUELTOS
- P-001/P-002: Vinculación Recurso-Servicio (Resuelto en fase de implementación).
- P-003: Agenda no muestra turnos creados. Diagnostico previo (2026-08-11): `_recursoService.GetAll()` devuelve lista vacia dentro de `Agenda()`. Causa sospechada: mismatch de TenantId. CERRO el TEAM LEAD el 2026-08-13: la Agenda renderiza recursos y Time Blocks (el Drawer de detalle funciono); el mismatch no se reproduce. Asociado a e7f4a0d/f146379.
- P-004: Error Razor en runtime en `Views/Home/Index.cshtml` ("Palabra clave foreach inesperada despues del caracter @"). Causa: `@foreach` dentro de `@if { }` en contexto de codigo (y el csproj usa `MvcBuildViews=false`, por eso no fallo al compilar). Corregido: `foreach` sin `@` en contexto de codigo; `@foreach` dentro de markup (`<div>`). Resuelto 2026-08-14 (build con `/p:MvcBuildViews=true` verifica las vistas).
- P-005: Pantalla 500 en `Turnos/Create` (y `Edit`) por "El recurso no esta disponible en el horario seleccionado". Causa: `TurnoService` lanza `InvalidOperationException` (regla de negocio correcta) pero el controller no la capturaba. Corregido: `TurnosController` captura la excepcion, agrega `ModelState.AddModelError` y re-renderiza el form con los dropdowns repoblados; `_FormLayout` renderiza `@Html.ValidationSummary` (clase `.jk-form-error`, DS). Resuelto 2026-08-14.
