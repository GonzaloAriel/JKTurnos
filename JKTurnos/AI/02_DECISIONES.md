# 02_DECISIONES.md

FORMATO: 1.0

## Reglas

Guardar unicamente decisiones tecnicas o de arquitectura que no deben volver a discutirse.

Una decision puede ser reemplazada por otra. Nunca se borra historial: la decision vieja se marca como REEMPLAZADA.

Solo registrar decisiones reales tomadas. No opiniones ni propuestas no aprobadas.

Cuando se registra una decision, indicar el numero siguiente libre (D-001, D-002, etc).

## Indice

| ID | Decision | Fecha | Estado |
|----|----------|-------|--------|
| D-001 | Un Turno usa exactamente un Recurso (obligatorio) | 2026-07-29 | VIGENTE |
| D-002 | La Agenda es el centro/Home. No existe dashboard | 2026-07-29 | VIGENTE |
| D-003 | Clic en Time Block abre Drawer desde la izquierda (nunca pantalla nueva) | 2026-07-29 | VIGENTE |
| D-004 | El color de un Time Block representa el recurso, nunca el estado | 2026-07-29 | VIGENTE |
| D-005 | Estados operativos y economicos son independientes (no se mezclan) | 2026-07-29 | VIGENTE |
| D-006 | El sistema administra Recursos, no Empleados. Modelo agnostico al rubro | 2026-07-29 | VIGENTE |
| D-007 | La Agenda no se persiste: se calcula desde turnos/bloqueos | 2026-07-29 | VIGENTE |
| D-008 | Soft delete obligatorio y filtrado por TenantId (BaseRepository) | 2026-07-29 | VIGENTE |
| D-009 | Textos visibles sin acentos (compatibilidad de encoding) | 2026-07-19 | VIGENTE |
| D-010 | CSS unico del DS v2 (Content/DesignSystem): sin estilos inline | 2026-07-19 | VIGENTE |
| D-011 | Visual de Time Block: banda + fondo pastel del color del recurso; estado = punto; compacto para 30 min | 2026-08-11 | VIGENTE |
| D-012 | Grid de Agenda con posicionamiento explicito; franjas de 30 min; rango 08:00-19:30 | 2026-08-11 | VIGENTE |

## Detalle

### D-001

Fecha: 2026-07-29

Decision: Un Turno posee exactamente un Recurso; el Recurso es obligatorio. Ningun Turno existe sin Cliente, Servicio y Recurso.

Motivo: La disponibilidad se define por recurso; sin recurso no hay reserva posible.

Consecuencias: Turno debe exponer RecursoId. La columna ya existe en BD pero la entidad Turno todavia no lo expone (ver P-001). Sprint 3 debe alinear entity + schema y validar que el servicio no quede inconsistente.

Reemplaza a: ninguna

Estado: VIGENTE

### D-002

Fecha: 2026-07-29

Decision: La Agenda es el Home y el centro del sistema. No existe Dashboard clasico ni listado de Turnos.

Motivo: El usuario administra tiempo; todo nace y vuelve a la Agenda (Docs/11, Docs/13).

Consecuencias: No crear paginas "Listado de Turnos". La entrada principal debe llevar a la Agenda. Home simple.

Reemplaza a: ninguna

Estado: VIGENTE

### D-003

Fecha: 2026-07-29

Decision: Al hacer clic sobre un Time Block se abre el Inspector Drawer desde la izquierda. Nunca se cambia de pantalla.

Motivo: El usuario debe seguir viendo la Agenda mientras analiza un turno (Docs/11, Docs/13).

Consecuencias: El Drawer del inspector es un componente distinto de la Sidebar de navegacion. Al abrirse, la Navigation Sidebar puede minimizarse.

Reemplaza a: ninguna

Estado: VIGENTE

### D-004

Fecha: 2026-07-29

Decision: El color de un bloque de agenda representa el recurso, nunca el estado.

Motivo: Distinguir claramente indispensable (Docs/11 seccion 5, Docs/13 seccion 10).

Consecuencias: El estado se indica con puntos/badges/iconos sutiles, no con el color principal de la tarjeta.

Reemplaza a: ninguna

Estado: VIGENTE

### D-005

Fecha: 2026-07-29

Decision: Los estados operativos (Pendiente...Finalizado) y economicos (Senia/Pago) son grupos independientes; no se mezclan.

Motivo: Un Turno puede estar Finalizado con Pago pendiente (Docs/11, Docs/14).

Consecuencias: La interfaz los muestra por separado. El estado economico no bloquea transiciones operativas.

Reemplaza a: ninguna

Estado: VIGENTE

### D-006

Fecha: 2026-07-29

Decision: El sistema administra Recursos (no Empleados). Todo recurso es un elemento reservable; ningun logica distinta segun rubro.

Motivo: Unico modelo para cualquier negocio basado en tiempo (Docs/11, Docs/12).

Consecuencias: La UI habla de "Recursos". No crear tablas/logica para profesionales, canchas, etc.

Reemplaza a: ninguna

Estado: VIGENTE

### D-007

Fecha: 2026-07-29

Decision: La Agenda no es una entidad; es una vista calculada a partir de turnos y bloqueos. La disponibilidad se obtiene por diferencia, no se persiste.

Motivo: Evitar duplicar informacion que puede calcularse (Docs/13 DER).

Consecuencias: No crear tabla "Agenda" ni "Disponibilidad". Se persisten turnos y bloqueos.

Reemplaza a: ninguna

Estado: VIGENTE

### D-008

Fecha: 2026-07-29

Decision: Soft delete obligatorio (IsActive) y todo acceso a los datos se hace por BaseRepository filtrando TenantId.

Motivo: Aislamiento entre tenants y trazabilidad (Docs/03, Docs/01).

Consecuencias: Toda consulta pasa por BaseRepository; nunca SQL directo salvo necesidad acotada.

Reemplaza a: ninguna

Estado: VIGENTE

### D-009

Fecha: 2026-07-19

Decision: Todos los textos visibles se escriben sin acentos.

Motivo: Compatibilidad de encoding (Docs/01 Seccion Textos, Docs/06 regla 6).

Consecuencias: Al agregar UI, evitar acentos para no romper encoding.

Reemplaza a: ninguna

Estado: VIGENTE

### D-010

Fecha: 2026-07-19

Decision: El estilo del sistema vive en el Design System v2 (Content/DesignSystem/jk-*.css). Sin estilos inline ni por pagina suelta (salvo Content/Pages/jk-{modulo}.css).

Motivo: Consistencia visual y no duplicar HTML (Docs/06, Docs/13).

Consecuencias: No agregar CSS por modulo al componente; usar variables de jk-tokens.css; SSB basea solo reutilizar.

Reemplaza a: ninguna

Estado: VIGENTE

### D-011

Fecha: 2026-08-11

Decision: El Time Block de Agenda se compone de una banda solida con el color del recurso (--jk-block-color) sobre un fondo pastel derivado del mismo color (--jk-block-bg = color-mix(in srgb, var(--jk-block-color) 15%, transparent)). El estado se indica exclusivamente con un punto de color (--jk-status-color). Los turnos de 30 min usan layout compacto (cliente | servicio en una linea) mediante la clase is-compact aplicada server-side.

Motivo: Legibilidad en turnos cortos manteniendo D-004 (el color representa el recurso, nunca el estado).

Consecuencias: Nunca hardcodear colores: usar --jk-block-color, --jk-block-bg y --jk-status-color. La clase is-compact se aplica cuando durationSlots == 1.

Reemplaza a: ninguna

Estado: VIGENTE

### D-012

Fecha: 2026-08-11

Decision: Todos los items del CSS Grid de la Agenda (corner, resource headers, horas, celdas y time blocks) usan posicionamiento explicito (grid-row/grid-column). La grilla usa franjas de 30 min, rango 08:00-19:30, filas fijas de 48px y fila de headers de 44px. Los time blocks se superponen a las celdas de su columna via z-index.

Motivo: El auto-placement del grid esquivaba los time blocks creando huecos que desplazaban filas/columnas (turnos simultaneos deformados y horas mal distribuidas).

Consecuencias: No usar auto-placement en la grilla de Agenda. El rango (gridInicio/gridFin), el tamano de franja (franjaMinutos) y las filas se definen en _AgendaGrid.cshtml.

Reemplaza a: ninguna

Estado: VIGENTE

## Como actualizar

Al tomar una decision nueva, agregar una ficha con el ID siguiente y actualizar el indice.

Al reemplazar una decision, marcar la ficha anterior como REEMPLAZADA por D-XXX y agregar la nueva.

Registrar bajo confirmacion del TEAM LEAD.