FORMATO: 1.0

## Reglas
Guardar problemas conocidos que no estan resueltos.

## Problemas
- P-003: Agenda no muestra turnos creados. Causa sospechada: Mismatch de TenantId en ClaimsIdentity o fallo en TenantProvider. Diagnostico previo (2026-08-11): `_recursoService.GetAll()` devuelve lista vacia dentro de `Agenda()`. OBSERVACION (2026-08-13): la Agenda renderiza recursos y Time Blocks con los turnos del dia (el Drawer de detalle funciono sobre bloques reales). Parece resuelto, posiblemente como efecto de las correcciones en e7f4a0d/f146379. Prioridad fijada por TEAM LEAD: P0 (bloquea la vista movil). Pendiente de confirmacion de cierre por TEAM LEAD.

## RESUELTOS
- P-001/P-002: Vinculación Recurso-Servicio (Resuelto en fase de implementación).
