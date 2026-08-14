FORMATO: 1.0

## Reglas
Guardar problemas conocidos que no estan resueltos.

## Problemas

## RESUELTOS
- P-001/P-002: Vinculación Recurso-Servicio (Resuelto en fase de implementación).
- P-003: Agenda no muestra turnos creados. Diagnostico previo (2026-08-11): `_recursoService.GetAll()` devuelve lista vacia dentro de `Agenda()`. Causa sospechada: mismatch de TenantId. CERRO el TEAM LEAD el 2026-08-13: la Agenda renderiza recursos y Time Blocks (el Drawer de detalle funciono); el mismatch no se reproduce. Asociado a e7f4a0d/f146379.
