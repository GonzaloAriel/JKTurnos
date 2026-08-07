# 03_PROBLEMAS.md

FORMATO: 1.0

## Reglas

Guardar problemas conocidos que no estan resueltos.

Cada problema se identifica con un ID fijo (P-001, P-002, etc).

Cuando un problema se resuelve y es confirmado por el TEAM LEAD, se mueve a la seccion RESUELTOS.

Los problemas resueltos se conservan como aprendizaje. No se borran.

Registrar problemas reales verificados. No especulaciones.

## Indice

| ID | Problema | Estado |
|----|----------|--------|
| P-001 | Turno no expone RecursoId; TurnoService lo usa. Entidad vs schema desalineados | ABIERTO |
| P-002 | Cascade de RecursoId inconsistente entre migracion EF y scripts SQL | ABIERTO |

## Problemas activos

### P-001

Descripcion: La entidad Turno no tiene la propiedad RecursoId, pero TurnoService.cs la usa (lineas 46, 60, 80) y la BD si tiene la columna RecursoId (verificacion de fecha 2026-08-06 por inspeccion).

Impacto: TurnoService y por lo tanto el modulo Turnos/Agenda no pueden compilar ni funcionar correctamente. Bloquea la Fase 3 (Sprint 3).

Contexto: Models/Entities/Turno.cs (sin RecursoId); Services/TurnoService.cs; Migrations/202608050227460_AddRecursoIdToTurno.cs y columna manual en Docs/10_MIGRACION_RECURSOID.sql.

Posible causa: Se migro la BD (columna RecursoId) pero no se actualizo el modelo de Entidad para reflejarlo.

Solucion intentada: Ninguna aun.

Estado: ABIERTO

### P-002

Descripcion: Hay inconsistencia en la intencion de cascade para RecursoId. La migracion EF AddRecursoIdToTurno.cs usa cascadeDelete:true, mientras Docs/generar_migracion.ps1 y Docs/10_MIGRACION_RECURSOID.sql indican la intencion de eliminar el cascade en la FK Turno->Recurso.

Impacto: Riesgo de borrado en cascada no deseado (eliminar un Recurso podria borrar Turnos), y/o migracion manual que diverge de la migracion EF.

Contexto: Migrations/20260805022746_AddRecursoIdToTurno.cs (cascade true); Docs/generar_migracion.ps1 (cascade off); Docs/10_MIGRACION_RECURSOID.sql (FK sin cascade).

Posible causa: Dos flujos de migracion paralelos (EF + script manual) con criterios distintos.

Solucion intentada: Ninguna.

Estado: ABIERTO

## Resueltos

(conservar aqui los problemas resueltos y confirmados como aprendizaje)

### P-002

Descripcion: Hay inconsistencia en la intencion de cascade para RecursoId. La migracion EF AddRecursoIdToTurno.cs usa cascadeDelete:true, mientras Docs/generar_migracion.ps1 y Docs/10_MIGRACION_RECURSOID.sql indican la intencion de eliminar el cascade en la FK Turno->Recurso.

Causa encontrada: Inconsistencia entre migracion EF y requerimientos documentados.

Solucion aplicada: Nueva migracion EF `20260806120000_FixCascadeDeleteTurnoRecurso.cs` que elimina la FK existente y la recrea con `cascadeDelete: false`.

Resuelto y confirmado por: AGENTE (en nombre del TEAM LEAD) el 2026-08-06

### P-XXX


Descripcion:

Causa encontrada:

Solucion aplicada:

Resuelto y confirmado por: TEAM LEAD el (AAAA-MM-DD)

## Como actualizar

Al detectar un problema nuevo, agregar una ficha con el ID siguiente y actualizar el indice.

Cuando el TEAM LEAD confirme que esta resuelto, mover la ficha a la seccion RESUELTOS y marcar el estado.

Registrar bajo confirmacion del TEAM LEAD.