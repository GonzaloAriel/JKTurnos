# SESION 2026-08-28: PLAN REFACTOR - ESTADOS DE TURNO (CATALOGO FIJO)

ESTADO: POSTA 9 COMPLETADA (2026-08-28). Plan aprobado; refactor de estados (D-040) IMPLEMENTADO Y DOCUMENTADO en su totalidad.

Decisión marco: D-040 (estados de turno = catalogo operativo FIJO del sistema, no configurables por tenant; lo extra va a eventos/motivos/atributos). Reemplaza el CRUD configurable de D-025 en lo referente a estados. APROBADA por TEAM LEAD (2026-08-28).

Proximo paso: refactor de estados (D-040) COMPLETO en codigo y documentacion. Los "detalles" adicionales que indique el TEAM LEAD se trabajan por separado.

Posta 1 (auditoria) COMPLETADA (2026-08-28): radio de explosion mapeado y datos de BD reales auditados. Ver seccion "HALLAZGOS POSTA 1".

Posta 2 (contrato funcional) COMPLETADA (2026-08-28): contrato cerrado y APROBADO por TEAM LEAD (decisiones A1/B1/C). Ver seccion "CONTRATO POSTA 2 (APROBADO)".

Posta 3 (saneamiento BD / seed / migracion) COMPLETADA (2026-08-28): catalogo fijo sembrado con Codigo; migracion aditiva `202608281509241_AddCatalogoEstados` creada y registrada en csproj; rama de backfill legacy limpiada; rebuild desde cero VERIFICADO (tenant nuevo = exactamente 4 estados con Codigo estables, sin Confirmado/Atendido/No asistio; columnas Codigo+Icono presentes; build 0 errores). NOTA: `Codigo` se agrega por migracion EXPLICITA, pero `Icono` sigue viajando por migracion AUTOMATICA residual (porque `AutomaticMigrationsEnabled=true` en Configuration.cs, drift vs D-027 que documenta false). Detalle y evidencia en seccion "POSTA 3 (COMPLETADA)".

Posta 4 (eliminar CRUD de estados) COMPLETADA (2026-08-28): eliminadas acciones `EstadoCrear/EstadoEditar/EstadoEliminar(Confirmed)` en ConfiguracionController; ConfiguracionService sin CRUD de estados; eliminadas las 3 vistas CRUD; card "Estados de turno" fuera de Configuracion/Index; build 0 errores. Detalle y evidencia en seccion "POSTA 4 (COMPLETADA)".

Posta 5 (simplificar Crear/Editar sin dropdown; UX de acciones) COMPLETADA (2026-08-28): dropdown de estado eliminado del Edit; state card con badge + botones contextuales (Iniciar/Finalizar/Cancelar); endpoints POST ajax `IniciarAtencion`/`FinalizarAtencion`/`CancelarAtencion`; motor simple reusando TurnoService (matriz completa centralizada en P6). Build 0 errores + verificado end-to-end en app (transiciones reales + guardias). Detalle y evidencia en seccion "POSTA 5 (COMPLETADA)".

Posta 6 (motor de transiciones centralizado) COMPLETADA (2026-08-28): nuevo `Services/TurnoEstadoMotor.cs` centraliza la matriz canonica unica (`EsTransicionPermitida`), las transiciones `Iniciar`/`Finalizar`/`Cancelar` y el generico `CambiarEstado`, con toda la logica comun (guardia de estado origen, resolucion por Codigo, notificacion y auditoria). Se agrega la guardia de origen a `Cancelar` (Pendiente/En curso; terminales inmutables). `TurnoService` delega al motor (eliminada la logica duplicada). Build 0 errores + matriz verificada end-to-end (Pendiente->En curso SI, En curso->Cancelado/Finalizado SI, Pendiente->Finalizado NO, Cancelado->cualquier NO). Detalle y evidencia en seccion "POSTA 6 (COMPLETADA)".

Posta 7 (situacion temporal / reloj, calculada, NO estado) COMPLETADA (2026-08-28): nuevo `Models/Entities/SituacionTemporal.cs` con 3 claves limpias (`demorado`, `tiempo-excedido`, `a-tiempo`; se elimina 'horario-iniciado', redundante) + `Calcular(Turno, ahora)` puro (interpreta el reloj sobre el estado persistido; NUNCA muta estado) + helpers de display y `EsAlarma`. Se ELIMINA el dead code `TurnoService.CalcularEstadoEnVivo` (reemplazado; sin call sites). Build 0 errores + 11 tests de harness sobre la logica pura ALL PASS.   Detalle y evidencia en seccion "POSTA 7 (COMPLETADA)".

  Posta 8 (Agenda + Dashboard + Home; superficies + situacion temporal) COMPLETADA (2026-08-28): strings magicos por `Nombre` -> `Codigo` (Home/Dashboard/Recordatorio); Contrato C ("Sin confirmar" -> "Por atender"); dead data `AgendaViewModel.Estados` eliminada; situacion temporal pintada en Agenda (grid/semanal/mobile), Home ("Mi dia") y Dashboard (Proximos/Por atender); `TurnoDetailViewModel` + drawer pintan la situacion temporal junto al badge de estado. Build 0 errores + smoke test de runtime (login + Home/Dashboard/Agenda dia/semana/mobile/drawer = 200, indicadores de alarma renderizados). Detalle en seccion "POSTA 8 (COMPLETADA)".

  Posta 9 (auditoria final + documentacion) COMPLETADA (2026-08-28): docs oficiales (11, 12, 13_DER, 13_AGENDA_UX, 14) actualizados al catalogo FIJO de 4 estados (Pendiente/En curso/Finalizado/Cancelado) con la matriz canonica UNICA de `TurnoEstadoMotor` y la situacion temporal calculada; nota de obsoleto en 16_DRAWER_TURNO_FUTURO (sin reescribir los "futuro"); D-024 corregida (seed de 4 estados con Codigo) y D-025 marcada SUPERADA (CRUD eliminado en P4); auditoria de codigo limpia (sin strings por Nombre, sin CRUD residual). Alcance: solo docs oficiales + marca (confirmado por TEAM LEAD). Detalle en seccion "POSTA 9 (COMPLETADA)".

## Objetivo final

Pasar de un sistema de estados CONFIGURABLES por tenant a un catalogo operativo FIJO, administrado por el sistema:

- Pendiente
- En curso
- Finalizado
- Cancelado

El usuario NO elige el estado al crear un turno: el sistema crea siempre el turno como Pendiente. `EstadoTurnoId` sigue siendo NOT NULL en BD (lo asigna el sistema, no el usuario).

## Contexto / por que

- El sistema esta en ETAPA DE PRUEBAS, no en produccion (definido por TEAM LEAD). Los datos actuales de turnos/estados son descartables: NO se disena migracion defensiva para conservarlos.
- La auditoria dejo al descubierto que el CRUD "configurable" ya estaba roto en la practica: todos los estados de los 2 tenants estan soft-deleteados (IsActive=0), el dropdown de estados del form de turno y el CRUD de Configuracion devuelven listas vacias (BaseRepository.GetAll filtra IsActive). Y la logica ya depende por NOMBRE de los estados (strings magicos en TurnoService, HomeService, DashboardService, RecordatorioService).
- El codigo actual ya siembra los 4 estados (TenantService.SeedBaseData); la documentacion D-024 esta desactualizada (dice Pendiente/Confirmado/Atendido). Hay drift docs-vs-codigo a corregir.

## Principio de trabajo (islas pequenas)

- Un objetivo -> pocos archivos -> una decision -> una prueba -> posta segura.
- PROHIBIDO refactorizar fuera del alcance de la posta.
- Cada posta cierra con reporte formal y actualizacion de memoria.
- No se pasa de una posta a otra sin confirmacion del TEAM LEAD.
- Compilar (MSBuild Debug) con 0 errores al terminar cada posta.
- REGLA DE ORO: no crear complejidad para resolver compatibilidad con datos de prueba.

## Decisiones estructurales aprobadas (NO re-discutir)

1. CATALOGO FIJO DE 4 ESTADOS: Pendiente, En curso, Finalizado, Cancelado. Son estados del CICLO DE VIDA del turno, NO estados configurables del negocio.
2. NO se agregan estados: ni No asistio, ni Confirmado, ni Esperando, ni Reprogramado, ni Aviso tarde, ni Llego tarde. La informacion extra, si se necesita a futuro, se modela como EVENTOS, MOTIVOS o ATRIBUTOS del turno (no inflando EstadoTurno).
3. ESTADO vs SITUACION TEMPORAL son conceptos SEPARADOS (regla fundamental):
   - Estado = realidad operativa confirmada por el usuario (se persiste).
   - Situacion temporal = interpretacion del reloj (se CALCULA, no se persiste; nunca es un estado).
4. EL RELOJ NO MUTA EL ESTADO. Nunca Pendiente -> En curso automatico por hora, ni En curso -> Finalizado por hora. El reloj informa (situacion temporal); el operador decide (transicion).
5. EstadoTurnoId se mantiene NOT NULL en BD (no se hace nullable). El sistema asigna Pendiente al crear; el usuario nunca elige el estado.
6. RECONSTRUCCION de BD como opcion preferida por sobre migracion defensiva (mecanismo D-027 ya soportado: auto-migracion + seed). No normalizar estados historicos ni recuperar soft-deleteados.
7. SEED produce exclusivamente los 4 estados, sin adicionar.
8. SEMANTICA POR IDENTIFICADOR/CATALOGO, no por string magico. Se eliminan las dependencias frágiles `Nombre == "Pendiente"/"Cancelado"/...`.
9. METRICAS DE ESTADOS: pertenecen al TENANT (Admin del negocio) y se usan en los dashboards del tenant (D-034). AdminGlobal NO conoce datos de negocio del tenant (aislamiento D-022): solo ve suscripciones/tenants, nunca estados de turno.
10. MIGRACIONES ADITIVAS unicamente (D-027): Add-Migration + publicar; la auto-migracion aplica al arrancar.

## Mapa de postas

| Posta | Contenido | Riesgo | Estado |
|---|---|---|---|
| P1 | Auditoria read-only (radio de explosion + datos reales) | Bajo | COMPLETADA (2026-08-28) |
| P2 | Contrato funcional y reglas de negocio (solo documento) | Bajo | COMPLETADA (2026-08-28) |
| P3 | Limpieza BD / seed limpio (reconstruccion, corregir D-024) | Medio | COMPLETADA (2026-08-28) |
| P4 | Eliminar CRUD de estados (Configuracion) | Medio | COMPLETADA (2026-08-28) |
| P5 | Simplificar Crear / Editar (sin dropdown; UX de acciones) | Alto | COMPLETADA (2026-08-28) |
| P6 | Motor de transiciones centralizado (Iniciar/Finalizar/Cancelar) | Alto | COMPLETADA (2026-08-28) |
| P7 | Situacion temporal / reloj (calculada, NO estado) | Bajo | COMPLETADA (2026-08-28) |
| P8 | Agenda + Dashboard + Home (adaptar superficies, sacar strings) | Medio | COMPLETADA (2026-08-28) |
| P9 | Auditoria final + documentacion (D-024, DECISIONES, DS) | Bajo | COMPLETADA (2026-08-28) |

Orden clave: NO se toca TurnoService como corazon tecnico al inicio. Se fija contrato, se limpia la fuente de datos, se elimina config, se cambia UX, y recien despues se centralizan transiciones. Asi un agente que se detenga a mitad de camino deja decisiones ya cerradas y documentadas.

## Hallazgos POSTA 1 (2026-08-28)

- Archivos que tocan el modelo de estados (~20 sitios, 9 archivos): TurnoService, HomeService, DashboardService, RecordatorioService (filtros por nombre), ConfiguracionService + ConfiguracionController + 4 vistas CRUD, TurnosController (dropdown estados, linea 424), TurnoViewModel y TurnoDetailViewModel, Configuration.cs (seed/backfill), 202608182313336_Inicial.cs (schema FK).
- CalcularEstadoEnVivo (TurnoService) es CODIGO MUERTO: definido sin call sites y sin persistir. No existe ningun proceso automatico de cambio de estado. Se REEMPLAZA por la situacion temporal (P7), no se parchea.
- BD real (JKTurnosDB): todos los estados de ambos tenants soft-deleteados (IsActive=0); 41 turnos, 6 clientes, 14 recursos, 12 servicios, 9 pagos, 4 users, 2 tenants (JK Demo / Gonzo), 1 AvatarPlataforma. Existe un estado "No asistio" (custom, tenant 1) y un "Pendiente" duplicado: datos de prueba descartables.
- Extras a reingresar MANUALMENTE si se reconstruye (configuracion del negocio, no test): LogoUrl del tenant demo https://i.postimg.cc/NMHddC1D/maseta-png.png y AvatarUrl del tenant 2 (mismo link) + cuentas de usuario admin.
- Drift docs: D-024 documenta Pendiente/Confirmado/Atendido; el codigo real (SeedBaseData) ya siembra los 4 estados. Corregir D-024 en P3/P9.

## CONTRATO POSTA 2 (APROBADO por TEAM LEAD 2026-08-28)

Decisiones de reconciliacion cerradas:
- A) CONFIRMAR: se ELIMINA como accion/estado. "Pendiente" deja de significar "sin confirmar" (es "aguarda atencion"). No existe accion Confirmar en la UI (nunca se implemento; solo era label de dashboard/recordatorios).
- B) REAGENDAR: = editar fecha/hora del turno (ya existe en Edit). NO es estado ni atributo nuevo. El turno permanece en su estado.
- C) "SIN CONFIRMAR": se RENOMBRA por semantica neutral (ej. "turnos por atender") en Dashboard y RecordatorioService. Ya no depende del estado "Pendiente" conceptualizado como "no confirmado".

Reconciliacion con documentacion vigente: los docs oficiales (11, 12, 13_DER, 13_AGENDA_UX, 14) describen 6 estados operativos (Pendiente/Confirmado/En curso/Finalizado/Cancelado/Reagendado) y acciones Confirmar/Reagendar. Este contrato los REDUCE a 4 bajo D-040. Actualizar esos Docs en P9 (no antes).

### 1. Catalogo de estados operativos (FIJO)

| Estado | Significado |
|---|---|
| Pendiente | Turno creado, aguarda atencion. Estado inicial por defecto. NO significa "sin confirmar". |
| En curso | La atencion comenzo (operador inicia manualmente). |
| Finalizado | Atencion completada (terminal). |
| Cancelado | No se realizo (terminal; salida desde Pendiente o En curso). |

### 2. Acciones del ciclo (operatorias)

| Accion | Regla |
|---|---|
| Crear | Siempre nace Pendiente. Sin selector de estado. |
| Iniciar | Pendiente -> En curso. Manual. No depende del reloj (permite antes/despues de hora). |
| Finalizar | En curso -> Finalizado. Manual. No automatico por hora de fin. |
| Cancelar | desde Pendiente o En curso. Manual. Requiere motivo. |

### 3. Matriz de transiciones (regla unica en motor P6)

Pendiente -> En curso   SI
Pendiente -> Cancelado  SI
Pendiente -> Finalizado NO
En curso  -> Finalizado SI
En curso  -> Cancelado  SI
En curso  -> Pendiente  NO
En curso  -> Finalizado SI
Finalizado -> cualquier NO (terminal)
Cancelado  -> cualquier NO (terminal)

### 4. Situacion temporal (P7) - calculada, NO estado

Claves estables (compartidas por motor y UI, sin strings dispares):
- "horario-iniciado": Pendiente y ya paso la hora de inicio (la atencion deberia haber comenzado).
- "demorado": Pendiente, paso inicio sin iniciar.
- "tiempo-excedido": En curso y ya paso la hora de fin.
NUNCA transiciones automaticas por reloj: el reloj informa, el operador decide.

### 5. Estados economicos (independientes, D-005/D-028, NO se tocan)

Sin senia / Senia / Parcial / Completo. Calculado desde Pagos. No bloquean transiciones operativas.

### 6. Reagendar (resuelto por B)

Editar fecha/hora del turno en Edit; valida disponibilidad del recurso (logica existente en TurnoService). No cambia estado. No nuevo catálogo.

## POSTA 3 (COMPLETADA 2026-08-28): saneamiento BD / seed / migracion

Cambios aplicados y verificados:

- `Models/Entities/EstadosTurnoBase.cs` (NUEVO): catalogo FIJO con Codigo estable (`PENDIENTE`,`EN_CURSO`,`FINALIZADO`,`CANCELADO`) + Nombre/Color/Icono de display. Patron: `Roles.cs`. Registrado en `.csproj`.
- `Models/Entities/EstadoTurno.cs`: agregada propiedad `Codigo` (semantica por Codigo, no por Nombre - D-040).
- `Services/TenantService.cs` (`SeedBaseData`): siembra el catalogo desde `EstadosTurnoBase.Todos` con `Codigo`.
- `Services/TurnoService.cs`: `Crear` resuelve Pendiente por `Codigo` (fallback `SeedEstadosBase`); `Cancelar` resuelve Cancelado por `Codigo`; `SeedEstadosBase` sembra catalogo con `Codigo` y devuelve Id de Pendiente por `Codigo`.
- `Migrations/Configuration.cs`: rama de backfill legacy LIMPIADA (eliminados casos `"Confirmado"`/`"Atendido"`). El seed de estados ahora usa `EstadosTurnoBase.Todos` con Codigo+Icono, y reconcilia por Codigo si el tenant ya tiene estados. Los turnos demo resuelven el estado por `Codigo` (no por `Nombre`).
- Migracion aditiva `Migrations/202608281509241_AddCatalogoEstados.cs`: `AddColumn Codigo` (D-040). Generada con `ef6.exe` (requiere `--connection-string-name JKTurnosConnection --config Web.config`; la exe NO registra los archivos en csproj, hubo que agregarlos a mano: 3 entries Compile + 1 EmbeddedResource).
- `JKTurnos.csproj`: agregados `EstadosTurnoBase.cs` + los 3 archivos de la migracion.

Verificacion de reconstruccion DESDE CERO (drop `JKTurnosDB` + re-aplicar migraciones + seed):
- Columnas en `dbo.EstadoTurnoes`: `Codigo` y `Icono` presentes.
- Tenant `local` (JK Demo) = EXACTAMENTE 4 estados con Codigo estable: Pendiente/PENDIENTE, En curso/EN_CURSO, Finalizado/FINALIZADO, Cancelado/CANCELADO (con Color e Icono correctos).
- Sin referencias de seed a Confirmado/Atendido/No asistio.
- Build MSBuild Debug: 0 errores.

NOTA / DEVIACION: `Codigo` se agrega por migracion EXPLICITA (`AddCatalogoEstados`). La columna `Icono` sigue viajando por migracion AUTOMATICA residual (`..._AutomaticMigration`) porque `Configuration.cs` tiene `AutomaticMigrationsEnabled = true` (drift vs D-027 que documenta false). Funcionalmente el rebuild produce el schema completo y el seed correcto; el drift de AutomaticMigrationsEnabled queda como pendiente a resolver en una posta posterior (no es bloqueante de P3 ni estaba en su alcance).

NO se toco en P3 (por alcance): `TurnoService.CalcularEstadoEnVivo` (dead code, se reemplaza en P7), CRUD de config de estados, UI Crear/Editar, Agenda, Dashboard, motor de transiciones (P6), situacion temporal (P7).

## POSTA 4 (COMPLETADA 2026-08-28): eliminar CRUD de estados (Configuracion)

Objetivo: quitar la capacidad de configurar estados de turno por tenant (D-040: catalogo FIJO), eliminando el CRUD de estados de la seccion Configuracion.

Cambios aplicados (el catalogo fijo NO se toca: entidad/tabla `EstadoTurno` siguen siendo el modelo de datos del turno, con `Codigo`; solo desaparece la UI de CRUD):

- `Controllers/ConfiguracionController.cs`: eliminadas acciones `EstadoCrear` (GET+POST), `EstadoEditar` (GET+POST), `EstadoEliminar` (GET) + `EstadoEliminarConfirmed` (POST). `Index` ya no recibe el modelo de estados (`return View()`).
- `Services/ConfiguracionService.cs`: eliminados `GetEstados`, `GetEstado`, `CrearEstado`, `ActualizarEstado`, `EliminarEstado` y el helper `NormalizarColor`; eliminado el campo `_estadoRepo` y `using System.Collections.Generic`. El servicio queda solo con datos del negocio + notificaciones + recordatorios ocultos.
- `Views/Configuracion/Index.cshtml`: eliminada la card "Estados de turno" con sus links Editar/Eliminar y el "Nuevo Estado"; eliminada la directiva `@model List<EstadoTurno>`.
- Borrados `Views/Configuracion/EstadoCrear.cshtml`, `EstadoEditar.cshtml`, `EstadoEliminar.cshtml`.
- `JKTurnos.csproj`: removidas las 3 entries `<Content>` de esas vistas.
- Build MSBuild Debug: 0 errores.

No se toco por alcance: el dropdown de estados en `TurnosController` (linea 424) y `TurnoViewModel.EstadosTurno` corresponden a POSTA 5. Coordinacion con P5 (ver Notas): al quedar sin CRUD, el dropdown del form de turno queda sin fuente logica y se completa en P5.

## POSTA 5 (COMPLETADA 2026-08-28): simplificar Crear/Editar sin dropdown; UX de acciones

Objetivo: eliminar el dropdown de estado del form de turno (el estado ya NO lo elige el usuario: lo asigna el sistema, D-040) y reemplazarlo por una card de estado con badge + acciones contextuales. Decision del TEAM LEAD: "Acciones + endpoints en P5" (quitar dropdown de Edit y reemplazar por badge + botones contextuales con endpoints POST; motor simple reusando TurnoService; P6 lo centraliza).

Cambios aplicados:

- `Services/TurnoService.cs`: nuevos metodos operatorios que resuelven por `Codigo` estable (D-040), con guardia de estado actual + notificacion + auditoria (patron de `Cancelar`):
  - `Iniciar(int turnoId)`: Pendiente -> En curso. Guardia: solo si el estado actual es Pendiente, si no `InvalidOperationException("Solo se puede iniciar un turno cuyo estado sea Pendiente.")`.
  - `Finalizar(int turnoId)`: En curso -> Finalizado. Guardia: solo si En curso.
  - `Cancelar(int turnoId)`: ya existia (resuelve Cancelado por `Codigo`). NOTA: `Cancelar` NO tiene guardia de estado origen aun (comportamiento legacy); la matriz de transiciones completa se centraliza en P6.
- `Models/ViewModels/TurnoViewModel.cs`: agregados `EstadoNombre`, `EstadoColor`, `EstadoIcono`, `EstadoCodigo`, `PuedeIniciar`, `PuedeFinalizar`, `PuedeCancelar`; ELIMINADA la propiedad `EstadosTurno` (y su init en el constructor).
- `Controllers/TurnosController.cs`:
  - Edit GET puebla estado actual + flags comparando `turno.EstadoTurno` contra `EstadosTurnoBase.*` por `Codigo`.
  - Edit POST ya NO asigna `turno.EstadoTurnoId = model.EstadoTurnoId` (preserva el estado del turno; las transiciones van por acciones).
  - Eliminada la poblacion de `EstadosTurno` en `PopulateDropdowns`.
  - Nuevos endpoints POST ajax: `IniciarAtencion`, `FinalizarAtencion`, `CancelarAtencion` (patron QuickCreate: `[HttpPost]`+`[ValidateAntiForgeryToken]`+`Json(new { success=..., message=ex.Message })`).
- `Views/Turnos/Edit.cshtml`: reescrito completo — eliminado el dropdown `EstadoTurnoId`; nueva card "Estado del turno" con badge (icono+nombre, color) + botones contextuales condicionados a `Model.PuedeIniciar/PuedeFinalizar/PuedeCancelar`, con `data-url`/`data-turno`, `@Html.AntiForgeryToken()`, y JS que hace fetch POST (token del `input[name=__RequestVerificationToken]` primero del DOM) y recarga la pagina en exito / `showToast(msg,'error')` en fallo; mensaje "no tiene acciones pendientes" si el estado es terminal. Se preserva el resto (quick drawers, loadRecursos por ServicioId).
- `Views/Turnos/Create.cshtml`: sin cambios en P5 (no tenia dropdown de estado).

Verificacion: build MSBuild Debug 0 errores. Ademas verificado END-TO-END en la app (IIS Express + SQL Server Express, sesion admin tenant `local`):
- Edit de turno PENDIENTE muestra "Iniciar atencion" + "Cancelar turno" (no Finalizar).
- Edit de turno EN CURSO muestra "Finalizar" + "Cancelar turno" (no Iniciar).
- POST `IniciarAtencion` (PENDIENTE) -> {"success":true}, BD: PENDIENTE -> EN_CURSO.
- POST `FinalizarAtencion` (EN_CURSO) -> {"success":true}, BD: EN_CURSO -> FINALIZADO.
- POST `IniciarAtencion` sobre FINALIZADO -> {"success":false,"message":"Solo se puede iniciar un turno cuyo estado sea Pendiente."}, BD sin cambios (guardia OK).
- POST `CancelarAtencion` (EN_CURSO) -> {"success":true}, BD: EN_CURSO -> CANCELADO.

No se toco por alcance (P6): centralizar la matriz de transiciones (guardia de estado origen para `Cancelar`, regla unica del motor).

## POSTA 6 (COMPLETADA 2026-08-28): motor de transiciones centralizado

Objetivo: centralizar la matriz de transiciones de estado del turno en un UNICO motor (regla canonica, contrato P2 seccion 3 / D-040). Decision del TEAM LEAD: "Clase TurnoEstadoMotor + delegacion" (TurnoService queda como fachada del turno y delega al motor; el motor es el unico que sabe transicionar estados).

Cambios aplicados:

- `Services/TurnoEstadoMotor.cs` (NUEVO), con la matriz canonica como regla unica:
  - `EsTransicionPermitida(codigoOrigen, codigoDestino)`: Pendiente -> En curso/Cancelado SI; En curso -> Finalizado/Cancelado SI; Pendiente -> Finalizado NO; Finalizado y Cancelado = terminales (sin salida).
  - Transiciones operatorias que resuelven por `Codigo` estable (D-040), con guardia de estado origen + notificacion (`NotificacionSistemaService`) + auditoria (`AuditService`), todo centralizado en `Aplicar(...)`:
    - `Iniciar(turnoId)`: Pendiente -> En curso. Guardia: "Solo se puede iniciar un turno cuyo estado sea Pendiente."
    - `Finalizar(turnoId)`: En curso -> Finalizado. Guardia: "Solo se puede finalizar un turno cuyo estado sea En curso."
    - `Cancelar(turnoId)`: Pendiente/En curso -> Cancelado. **P6 agrega la guardia de estado origen** (antes `Cancelar` no la tenia): "Solo se puede cancelar un turno Pendiente o En curso."
  - `CambiarEstado(turnoId, estadoTurnoId)`: transicion generica por Id de destino, validada contra la matriz canonica (reemplaza y centraliza el `CambiarEstado` generico que estaba en TurnoService, sin call sites previos).
- `Services/TurnoService.cs`: los 4 metodos publicos (`Iniciar`, `Finalizar`, `Cancelar`, `CambiarEstado`) ahora DELEGAN en `_turnoEstadoMotor` (`new TurnoEstadoMotor()`). Se elimino toda la logica duplicada de transiciones que vivia en TurnoService. El resto de TurnoService (Crear/Update/Delete/Get*, disponibilidad, etc.) queda intacto.
- `JKTurnos.csproj`: registrado `Services\TurnoEstadoMotor.cs` (alfabeticamente antes de TurnoService.cs).

Verificacion: build MSBuild Debug 0 errores. Matriz verificada end-to-end en la app (IIS Express + SQL Server Express, sesion admin tenant `local`):
- `IniciarAtencion` (PENDIENTE) -> {"success":true}, BD: PENDIENTE -> EN_CURSO (Pendiente->En curso SI).
- `CancelarAtencion` (EN_CURSO) -> {"success":true}, BD: EN_CURSO -> CANCELADO (En curso->Cancelado SI).
- `CancelarAtencion` de nuevo (CANCELADO) -> {"success":false,"message":"Solo se puede cancelar un turno Pendiente o En curso."}, BD sin cambios (Cancelado terminal, guardia nueva OK).
- `FinalizarAtencion` sobre CANCELADO -> {"success":false,"message":"Solo se puede finalizar un turno cuyo estado sea En curso."} (terminal NO).
- `FinalizarAtencion` sobre PENDIENTE -> {"success":false,"message":"Solo se puede finalizar un turno cuyo estado sea En curso."} (Pendiente->Finalizado NO).
- `IniciarAtencion` sobre PENDIENTE (turno 3) -> {"success":true} (Pendiente->En curso SI).

Nota: `TurnoService._estadoRepo` sigue usado por Crear/SeedEstadosBase; el motor usa sus propios repos (BaseRepository) sobre el mismo JKContext. (`CalcularEstadoEnVivo` ya NO existe: se elimino en P7.)

No se toco por alcance (P7): situacion temporal / reloj (calculada, NO estado) y `CalcularEstadoEnVivo` (dead code), Agenda/Dashboard/Home (P8), Auditoria final + docs (P9).

## POSTA 7 (COMPLETADA 2026-08-28): situacion temporal / reloj (calculada, NO estado)

Objetivo: introducir la SITUACION TEMPORAL como concepto CALCULADO y separado del estado persistido (D-040, contrato P2 seccion 4): "el reloj informa, el operador decide" - el reloj NUNCA muta el estado. Reemplaza el dead code `CalcularEstadoEnVivo` (que intentaba computar estado por reloj y no se usaba).

Decision del TEAM LEAD: "3 claves limpias" (se elimina la clave redundante 'horario-iniciado'; el caso Pendiente+paso la hora de inicio es una sola clave 'demorado').

Cambios aplicados:

- `Models/Entities/SituacionTemporal.cs` (NUEVO), componente ESTATICO y PURO (sin repos; recibe `Turno` + reloj) estilo `EstadosTurnoBase`, con:
  - Claves estables: `Demorado = "demorado"`, `TiempoExcedido = "tiempo-excedido"`, `ATiempo = "a-tiempo"`.
  - `Calcular(Turno turno, DateTime? ahora = null)` -> clave temporal (calculada, no se persiste):
    - Pendiente y ya paso inicio -> "demorado".
    - En curso y ya paso fin -> "tiempo-excedido".
    - Pendiente/En curso en orden -> "a-tiempo".
    - Terminales (Finalizado/Cancelado), turno null o sin estado -> null (estado resolutivo, sin situacion temporal).
    - Semantica por `turno.EstadoTurno.Codigo` (estable, D-040).
  - Helpers: `EsAlarma(clave)` (demorado/tiempo-excedido = alarma) y `De(clave)` -> item de display, y catalogo `Todas` con Label/Color/Icono (para P8 reutilizar UI sin strings dispares).
- `Services/TurnoService.cs`: ELIMINADO el dead code `CalcularEstadoEnVivo` (no tenia call sites; dependia de `Nombre=="En curso"/"Finalizado"` por string magico). Se reemplaza por el concepto de situacion temporal.
- `JKTurnos.csproj`: registrado `Models\Entities\SituacionTemporal.cs`.

Verificacion: build MSBuild Debug 0 errores. Logica pura verificada con harness compilado contra el ensamblado (csc) - 11 checks ALL PASS:
- Pendiente+inicio pasado -> demorado; Pendiente+inicio futuro -> a-tiempo.
- En curso+fin pasado -> tiempo-excedido; En curso+fin futuro -> a-tiempo.
- Finalizado -> null; Cancelado -> null; Turno null -> null.
- EsAlarma(demorado/tiempo-excedido)=true; EsAlarma(a-tiempo)=false; De('demorado').Label='Demorado'.
- Sin referencias residuales a `CalcularEstadoEnVivo` en Services/Controllers/Views. Harness temporal borrado.

Nota de alcance (RESUELTA en P8): la SITUACION TEMPORAL se muestra en las superficies (Agenda/Dashboard/Home/drawer) via `SituacionTemporal.Calcular` + `De/EsAlarma`, sin strings por nombre de estado.

## POSTA 8 (COMPLETADA 2026-08-28): Agenda + Dashboard + Home (adaptar superficies, situacion temporal)

Alcance confirmado por TEAM LEAD ("Nucleo + superficies"): quitar dependencias por nombre de estado, aplicar Contrato C, y pintar la situacion temporal en todas las superficies. Detalle por cambio:

- **Strings magicos por `Nombre` -> `Codigo`** (`Services/`):
  - `HomeService.cs`: `!= "Cancelado"` -> `Codigo != EstadosTurnoBase.Cancelado` (agregado `using JKTurnos.Models.Entities`).
  - `DashboardService.cs`: 3x `!= "Cancelado"` -> `Codigo != EstadosTurnoBase.Cancelado` + `== "Pendiente"` -> `Codigo == EstadosTurnoBase.Pendiente`.
  - `RecordatorioService.cs`: filtros `Nombre == "Pendiente"` -> `Codigo == EstadosTurnoBase.Pendiente` y `!= "Cancelado"` -> `Codigo != EstadosTurnoBase.Cancelado`.
- **Contrato C (semantica neutral "Por atender")** (`RecordatorioService.cs` + `Views/Dashboard/Index.cshtml`): textos/metodo/variables "sin confirmar" -> "por atender"; titulo de card e icono actualizados.
- **Dead data `AgendaViewModel.Estados`**: eliminada la propiedad y su carga en `TurnosController.Agenda` (junto con `_estadoRepo`). La agenda ya no depende del catalogo de estados para pintar blocks.
- **Pintar situacion temporal** (reloj informa, el estado NO muta - D-040):
  - `_AgendaGrid.cshtml` / `_AgendaGridSemanal.cshtml` / `_AgendaMobile.cshtml`: `SituacionTemporal.Calcular(t)` + `EsAlarma/De`; icono de alarma junto al status del block.
  - `Views/Home/Index.cshtml` (chip "Mi dia") y `Views/Dashboard/Index.cshtml` (Proximos turnos / Por atender): indicador de alarma.
  - `TurnoDetailViewModel` + `_TurnoDetailDrawer.cshtml` + `GetDetailDrawer`: nuevos campos `SituacionClave/Label/Color/Icono`, calculados via `SituacionTemporal.Calcular/De` y pintados como icono de alarma junto al badge de estado.

Verificacion: build MSBuild Debug 0 errores. Smoke test de runtime (iisexpress 8089, login seed admin@jkturnos.com/123456): GET / (Home, 200 LEN 24351), /Dashboard (200 LEN 21691), /Turnos/Agenda (dia, 200 LEN 97520), /Turnos/Agenda?vista=semana (200 LEN 66396). Drawer via ajax GetDetailDrawer para turnos 1/2/3: 200, badge con indicador de alarma renderizado (Pendiente->"Demorado" naranja; En curso->"Tiempo excedido" rojo).

## POSTA 9 (COMPLETADA 2026-08-28): auditoria final + documentacion

Alcance confirmado por TEAM LEAD: "Solo docs oficiales + marca" (Recomendado):
cerrar el drift documentacion-vs-codigo dejando el modelo de estados coherente
con D-040, sin reescribir los docs "a futuro".

### Docs oficiales actualizados (catalogo FIJO de 4 estados)

- `Docs/14_TURNO_LIFECYCLE.md`: reescrito a v2.0 — estados operativos
  Pendiente/En curso/Finalizado/Cancelado por `Codigo`; matriz canonica UNICA de
  `TurnoEstadoMotor` (Pendiente->En curso/Cancelado, En curso->Finalizado/
  Cancelado, terminales Finalizado/Cancelado); situacion temporal calculada
  (`demorado`/`tiempo-excedido`/`a-tiempo`); acciones reales Iniciar/Finalizar/
  Cancelar. Eliminados Confirmado/Reagendado y las acciones Confirmar/Reagendar.
- `Docs/12_CORE_DOMAIN_MODEL.md`: seccion 2.5 Estado del Turno -> catalogo fijo
  (no por tenant) con `Codigo`; se agrega la situacion temporal como calculada.
- `Docs/13_DER.md`: seccion 1.7 EstadoTurno -> catalogo fijo con `Codigo`;
  relacion "pertenece a un Tenant" corregida a "catalogo fijo sembrado por
  tenant (D-040)"; se agrega la situacion temporal.
- `Docs/11_ARQUITECTURA_FUNCIONAL.md`: seccion 11.1 estados operativos -> 4
  estados + matriz canonica + situacion temporal; seccion 7.1 acciones
  (Reagendar ya no es cambio de estado, sin estado Reagendado).
- `Docs/13_AGENDA_UX.md`: seccion 14 (Drawer) acciones de estado -> Iniciar/
  Finalizar/Cancelar; nueva subseccion "Indicador de situacion temporal"
  (D-040/P8) en Indiceadores de estado.

### Docs "a futuro" (solo marca de obsoleto, sin reescribir)

- `Docs/16_DRAWER_TURNO_FUTURO.md`: nota al inicio — D-040 invalida las
  acciones V2 `Confirmar`/`Reagendar` y el estado `Reagendado`; la V1 ya pinta
  la situacion temporal (P8). Al retomar V2/V3, alinear con la matriz canonica.

### Decisiones (AI/02_DECISIONES.md)

- `D-024` actualizada: el seed de tenant siembra el catalogo FIJO de 4 estados
  con `Codigo` (Pendiente/En curso/Finalizado/Cancelado) — se corrige la
  referencia antigua a Pendiente/Confirmado/Atendido.
- `D-025` marcada SUPERADA (parcial): la seccion "Estados de Turno" del CRUD de
  configuracion dejo de existir (D-040, CRUD eliminado en P4); solo sobrevive
  la seccion (1) Datos del negocio.
- D-040 ya estaba registrada (VIGENTE); no se re-registra.

### Auditoria final de codigo

Verificada sin referencias residuales:
- Sin strings de estados por `Nombre` en Services (Home/Dashboard/Recordatorio).
- Sin CRUD de estados (no hay `EstadoCrear`/`EstadoEditar`/`EstadoEliminar` ni
  vistas CRUD de estados).
- Transiciones y permisos resueltos por `EstadosTurnoBase` (constantes de
  `Codigo`), nunca por `Nombre`.
- Sin `Confirmado`/`Reagendado`/`Atendido` como estados en el codigo activo.

Nota de alcance: `Docs/15_TURNO_CRUD_TECHNICAL_SUMMARY.md` y
`Docs/DEPLOYMENT_NOTES.md` (entre otros) mencionan "3 estados" / detalle legacy
de host_bootstrap/host_schema; quedan FUERA del alcance de P9 (solo oficiales
+ marca). Se pueden ajustar luego en los "detalles" que indique el TEAM LEAD.

---
## Notas de coordinacion

- P4 (eliminar CRUD) y P5 (quitar dropdown) RESUELTAS conjuntamente: ya no existe dropdown de estado en el form de turno, ni `TurnoViewModel.EstadosTurno`, ni fuente de estados para el dropdown. La UI expone estado como badge + acciones contextuales.
- Edit de turno: el estado SI aparece (es operacion operativa), pero como ACCIONES contextuales ("Iniciar atencion"/"Cancelar" cuando esta Pendiente; "Finalizar"/"Cancelar" cuando En curso), no como dropdown generico. Esto reduce transiciones absurdas.
- P6 RESUELTA: la matriz quedo centralizada en `TurnoEstadoMotor` (regla canonica unica en `EsTransicionPermitida`); `TurnoService` delega; se agrego la guardia de estado origen a `Cancelar`. TurnoService dejo de tener logica de transiciones propia.
- `TurnosController._estadoRepo` dejó de usarse: eliminado junto con la carga de `AgendaViewModel.Estados` en P8.
- P7 RESUELTA: situacion temporal calculada y separada del estado (el reloj informa pero NUNCA muta el estado). `SituacionTemporal` (Models/Entities) es el unico lugar que define las claves temporales (`demorado`/`tiempo-excedido`/`a-tiempo`); `CalcularEstadoEnVivo` fue eliminado. P8 RESUELTA: las superficies (Agenda/Home/Dashboard/drawer) pintan la situacion temporal via `SituacionTemporal.Calcular` + `EsAlarma`/`De` (sin strings por nombre de estado).

## Archivo referenciado

- Plan base de este refactor fue analizado en sesion; este archivo ES la consolidacion del plan aprobado.
- Decisiones relacionadas: D-004 (color del block = recurso, nunca estado), D-005 (estados operativos y economicos independientes), D-022 (aislamiento AdminGlobal), D-024/D-025 (a corregir), D-034 (dashboard tenant), D-037/D-038 (plan suscripciones, ajeno a este refactor).
