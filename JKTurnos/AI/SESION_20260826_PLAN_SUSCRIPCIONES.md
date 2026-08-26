# SESION 2026-08-26: PLAN AG - SUSCRIPCIONES, PAGOS DE PLATAFORMA Y ADMIN GLOBAL

ESTADO: PLAN EN PROGRESO (AG-0..AG-5, AG-7, AG-8 implementadas; AG-6 EXCLUIDA; AG-9/AG-10 pendientes).

Decisiones marco: D-037 (estructura y alcance) / D-038 (sin suspension automatica; AG-6 EXCLUIDA).

Proximo paso: POSTA AG-9 (Dashboard Admin Global minimo) o confirmacion del TEAM LEAD sobre AG-1..AG-8.

AG-0 CONGELADA (2026-08-26): ver seccion "AG-0 ESPECIFICACION FUNCIONAL" al final de este archivo.

## Objetivo final

El AdminGlobal tendra un centro de gestion desde el cual podra:

- Activar / desactivar tenants (SOLO manual, D-038).
- Registrar pagos de suscripcion y asociarlos a un plan.
- Ver el estado de cada suscripcion: plan, inicio, vencimiento, tiempo restante y estado (Activo / Por vencer / En gracia / Vencido / Suspendido manual).
- Ver alertas operativas (en gracia, vencidos, vencen esta semana, pagos del periodo).

El Tenant ve su propia suscripcion en una card persistente (Configuracion/Negocio).

## Principio de trabajo (islas pequenas)

- Un objetivo -> pocos archivos -> una decision -> una prueba -> posta segura.
- PROHIBIDO refactorizar fuera del alcance de la posta.
- Cada posta cierra con reporte formal y actualizacion de memoria.
- No se pasa de una posta a otra sin confirmacion del TEAM LEAD.
- Compilar (MSBuild Debug) con 0 errores al terminar cada posta.

## Decisiones estructurales aprobadas (NO re-discutir)

1. MODELO PLANO: NO se crea entidad Subscription en v1. La suscripcion son campos sobre Tenant: PlanCodigo, FechaInicioPeriodo, FechaVencimiento (nombres definitivos tras auditar el modelo en AG-1). El historial contractual son los propios pagos.
2. CATALOGO DE PLANES ESTATICO EN CODIGO (patron Roles.cs): Mensual (1 mes), Trimestral (3), Semestral (6), Anual (12). Sin tabla, sin CRUD de planes, sin hardcodear valores en controllers/views.
3. PAGO DE PLATAFORMA separado del Pago de turno (D-028): entidad NUEVA (nombre de trabajo PagoSuscripcion; definitivo en auditoria AG-3). Registrar = confirmar: SIN estados pendientes. El pago es un evento financiero; nunca se mezcla con el estado de la suscripcion.
4. RENOVACION ANTICIPADA: si el tenant tiene vigencia futura, nuevo vencimiento = vencimiento vigente + duracion del plan (nunca hoy + duracion). Si ya vencio, nuevo vencimiento = hoy + duracion.
5. DIAS DE GRACIA: constante de plataforma en codigo (junto al catalogo de planes). NUNCA en ConfiguracionTenant (esa configuracion es del Admin del tenant).
6. SIN NOTIFICACIONES NUEVAS NI SIGNALR (descartado definitivamente para este modulo): la visibilidad se logra con (a) card persistente de Suscripcion en Configuracion/Negocio (NUNCA en el Command Center, D-034) y (b) Dashboard Admin Global. Opcional posterior: email al confirmar pago reutilizando el patron SMTP de D-033.
7. ESTADO DERIVADO SOLO PARA MOSTRAR: Activo/Por vencer/En gracia/Vencido se calculan desde fechas en server (services/viewmodels). Nunca se persiste un estado transicionado ni existe maquina de estados.
8. SIN BLOQUEO AUTOMATICO (D-038): la unica suspension efectiva es manual (posta AG-2). Riesgo aceptado por TEAM LEAD: un tenant vencido sigue operativo hasta desactivacion manual; el Dashboard Admin Global debe volverlo visible (contadores/alertas). Si en el futuro se desea enforcement, se agrega como posta independiente sin cambiar el modelo.
9. SEGURIDAD: controllers nuevos con [Authorize(Roles = Roles.AdminGlobal)] y acceso exclusivo via GlobalRepository (D-022). Un tenant JAMAS puede activarse, cambiar su plan, registrar un pago propio ni modificar su vencimiento. AdminGlobal sigue sin ver datos de negocio via BaseRepository (D-022).
10. MIGRACIONES ADITIVAS unicamente (D-027): Add-Migration + publicar; la auto-migracion aplica al arrancar.
11. REUSO OBLIGATORIO: extender TenantsController/TenantService y el grupo Sistema del sidebar; no duplicar CRUD ni vistas.

## Mapa de postas

| Posta | Contenido | Riesgo | Estado |
|---|---|---|---|
| AG-0 | Especificacion funcional (solo documento; congela reglas) | Bajo | CONGELADA (2026-08-26) |
| AG-1 | Modelo plano + catalogo de planes + migracion aditiva | Medio | IMPLEMENTADA (2026-08-26, pendiente confirmacion) |
| AG-2 | Activar/desactivar tenant (manual) + semantica de suspension | Medio | IMPLEMENTADA (2026-08-26, pendiente confirmacion) |
| AG-3 | Entidad PagoSuscripcion + migracion aditiva | Bajo | IMPLEMENTADA (2026-08-26, pendiente confirmacion) |
| AG-4 | Registrar pago -> extender vencimiento (transaccional, service) | Alto | IMPLEMENTADA (2026-08-26, pendiente confirmacion) |
| AG-5 | Card Suscripcion lado Tenant | Bajo | IMPLEMENTADA (2026-08-26, pendiente confirmacion) |
| AG-6 | EXCLUIDA por TEAM LEAD (bloqueo automatico). Ver D-038 | - | EXCLUIDA |
| AG-7 | Shell Admin Global + grilla Tenants (reusa existente) | Medio | IMPLEMENTADA (2026-08-26, pendiente confirmacion) |
| AG-8 | Gestion de pagos UI (expone el flujo AG-4) | Alto | IMPLEMENTADA (2026-08-26, pendiente confirmacion) |
| AG-9 | Dashboard Admin Global minimo | Bajo | PENDIENTE |
| AG-10 | Prueba integral + matriz de seguridad | Alto | PENDIENTE |

Nota: el estado derivado para MOSTRAR (regla 7) se implementa dentro de la posta donde aparece primero (AG-5 card / AG-7 grilla / AG-9 dashboard), no como posta aparte.

## Detalle minimo por posta

### AG-0 - Especificacion funcional

Salida: seccion "AG-0 ESPECIFICACION FUNCIONAL" congelada AL FINAL de este archivo, tras aprobacion del TEAM LEAD. No se toca Views/CSS/Controllers/DB.

Checklist a congelar:

1. Umbrales exactos del estado mostrado (dias de aviso "por vencer", dias de gracia) y formato de "tiempo restante".
2. Semantica exacta de Desactivar/Activar manual: relacion con la baja logica existente (IsActive) vs suspension operativa; que exige Activar sobre un tenant vencido.
3. Que muestra/queda un tenant recien creado sin pago (propuesta: nace "Sin plan", operativo, FechaVencimiento null).
4. Backfill de tenants existentes (vencimiento null = "Sin plan").
5. Campos finales del pago de plataforma (monto, fecha, plan, referencia/observacion, usuario que registra).
6. Importe sugerido por plan en el formulario de pago (editable).
7. Moneda unica y convencion de fechas (hora local, misma convencion que Turnos).
8. Cambio de plan: solo via nuevo pago registrado (sin prorrateo).
9. FUERA DE ALCANCE declarado: pasarelas de pago, multi-moneda, facturacion, prorrateo, estados PENDING/CANCELLED, multi-plan simultaneo.

### AG-1 - Modelo plano + catalogo

Auditar Tenant/BD -> agregar campos de suscripcion + clase estatica de planes (codigo, nombre, meses) y constante DiasGraciaPlataforma. Migracion aditiva. Verificar: mensual/trimestral/semestral/anual calculan fecha correcta.

### AG-2 - Activar/desactivar manual

Accion de AdminGlobal sobre un tenant (service + endpoint). Activar/Desactivar NO toca historial ni montos. Campos segun lo congelado en AG-0 punto 2. Verificacion: activar/desactivar y confirmar que nada mas cambia.

### AG-3 - Entidad PagoSuscripcion

Modelo + migracion aditiva. Sin UI. Verificacion: alta de prueba via servicio.

### AG-4 - Flujo de pago (transaccional)

RegistrarPago(tenantId, plan, monto, ...) en service: crea el pago + recalcula el vencimiento (regla 4) en UNA transaccion. Verificacion: renovacion anticipada conserva dias; pago sobre vencido parte de hoy.

### AG-5 - Card Suscripcion (Tenant)

Card en Configuracion/Negocio con plan/estado/inicio/vencimiento/tiempo restante y avisos (vence pronto / en gracia / vencido). Solo lectura; el server entrega el estado.

### AG-7 - Shell Admin Global + grilla Tenants

Seccion AdminGlobal (sidebar grupo Sistema): resumen minimo + tabla Tenants (estado, plan, vencimiento, tiempo, acciones: Ver / Activar / Desactivar / Registrar pago). Acciones peligrosas con confirmacion. Reusa TenantsController/TenantService/GlobalRepository.

### AG-8 - Gestion de pagos

Formulario de registro de pago (tenant, plan, importe, fecha, referencia) que ejecuta el flujo AG-4; listado de pagos de plataforma con filtros basicos.

### AG-9 - Dashboard Admin Global minimo

Contadores (activos, en gracia, vencidos, suspendidos manuales), "vencen esta semana" y ultimos pagos. CSS del DS (tokens --jk-*); graficas NO requeridas.

### AG-10 - Prueba integral + seguridad

Caso feliz completo, renovacion anticipada, activacion manual, backfill "Sin plan". Matriz de seguridad: un tenant no ve rutas AdminGlobal ni muta su suscripcion; AdminGlobal sigue sin ver datos de negocio (D-022).

## Reporte de cierre (obligatorio por posta)

POSTA AG-X - CIERRE
- Archivos creados / modificados
- Archivos NO modificados (explicitos)
- Cambios
- Reglas implementadas
- Pruebas
- Regresiones
- Deuda pendiente
- Siguiente posta autorizada (requiere confirmacion del TEAM LEAD)

Mas actualizacion de AI/00_MEMORIA.md, AI/01_ESTADO.md y, si corresponde, AI/02_DECISIONES.md.

---

## AG-0 ESPECIFICACION FUNCIONAL

ESTADO: CONGELADA (aprobada por TEAM LEAD, 2026-08-26). Fuente unica de las reglas funcionales del modulo. Cambios solo con nueva decision del TEAM LEAD.

Base auditada: `Tenant` hereda BaseEntity (Id/CreatedAt/UpdatedAt/IsActive) y solo tiene Nombre + Subdominio. La baja logica actual (Tenants/Delete -> IsActive=false) NO se toca: la suspension es un concepto distinto.

### 1. Estados de suscripcion (derivados, solo para mostrar)

Se calculan en server (service) desde las fechas. Precedencia:

| # | Estado | Regla |
|---|--------|-------|
| 1 | SUSPENDIDO MANUAL | `SuspendidoManual = true` (prioridad maxima) |
| 2 | SIN PLAN | Sin `PlanCodigo` y sin `FechaVencimiento` |
| 3 | VENCIDO | hoy > Vencimiento + DiasGracia |
| 4 | EN GRACIA | Vencimiento < hoy <= Vencimiento + DiasGracia |
| 5 | POR VENCER | Vencimiento - DiasAviso <= hoy <= Vencimiento |
| 6 | ACTIVO | resto |

Constantes (codigo, junto al catalogo de planes): `DiasAvisoPorVencer = 7`, `DiasGraciaPlataforma = 5`.

Tiempo restante: "N dias restantes"; en gracia: "Vencido hace N dias".

Comparacion por DIA calendario (hora local, misma convencion que Turnos): el dia del vencimiento aun cuenta como vigente; la gracia empieza al dia siguiente.

### 2. Activar / Desactivar manual

- Campo nuevo sobre Tenant: `SuspendidoManual` (bool). Distinto de IsActive (que sigue siendo la baja logica del registro).
- DESACTIVAR: setea `SuspendidoManual = true`. NO bloquea acceso (D-038); es estado de gestion visible en grilla/dashboard. Si se necesita cortar el uso real de un tenant: desactivar sus usuarios desde el modulo Usuarios ya existente.
- ACTIVAR: setea `SuspendidoManual = false`. NO modifica fechas ni pagos. Para extender vigencia se registra un pago (punto 5); cortesias con Monto 0.
- Sin tabla de auditoria de activaciones en v1 (deuda registrada).

### 3. Tenant recien creado

Nace SIN PLAN: `PlanCodigo`, `FechaInicioPeriodo`, `FechaVencimiento` en null. Operativo igual que hoy (D-024 intacto): no se exige pago para operar. Card/dashboard muestran "Sin plan asignado".

### 4. Backfill de tenants existentes

La migracion agrega columnas nullables -> los existentes quedan automaticamente SIN PLAN. No se asignan planes ni fechas retroactivos.

### 5. Entidad PagoSuscripcion (nombre de trabajo)

Campos: Id, TenantId (obligatorio), PlanCodigo (string), Monto (decimal >= 0), Fecha (DateTime, hora local), Observacion (string opcional, max 500; unifica referencia/comprobante), UsuarioId (AdminGlobal que registra), + BaseEntity (IsActive/CreatedAt/UpdatedAt).

Reglas:
- Registrar = confirmado (sin estados pendientes). Registro financiero: SIN edicion ni baja en v1; una correccion = nuevo pago ajustativo (deuda: anulacion con recálculo).
- Recalculo de vigencia (transaccional con la creacion del pago):
  - Si Vencimiento >= hoy: nuevo Vencimiento = Vencimiento + meses del plan (AddMonths).
  - Si no (vencido/en gracia/sin plan): nuevo Vencimiento = hoy + meses del plan.
  - `FechaInicioPeriodo` = fecha del pago; `PlanCodigo` del Tenant = plan de ese pago (el ultimo pago manda).

### 6. Catalogo de planes (clase estatica en codigo)

| Codigo | Nombre | Meses | Precio sugerido |
|---|---|---|---|
| MENSUAL | Mensual | 1 | 0 |
| TRIMESTRAL | Trimestral | 3 | 0 |
| SEMESTRAL | Semestral | 6 | 0 |
| ANUAL | Anual | 12 | 0 |

El precio sugerido precarga el formulario de pago y queda SIEMPRE editable. Valores en 0 por decision del TEAM LEAD (2026-08-26); se definen despues editando la clase estatica, sin migracion.

### 7. Moneda y fechas

Moneda unica (sin campo moneda; formato $ en UI). Fechas en hora local server (misma convencion que Turnos).

### 8. Cambio de plan

Solo via nuevo pago registrado. La suscripcion siempre refleja el ultimo pago (acumula dias aunque cambie de plan). Sin prorrateo.

### 9. Fuera de alcance (declarado)

Pasarelas de pago; multi-moneda; facturacion; prorrateo; estados PENDING/CANCELLED; multi-plan simultaneo; bloqueo automatico de acceso (D-038); recordatorios automaticos programados; auditoria de activaciones/desactivaciones; email automatico al confirmar pago (fase posterior opcional reutilizando el patron SMTP de D-033); anulacion/edicion de pagos de plataforma.

### Resolucion de puntos abiertos (TEAM LEAD, 2026-08-26)

1. Umbrales: aviso "por vencer" 7 dias / gracia 5 dias (aprobado).
2. Desactivar = informativo puro (aprobado): sin bloqueo de acceso; corte real de uso = desactivar usuarios desde el modulo Usuarios existente.
3. Precios sugeridos: en 0 por ahora; se definen despues editando la clase estatica (aprobado).
4. Pago con un solo campo Observacion (aprobado); no se crea campo Referencia separado.
