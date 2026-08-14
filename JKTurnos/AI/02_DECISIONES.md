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
| D-013 | Vinculacion Recurso-Servicio N:M editada con checkbox desde ambos CRUDs; creacion rapida de la entidad relacionada en Drawer (POST ajax QuickCreate) SIEMPRE fuera del form principal | 2026-08-12 | VIGENTE |
| D-014 | El Servicio del Turno es OPCIONAL (ServicioId nullable). D-001 sigue vigente respecto al Recurso obligatorio | 2026-08-12 | VIGENTE |
| D-015 | Drawer de detalle del Time Block V1 en solo lectura con slot de estado economico SIEMPRE visible ("Sin senia" como placeholder hasta V3). V2 (acciones de estado/reagendar/editar) y V3 (economico) a futuro en Docs/16 | 2026-08-13 | VIGENTE |
| D-016 | El Drawer de detalle usa un contenedor fijo montado en Agenda.cshtml y el contenido se inyecta via fetch. GetDetailDrawer vive en TurnosController (no se crea AgendaController). `_Drawer.cshtml` (RenderBody) no se reutiliza | 2026-08-13 | VIGENTE |
| D-017 | El FAB "Nuevo Turno" (movil) y el boton del PageHeader en Agenda usan un Drawer de Quick Create, NUNCA la pantalla /Create. La implementacion del Drawer de creacion es posterior; mientras tanto el boton se mantiene deshabilitado o pendiente, no apunta a /Create | 2026-08-13 | VIGENTE |
| D-018 | Vista movil de la Agenda = grilla COMPACTA multi-recurso (media query en jk-agenda.css), NO timeline de un carril: dos recursos con turnos a la misma hora se solaparian en columna unica sin reordenamiento JS | 2026-08-13 | REEMPLAZADA por D-019 |
| D-019 | Vista movil de la Agenda = acordeon de tarjetas por Recurso (details/summary, sin JS) con turnos y horarios LIBRES por recurso. Reemplaza la grilla en <768px; la grilla desktop se oculta | 2026-08-13 | VIGENTE |

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

### D-013

Fecha: 2026-08-12

Decision: La vinculacion Recurso-Servicio es una relacion N:M que se edita con checkbox desde ambos CRUDs (Recursos y Servicios). La creacion rapida de una entidad relacionada (Servicio desde Recurso, Recurso desde Servicio o ambos desde un Turno) se hace en un Drawer con POST ajax (QuickCreate + anti-forgery) que devuelve JSON; el item creado se agrega checked (checkbox) o seleccionado (select). El drawer se renderiza SIEMPRE fuera del form principal.

Motivo: Evitar pantallas intermedias y formularios anidados: un drawer anidado dentro del form principal postea campos sueltos y rompe la validacion del form.

Consecuencias: Los componentes `_ServiceQuickCreateDrawer` y `_RecursoQuickCreateDrawer` tienen doble modo: `ContainerId` (agrega checkbox al listado, uso en CRUDs) o `SelectId` (agrega `<option>` al select y la selecciona, uso en Turnos/Create). Los `QuickCreate` viven en los controllers de la entidad creada y asignan TenantId via BaseRepository.Add.

Reemplaza a: ninguna

Estado: VIGENTE

### D-014

Fecha: 2026-08-12

Decision: El Servicio de un Turno es OPCIONAL (ServicioId nullable, migracion `TurnoServicioOpcional`). El Recurso sigue siendo obligatorio (D-001).

Motivo: El negocio permite turnos sin servicio especfico (ej: "pasar a saludar" o uso del espacio). No debe exigirse un servicio para agendar.

Consecuencias: `TurnoViewModel.ServicioId` es `int?`. El dropdown de servicio muestra placeholder "(opcional)". El filtro de recursos por servicio carga la lista completa cuando no hay servicio (endpoint `TurnosController.GetRecursos()`), sin recargar la pagina.

Reemplaza a: la parte de D-001 que exigia Servicio obligatorio (el resto de D-001 sigue vigente)

Estado: VIGENTE

### D-015

Fecha: 2026-08-13

Decision: El Drawer de detalle del Time Block en la Agenda es V1 SOLO LECTURA (sin acciones de estado). Incluye el slot `TurnoDetailViewModel.EstadoEconomico` como reserva: la seccion de Pagos se muestra SIEMPRE, con el estado economico real o el placeholder "Sin senia" hasta que exista el modelo de pagos (V3). Las iteraciones V2 (acciones de estado, reagendar, editar, auditoria) y V3 (estado economico / senia / pago) quedan a FUTURO y se registran en Docs/16_DRAWER_TURNO_FUTURO.md.

Motivo: Entregar el inspector de turno primero en lectura sin acoplar la UI a modelos que todavia no existen (estado economico), dejando el espacio de la senia visible para que el diseno del Drawer no cambie al llegar el modelo. El alcance V1 fue aprobado por el TEAM LEAD.

Consecuencias: `_TurnoDetailDrawer.cshtml` no contiene botones de accion. `Docs/16_DRAWER_TURNO_FUTURO.md` es la unica fuente de V2/V3. No implementar V2/V3 hasta confirmacion del TEAM LEAD.

Reemplaza a: ninguna

Estado: VIGENTE

### D-016

Fecha: 2026-08-13

Decision: El Drawer de detalle se implementa con un contenedor fijo montado en Agenda.cshtml (`#drawer-turno-detalle` + backdrop + toast) y el contenido se inyecta via fetch desde jk-agenda.js hacia `TurnosController.GetDetailDrawer(int id)`, que devuelve el partial `_TurnoDetailDrawer`. NO se crea un AgendaController (la Agenda vive en TurnosController). El componente `_Drawer.cshtml` (basado en `@RenderBody()`) no se reutiliza para esto.

Motivo: `@RenderBody()` solo funciona con body inline en el render del servidor; inyectar contenido por fetch no tiene body para acoplar. Mantener el controller existente evita duplicar routing y autorizacion.

Consecuencias: La apertura es por click / Enter / Space sobre `.jk-agenda-time-block` (delegacion en jk-agenda.js), con `data-turno-detail-url` en el contenedor. Al cerrar se vacia el body y se devuelve el foco.

Reemplaza a: ninguna

Estado: VIGENTE

### D-017

Fecha: 2026-08-13

Decision: El FAB de mobile y el boton "Nuevo Turno" del PageHeader en la Agenda deben abrir un Drawer de Quick Create de Turno (flujo desde la Agenda), NUNCA navegar a `/Turnos/Create` (pantalla nueva, contradice D-002/D-003). Mientras no exista el Drawer de creacion, la accion queda pendiente (el boton no debe apuntar a /Create ni ser un link muerto activo).

Motivo: Regla de Agenda: "nunca cambiar de pantalla" y "el usuario no debe salir de la Agenda para operar su dia". Un FAB que mande a /Create repite el mismo defecto que el `href="#"` actual.

Consecuencias: El Drawer de Quick Create de Turno es trabajo a futuro (depende de resolver P-003 primero; ver Docs/16). La implementacion del FAB visual (CSS) no necesita el Drawer: el boton se puede renderizar y quedarse sin accion hasta que exista el flujo.

Reemplaza a: ninguna

Estado: VIGENTE

### D-018

Fecha: 2026-08-13

Decision: La vista movil de la Agenda (P1) se implementa como grilla COMPACTA multi-recurso via media query (max-width 767px) en `Content/Pages/jk-agenda.css`, sobre el mismo DOM `_AgendaGrid`. NO se adopta el timeline de un carril (columna unica + columna de hora) porque dos recursos con turnos a la misma hora se solaparian en una sola columna sin reordenamiento JS, rompiendo la legibilidad.

Motivo: AGENDA_UX seccion Mobile especifica "Time Blocks mas compactos" (la grilla se mantiene, no se reemplaza por lista). La grilla compacta conserva la posicion temporal (la fila de 44px mantiene 60 min = 2 franjas) y el mismo drawer de detalle sin JS.

Consecuencias: columna de horas sticky reducida a 40px, columnas de recurso angostas (minmax 128px, scroll horizontal), filas de 44px touch-friendly, tarjetas mas compactas. Sin JS de resize, sin duplicar DOM. Timeline unico queda como iteracion posterior si el TEAM LEAD decide aceptar el solapamiento o un reordenamiento JS.

Reemplaza a: ninguna

Estado: REEMPLAZADA por D-019

### D-019

Fecha: 2026-08-13

Decision: La vista movil de la Agenda (< 768px) se implementa como acordeon de tarjetas por Recurso usando `<details>/<summary>` (sin JS obligatorio). Cada recurso es una tarjeta desplegable con su nombre y conteo de turnos; al expandirse muestra los turnos del dia (reutilizando `.jk-agenda-time-block` y el mismo Drawer de detalle, D-003/D-016) y los HORARIOS LIBRES calculados (franjas de 30 min desocupadas 08:00-19:30, fusionadas en rangos continuos, renderizadas como chips "Libre HH:mm - HH:mm"). La grilla desktop (`_AgendaGrid`) se oculta en <768px via media query.

Motivo: evitar el scroll lateral y el solapamiento de columnas en movil (multi-recurso, turnos a la misma hora), priorizar el recurso como eje y mostrar disponibilidad (horarios libres) de forma explicita. El acordeon por recurso hace explícito lo que en la grilla era una celda vacia (sin logica).

Consecuencias: `_AgendaMobile.cshtml` es una variante de render generada en el servidor (no reordena DOM con JS; la grilla queda oculta). NUEVA VARIANTE: ya no es "un DOM + media query" como D-018 (esta decision queda REEMPLAZADA en lo que respecta a la vista movil). El calculo de horarios libres hoy vive en la vista (coherente con "el controller entrega datos, la vista renderiza"); cuando D-017 (quick-create) lo necesite se extrae a un metodo de servicio reutilizable. El primer recurso nace expandido (primero del orden alfabetico), el resto colapsado.

Reemplaza a: D-018 (en lo referente a la vista movil)

Estado: VIGENTE

Al tomar una decision nueva, agregar una ficha con el ID siguiente y actualizar el indice.

Al reemplazar una decision, marcar la ficha anterior como REEMPLAZADA por D-XXX y agregar la nueva.

Registrar bajo confirmacion del TEAM LEAD.