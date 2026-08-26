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
| D-009 | Textos visibles sin acentos (compatibilidad de encoding) | 2026-07-19 | REEMPLAZADA por D-032 |
| D-010 | CSS unico del DS v2 (Content/DesignSystem): sin estilos inline | 2026-07-19 | VIGENTE |
| D-011 | Visual de Time Block: banda + fondo pastel del color del recurso; estado = punto; compacto para 30 min | 2026-08-11 | VIGENTE |
| D-012 | Grid de Agenda con posicionamiento explicito; franjas de 30 min; rango 08:00-19:30 | 2026-08-11 | VIGENTE |
| D-013 | Vinculacion Recurso-Servicio N:M editada con checkbox desde ambos CRUDs; creacion rapida de la entidad relacionada en Drawer (POST ajax QuickCreate) SIEMPRE fuera del form principal | 2026-08-12 | VIGENTE |
| D-014 | El Servicio del Turno es OPCIONAL (ServicioId nullable). D-001 sigue vigente respecto al Recurso obligatorio | 2026-08-12 | VIGENTE |
| D-015 | Drawer de detalle del Time Block V1 en solo lectura con slot de estado economico SIEMPRE visible ("Sin senia" como placeholder hasta V3). V2 (acciones de estado/reagendar/editar) y V3 (economico) a futuro en Docs/16 | 2026-08-13 | VIGENTE |
| D-016 | El Drawer de detalle usa un contenedor fijo montado en Agenda.cshtml y el contenido se inyecta via fetch. GetDetailDrawer vive en TurnosController (no se crea AgendaController). `_Drawer.cshtml` (RenderBody) no se reutiliza | 2026-08-13 | VIGENTE |
| D-017 | (CANCELADA 2026-08-13) El FAB "Nuevo Turno" (movil) y el boton del PageHeader en Agenda usaban un Drawer de Quick Create, NUNCA /Create. El plan del Drawer de creacion fue ELIMINADO por TEAM LEAD (sin relevancia por ahora); el boton "Nuevo Turno" de la Agenda navega a /Turnos/Create | 2026-08-13 | CANCELADA |
| D-018 | Vista movil de la Agenda = grilla COMPACTA multi-recurso (media query en jk-agenda.css), NO timeline de un carril: dos recursos con turnos a la misma hora se solaparian en columna unica sin reordenamiento JS | 2026-08-13 | REEMPLAZADA por D-019 |
| D-019 | Vista movil de la Agenda = acordeon de tarjetas por Recurso (details/summary, sin JS) con turnos y horarios LIBRES por recurso. Reemplaza la grilla en <768px; la grilla desktop se oculta | 2026-08-13 | VIGENTE |
| D-020 | Command Center "Mi dia" = carriles por Recurso con chips compactos (dot de estado + hora de inicio + cliente). Color pastel del recurso; el recurso NO se repite en el chip. Scroll horizontal por carril y vertical en la seccion. Clic navega a /Turnos/Agenda | 2026-08-14 | VIGENTE |
| D-021 | KPI "Ingresos" del Command Center = PROYECCION del dia (suma de Servicio.Precio de los turnos del dia excluyendo "Cancelado"); no usa pagos reales (modelo economico V3 a futuro, D-005). Ocupacion = minutos ocupados / 690 (08:00-19:30) | 2026-08-14 | VIGENTE |
| D-022 | Acceso global de plataforma: GlobalRepository filtra solo IsActive (sin TenantId); Add NO asigna TenantId. Uso exclusivo de servicios de plataforma; AdminGlobal (TenantId=0) no ve datos de negocio via BaseRepository | 2026-08-18 | VIGENTE |
| D-023 | Roles como constantes (Roles.cs) + [Authorize(Roles=...)]; sin tabla de permisos. AdminGlobal = TenantId 0 (sin tenant), sin FK Users.TenantId->Tenants | 2026-08-18 | VIGENTE |
| D-024 | Un tenant nuevo nace operativo: TenantService.Create siembra Estados de Turno base (Pendiente/Confirmado/Atendido) + ConfiguracionTenant. TurnoService.Crear con guard si no hay estados | 2026-08-18 | VIGENTE |
| D-025 | Modulo Configuracion de Negocio (solo rol Admin): datos del negocio (ConfiguracionTenant) + CRUD de Estados de Turno del tenant con selector de color. Reglas en ConfiguracionService | 2026-08-18 | VIGENTE |
| D-026 | Logo del negocio por URL: se muestra en el sidebar (marca) y en la bienvenida del Command Center con fallback al icono/omitido si no hay URL. BaseController.OnActionExecuting expone ViewBag.ConfiguracionLogo/Nombre (un query por request) | 2026-08-18 | VIGENTE |
| D-027 | Cadena de migraciones aplastada en una unica migracion Inicial (modelo actual completo) + auto-migracion al arranque: Global.asax.cs usa MigrateDatabaseToLatestVersion. La BD del hosting se actualiza sola en cada deploy (Add-Migration + publicar). RecursoId en Turno sin cascade (configurado en JKContext.OnModelCreating) | 2026-08-18 | VIGENTE |
| D-028 | Modulo Estado Economico (V3): entidad Pago con el estado economico CALCULADO desde los pagos (no persistido en Turno). Sin tope de monto: el precio del servicio es solo referencia. Pago.UsuarioId autocontenido (TurnoHistorial a V2). Admin y Operador registran pagos | 2026-08-19 | VIGENTE |
| D-029 | KPI "Ingresos" del Command Center con pagos reales: suma de Monto de los Pagos con Fecha == hoy (reemplaza la proyeccion de D-021 solo en el calculo de IngresosHoy) | 2026-08-19 | VIGENTE |
| D-030 | Modulo Pagos: listado SOLO LECTURA + filtros (rango de fecha del pago y busqueda por cliente/recurso) + "Anular" con motivo (baja logica, D-008; el registro queda conservado con el motivo en Observacion). Sin edicion ni borrado de montos (registro financiero). Sidebar "Pagos" en grupo Gestion (Admin y Operador). Acceso rapido desde los Time Blocks/cards de la Agenda: el icono de pago abre el Drawer en modo pago (D-003). "Ver turno" desde el listado abre la Agenda y auto-abre el Drawer del turno (parametro openTurnoId). El selector de turnos del registro es un AUTOCOMPLETE ajax (busqueda por cliente/recurso, top 20) en vez de un dropdown estatico (escalable a cientos de turnos) | 2026-08-19 | VIGENTE |
| D-031 | Edicion de pagos: se permite CORREGIR Monto, TipoPago y Observacion de un pago (modifica la clausula "sin edicion" de D-030, pedido del TEAM LEAD). Registro financiero: NO se permite borrar el pago (se anula con motivo, D-008) ni cambiar el Turno o el UsuarioId. Acceso: Admin y Operador | 2026-08-19 | VIGENTE |
| D-032 | Encoding UTF-8 habilitado en todo el proyecto: `<globalization requestEncoding/responseEncoding/fileEncoding="utf-8">` en Web.config + archivos fuente (.cs, .cshtml, .css, .js, .config) guardados en UTF-8 con BOM. Los textos pueden usar ñ y acentos con normalidad. Reemplaza la regla D-009 de "textos sin acentos" | 2026-08-19 | VIGENTE |
| D-033 | Modulo Notificaciones al Cliente (Docs/14 seccion 8), canales por fases sin infraestructura externa al inicio. Fase 1 CONSOLIDADA (2026-08-19): WhatsApp via link `wa.me` con mensaje prellenado (boton "Recordar por WhatsApp" desde el Drawer del turno + plantillas configurables en Configuracion). Fase 2 CONSOLIDADA (2026-08-25): email SMTP — boton "Enviar recordatorio por email" en el Drawer (solo si el cliente tiene email), asunto y cuerpo editables, credenciales SMTP + switch de habilitado en `ConfiguracionTenant`. Fase 3: centro de notificaciones in-app y recordatorios programados. Los disparadores nacen de los eventos del turno (auditoria TurnoHistorial de V2) | 2026-08-19 | VIGENTE (Fases 1 y 2 CONSOLIDADAS) |
| D-034 | Modulo Dashboard en SECCION APARTE (decidido por TEAM LEAD 2026-08-19: nada en el Command Center). Fase 1 CONSOLIDADA (2026-08-19): KPIs del mes, ocupacion semanal (grafica CSS sin librerias), ingresos del mes por dia, proximos turnos y sin confirmar (click abre el drawer en la MISMA seccion via `jk-dashboard.js`), ranking de servicios, cobrado por recurso y saldo pendiente. Fase 2 CONSOLIDADA (2026-08-19): KPI de ingresos con variacion vs mes anterior, evolucion de ingresos 12 meses, ingresos por recurso (barras con %), pagos por tipo (Senia/Parcial/Total) y saldo pendiente. Fase 3: exportacion CSV/PDF | 2026-08-19 | VIGENTE (Fases 1 y 2 CONSOLIDADAS) |
| D-035 | Evolucion visual Fase 1 (isla segura): nueva identidad cromatica calida cambiando SOLO los valores de los tokens existentes en jk-tokens.css (prohibida taxonomia nueva `--jk-color-*`). Regla: accent/success/warning/danger no colapsan semanticamente; colores futuros (coral, mantequilla, dorado) NO se tokenizan sin consumidor real | 2026-08-25 | VIGENTE |
| D-036 | Capa decorativa de marca sistematizada: firma tricolor (`.jk-signature` + `--jk-gradient-tricolor`), chips circulares de icono por accion/feature, brand del sidebar en chip y blobs pastel en bienvenida/login. Se tokenizan los colores de identidad que ya tienen consumidor real: coral, butter, teal (+ lights). Ajuste posterior aprobado por TEAM LEAD: superficies y nav con tinte menta palida (`--jk-surface #F1FAFA`, `--jk-sidebar-bg #D7F0EF`) | 2026-08-25 | VIGENTE |
| D-037 | Plan AG Suscripciones/Pagos de Plataforma/AdminGlobal aprobado: modelo plano sin entidad Subscription v1 (campos sobre Tenant), catalogo estatico de planes en codigo, pago de plataforma = confirmado (sin pendientes), sin notificaciones nuevas ni SignalR (card persistente + dashboard), postas AG-0..AG-10. Plan: AI/SESION_20260826_PLAN_SUSCRIPCIONES.md | 2026-08-26 | VIGENTE |
| D-038 | Sin suspension/bloqueo automatico por vencimiento (posta AG-6 EXCLUIDA): estado derivado de fechas SOLO para mostrar; la unica suspension efectiva es manual. Riesgo aceptado por TEAM LEAD | 2026-08-26 | VIGENTE |

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

Consecuencias: El Drawer de Quick Create de Turno era trabajo a futuro (dependia de resolver P-003 primero; ver Docs/16). La implementacion del FAB visual (CSS) no necesita el Drawer: el boton se puede renderizar y quedarse sin accion hasta que exista el flujo.

Reemplaza a: ninguna

Estado: CANCELADA (2026-08-13, TEAM LEAD: "eliminamos p2, no tiene relevancia por ahora"). El plan del Drawer de Quick Create de la Agenda se descarta. El boton "Nuevo Turno" del PageHeader de la Agenda ahora navega a /Turnos/Create (antes era href="#").

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

Consecuencias: `_AgendaMobile.cshtml` es una variante de render generada en el servidor (no reordena DOM con JS; la grilla queda oculta). NUEVA VARIANTE: ya no es "un DOM + media query" como D-018 (esta decision queda REEMPLAZADA en lo que respecta a la vista movil). El calculo de horarios libres vive en la vista (coherente con "el controller entrega datos, la vista renderiza"). El primer recurso nace expandido (primero del orden alfabetico), el resto colapsado.

Reemplaza a: D-018 (en lo referente a la vista movil)

Estado: VIGENTE

### D-020

Fecha: 2026-08-14

Decision: La seccion "Mi dia" del Command Center agrupa los turnos del dia en CARRILES por Recurso (uno por recurso, apilados). Cada carril tiene un encabezado (dot de color del recurso + nombre + contador) y una fila horizontal de chips compactos (dot de estado + hora de inicio + cliente). El color pastel del chip deriva del color del recurso (`--jk-block-color`, D-011/D-004); el recurso NO se repite dentro del chip porque el carril ya lo identifica. Si un carril desborda en ancho hace scroll horizontal; la seccion tiene max-height 360px con scroll vertical (mobile libera el max-height). El clic en un chip navega a /Turnos/Agenda. El titulo "Mi dia" lleva un icono de sol (acorde a la estetica del sistema).

Motivo: un dia con muchos turnos debe leerse de un vistazo sin repetir datos (el recurso en cada chip ocupaba espacio y no agregaba informacion al estar agrupado); el carril por recurso mantiene la identidad visual de la Agenda sin duplicar la grilla.

Consecuencias: el chip muestra solo hora de inicio (no rango) y cliente. El estado se indica unicamente como punto (D-004). Reemplaza al diseno previo de mini-cards en grilla (descartado por TEAM LEAD).

Reemplaza a: el diseno inicial de "mini-cards Time Block" en grilla (descartado)

Estado: VIGENTE

### D-021

Fecha: 2026-08-14

Decision: El KPI "Ingresos" del Command Center es una PROYECCION del dia: suma de `Servicio.Precio` de los turnos del dia (fecha actual) excluyendo los de `EstadoTurno` "Cancelado". No se usa "pagos registrados" porque el modelo economico (seña/pago) no existe todavia (V3, D-005). La ocupacion se calcula como minutos ocupados / jornada (690 min, rango 08:00-19:30 de D-012) con tope 100%.

Motivo: mostrar valor de negocio sin acoplar la UI a un modelo de pagos inexistente; coherente con D-005 (estado economico independiente del operativo).

Consecuencias: cuando exista V3, el KPI debera recalcularse sobre pagos reales o mantener proyeccion (segun TEAM LEAD). El calculo vive en `HomeService.GetHomeData` (`MinutosJornada = 690`); `TurnosDelDia` incluye todos los turnos del dia (aun cancelados) para la lista "Mi dia".

Reemplaza a: ninguna

Estado: VIGENTE

### D-022

Fecha: 2026-08-18

Decision: Los datos de plataforma (Tenants, cuentas) se acceden SOLO via `GlobalRepository<TEntity>` (`Data/GlobalRepository.cs`), que filtra unicamente por `IsActive` (sin TenantId) y cuyo `Add` NO asigna TenantId (lo setea el llamador). Uso exclusivo de servicios de plataforma (`TenantService`, y la via global de `UserService`). Un AdminGlobal (claim `TenantId = 0`) no pertenece a ningun negocio: a traves de `BaseRepository` obtiene siempre vacio (aislamiento deseado, blinda los modulos operativos).

Motivo: D-008 (soft delete + aislamiento por tenant) aplica a entidades de negocio; las entidades de plataforma no pertenecen a un tenant y `BaseRepository.Add` sobrescribe `TenantId` con el tenant actual.

Consecuencias: Los services de plataforma usan `GlobalRepository` y setean `TenantId` explicitamente. `Tenant` no implementa `ITenantEntity` (no puede usar `BaseRepository`).

Reemplaza a: ninguna

Estado: VIGENTE

### D-023

Fecha: 2026-08-18

Decision: `Role` sigue siendo string; los roles viven como constantes en `Models/Entities/Roles.cs` (`AdminGlobal`, `Admin`, `User`) usadas con `[Authorize(Roles = ...)]` y `User.IsInRole(...)` en vistas. Sin tabla de permisos (RBAC completo a futuro). `AuthService.SignIn` emite `ClaimTypes.Role` y `"TenantId"`. `AdminGlobal` = `TenantId 0` (sin tenant): se elimino la navegacion `User.Tenant` y el FK `Users.TenantId -> Tenants.Id` para permitir `TenantId = 0`.

Motivo: distinguir cuentas de plataforma de cuentas de negocio sin mezclar el aislamiento por tenant.

Consecuencias: `UsuariosController` autoriza `AdminGlobal,Admin` y adapta dropdowns por rol (Admin solo crea rol `User` en su tenant; AdminGlobal elige Tenant y Rol libre). El login del AdminGlobal funciona con `TenantId=0`.

Reemplaza a: ninguna

Estado: VIGENTE

### D-024

Fecha: 2026-08-18

Decision: Al crear un tenant, `TenantService.Create` siembra automaticamente los Estados de Turno base (Pendiente #ffc107, Confirmado #0d6efd, Atendido #198754) y la `ConfiguracionTenant` del negocio. `TurnoService.Crear` tiene guard defensivo: si no existe un estado "Pendiente" lanza un error claro en vez del FK.

Motivo: sin estados base un tenant nuevo no puede crear turnos (el form postea `EstadoTurnoId=0` y choca con `FK_Turnoes_EstadoTurnoes`); los estados no tienen CRUD propio y solo el seed demo los creaba para "local".

Consecuencias: el alta de tenant via CRUD queda operable al instante. Clientes/Servicios/Recursos de un tenant nuevo nacen en blanco (se crean aparte).

Reemplaza a: ninguna

Estado: VIGENTE

### D-025

Fecha: 2026-08-18

Decision: Modulo Configuracion de Negocio accesible solo para rol Admin (tenant). Dos secciones: (1) Datos del negocio = formulario unico sobre `ConfiguracionTenant` (NombreComercial, Telefono, Email, Direccion, LogoUrl) con caja de ayuda para subir el logo gratis (postimages.org / imgbb.com) y pegar la URL directa; (2) Estados de Turno = CRUD del tenant con nombre + color (picker nativo), baja logica (los turnos que ya usan el estado no se pierden). Reglas en `ConfiguracionService`: nombre obligatorio, color con default #6c757d, mutacion de la entidad trackeada (evita el bug de attach de EF). Entrada desde el sidebar (grupo Configuracion, item Negocio) y la accion rapida "Configurar" del Command Center.

Motivo: los estados de turno no tenian CRUD propio (solo el seed los creaba) y la ConfiguracionTenant existia sin pantalla de edicion.

Consecuencias: los estados nuevos aparecen automaticamente en los formularios de turno (mismo repo de tenant). El modulo se mantiene progresivo: no obliga a cargar datos.

Reemplaza a: ninguna

Estado: VIGENTE

### D-026

Fecha: 2026-08-18

Decision: El logo del negocio (URL en ConfiguracionTenant) se muestra en dos lugares: (1) la marca del sidebar (`_Sidebar.cshtml`), reemplazando el icono `bi-calendar-check` cuando hay URL, con fallback al icono si no; (2) la bienvenida del Command Center (`Views/Home/Index.cshtml`), arriba del saludo, centrada en movil. `BaseController.OnActionExecuting` carga la `ConfiguracionTenant` una vez por request y expone `ViewBag.ConfiguracionLogo` y `ViewBag.ConfiguracionNombre` (disponible para todos los roles autenticados). Estilos: sidebar 100px y bienvenida 120px, `object-fit: contain` sin recuadro (el logo se ve completo, sin recortes ni caja).

Motivo: dar identidad visual al negocio; la configuracion es progresiva (no obliga a cargar logo, hay fallback).

Consecuencias: un query ligero de ConfiguracionTenant por request autenticado. Se removio el nombre comercial de la bienvenida (redundante con el logo).

Reemplaza a: ninguna

Estado: VIGENTE

### D-027

Fecha: 2026-08-18

Decision: La cadena de migraciones se reescribio en una unica migracion `Inicial` (modelo actual completo, con su Designer). Se habilito la auto-migracion al arranque: `Global.asax.cs` usa `MigrateDatabaseToLatestVersion<JKContext, Configuration>()`. El esquema de Turno->Recurso queda SIN cascade delete, configurado en `JKContext.OnModelCreating` (`WillCascadeOnDelete(false)`). Migraciones viejas sin Designer (FixCascadeDeleteTurnoRecurso, TurnoServicioOpcional) se eliminaron; sus efectos ya estan en la migracion Inicial.

Motivo: el hosting no permite Update-Database remoto (error 40). Con auto-migracion, cada deploy aplica las migraciones pendientes y el Seed en el primer request, sin scripts SQL manuales ni borrar la BD por cada modulo. El fix previo de cascade era DDL-only; ahora el modelo lo refleja.

Consecuencias: la BD del hosting se vacia UNA vez (primera migrada) y despues se actualiza sola. `AutomaticMigrationsEnabled=false` sigue vigente: si hay cambios de modelo sin migracion, el arranque falla a proposito (obliga a escribir Add-Migration).

Reemplaza a: la configuracion anterior de `Database.SetInitializer<JKContext>(null)` (sin auto-migracion).

Estado: VIGENTE

### D-028

Fecha: 2026-08-19

Decision: Modulo Estado Economico del Turno (V3 del Drawer). Nueva entidad `Pago` (TenantId, TurnoId, Monto, Fecha, TipoPago, Observacion, UsuarioId) en el Dominio Financiero del DER. El estado economico del turno (Sin senia / Senia pagada / Pago parcial / Pago completo) se CALCULA desde los pagos (acumulado vs precio), NO se persiste en `Turno` (respetando "no duplicar info calculable"). El precio del servicio es SOLO REFERENCIA: no hay tope de monto (un servicio puede cobrarse de mas o de menos, y un turno sin servicio acepta pagos). Reglas: `Monto > 0`; la senia se registra una sola vez; no se bloquea por estado operativo (D-005, un turno Cancelado puede recibir retencion/reembolso). `Pago.UsuarioId` guarda el responsable y deja el registro autocontenido; la auditoria `TurnoHistorial` completa se difiere a V2. Registran pagos los roles `Admin` y `User` (Operador), no `AdminGlobal`.

Motivo: cumplir D-005 (independencia operativo/economico) y Docs/16 V3, con decision de TEAM LEAD de no limitar el cobro al precio pactado.

Consecuencias: `TurnoDetailViewModel.EstadoEconomico` (slot de V1) se puebla con el estado real. `TurnosController.RegistrarPago` (POST ajax, patrón QuickCreate de D-013). El KPI de ingresos pasa a D-029. Docs/14 regla "no se puede registrar un pago mayor al precio del servicio" queda sin efecto por decision del TEAM LEAD.

Reemplaza a: la parte de las reglas economicas de Docs/14 que limitaban el monto al precio del servicio.

Estado: VIGENTE

### D-029

Fecha: 2026-08-19

Decision: El KPI "Ingresos" del Command Center se recalcula con PAGOS REALES: `IngresosHoy = sum(Monto)` de los `Pago` cuya `Fecha` cae el dia actual (dinero efectivamente cobrado hoy). Reemplaza la proyeccion de D-021 SOLO en el calculo de `IngresosHoy`; el resto de D-021 (ocupacion, exclusion de Cancelado para la lista "Mi dia") sigue vigente.

Motivo: el usuario quiere saber exacto cuanto gano en el dia, no una proyeccion. Los datos de referencia para un dashboard futuro de pagos se guardaran aparte.

Consecuencias: `HomeService.GetHomeData` consulta pagos del dia (via `PagoService`). `TurnosHoy` y `Ocupacion` no cambian.

Reemplaza a: D-021 en lo referente al calculo de IngresosHoy.

Estado: VIGENTE

### D-032

Fecha: 2026-08-19

Decision: Encoding UTF-8 habilitado en todo el proyecto: `<globalization requestEncoding/responseEncoding/fileEncoding="utf-8">` en Web.config + archivos fuente (.cs, .cshtml, .css, .js, .config) guardados en UTF-8 con BOM. Los textos pueden usar ñ y acentos con normalidad.

Motivo: eliminar la restriccion de escribir sin acentos (que generaba textos pobres) resolviendo el problema de raiz.

Consecuencias: los textos visibles nuevos pueden usar acentos y ñ. La regla D-009 (textos sin acentos) queda sin efecto.

Reemplaza a: D-009

Estado: VIGENTE

### D-033

Fecha: 2026-08-19

Decision: Modulo Notificaciones al Cliente (Docs/14 seccion 8), canales por fases SIN infraestructura externa al inicio. Fase 1: WhatsApp via link `wa.me` con mensaje prellenado — boton "Recordar por WhatsApp" desde el Drawer del turno (y los listados) + plantillas de mensaje configurables por tenant en el modulo Configuracion (roles placeholder: nombre, servicio, fecha, hora). Fase 2 CONSOLIDADA (2026-08-25, aprobado por TEAM LEAD; implementada 2026-08-19): email SMTP con envio real — boton "Enviar recordatorio por email" en el Drawer del turno (solo si el cliente tiene email), asunto y cuerpo editables en Configuracion, credenciales SMTP (servidor, puerto, usuario, password, remitente) + switch de habilitado en `ConfiguracionTenant`. La password no se re-renderiza en el form y en blanco conserva la actual. Fase 3: centro de notificaciones in-app y recordatorios programados. Los disparadores nacen de los eventos del turno: la auditoria `TurnoHistorial` del Drawer V2 (confirmar, cancelar, reagendar, iniciar, finalizar, Docs/14 seccion 8). El cliente se contacta por su Telefono/Email existentes (no requiere registro ni perfil).

Motivo: dar valor de negocio inmediato (reducir no-shows) sin APIs de pago ni jobs de segundo plano; Docs/14 deja los canales "por definir".

Consecuencias: se agrega a `ConfiguracionTenant` el telefono del negocio y las plantillas (o una entidad dedicada), un `NotificacionService` que construye el mensaje y el link `wa.me`, y los botones de accion en la UI. El envio en Fase 1 lo dispara el negocio (abre WhatsApp con el mensaje listo); la automatizacion queda para Fase 3.

Reemplaza a: ninguna

Estado: VIGENTE (Fases 1 y 2 CONSOLIDADAS)

### D-034

Fecha: 2026-08-19

Decision: El Dashboard vive en una SECCION APARTE (modulo propio `Dashboard`, sidebar grupo Principal, `DashboardController` + `Views/Dashboard/Index.cshtml`). El TEAM LEAD decidio que NO se toca el Command Center: nada de graficas ni reportes en el Home. Fase 1 (CONSOLIDADA 2026-08-19): KPIs del mes (turnos, ingresos por pagos, ocupacion de hoy, clientes), grafica de ocupacion semanal y de ingresos del mes por dia (CSS puro, sin librerias), proximos turnos y turnos sin confirmar (cada item abre el drawer en la MISMA seccion via `jk-dashboard.js`), ranking de servicios mas reservados, cobrado por recurso y saldo pendiente (precio - acumulado de pagos de turnos activos del mes). Fase 2 (CONSOLIDADA 2026-08-19): dashboard de pagos — KPI de ingresos con variacion vs mes anterior, evolucion de ingresos por mes (12 meses), ingresos por recurso del mes (barras con %), desglose de pagos por tipo (Senia/PagoParcial/Total) y saldo pendiente. Fase 3: exportacion CSV/PDF.

Motivo: el usuario quiere ver el estado del negocio en una seccion dedicada sin mezclarla con el Home.

Consecuencias: nuevas consultas en `DashboardService` (usa `PagoService.GetPagosEnRango` nuevo) sin tocar `HomeService`. D-002 sigue vigente: el Command Center no cambia.

Reemplaza a: la redaccion previa de D-034 que evolucionaba el Home (nunca implementada).

Estado: VIGENTE (Fase 1 y Fase 2 CONSOLIDADAS)

### D-035

Fecha: 2026-08-25

Decision: Evolucion visual del sistema por fases con POSTAS SEGURAS (isla segura: pocos archivos, reversible, sin logica ni markup). Primera etapa (Fase 1 del roadmap visual, aprobada por TEAM LEAD): cambiar UNICAMENTE los VALORES de los tokens cromáticos existentes en `Content/DesignSystem/jk-tokens.css`. PROHIBIDO crear taxonomia nueva (`--jk-color-bg-body`, `--jk-color-surface-base`, etc.): se conservan los nombres actuales (`--jk-bg`, `--jk-surface`, `--jk-sidebar-bg`, `--jk-text-*`, `--jk-border*`, `--jk-accent*`, `--jk-success*`, `--jk-warning*`, `--jk-danger*`) para no duplicar la fuente de verdad. Nueva direccion: estetica calida retro-contemporanea — crema/marfil/beige (`--jk-bg #F6F1E8`, `--jk-surface #FFFDF8`), sidebar sage muy claro (`#DCE9D7`), texto marron profundo `#4A4038` en lugar de negro absoluto, accent sage profundo `#5F7A5D` (blanco encima ≈ 4.7:1, AA) con hover `#4C624A`, semanticos suavizados (success `#759572`, warning `#D9A441`, danger `#BE5B50`). Los colores de identidad FUTUROS (coral/terracota `#D98272`, amarillo mantequilla `#E8C96A`, dorado, rosa empolvado, sage adicional, colores de graficos) quedan como PALETA DE REFERENCIA: NO se tokenizan hasta que exista un consumidor real (evita tokens huerfanos).

Regla de estados: los colores semanticos NO colapsan entre si ni con la identidad. `accent` = identidad/accion principal; `success` = exito; `warning` = advertencia; `danger` = error/peligro. El futuro coral de identidad NO reemplaza automaticamente a `danger`; se diferencian por luminosidad/saturacion.

Motivo: evolucionar la personalidad visual de la app sin tocar componentes, layouts, Razor ni JS; cambio minimo, verificable visualmente y reversible. El sage `#759572` fue descartado como accent por contraste insuficiente con texto blanco (≈3.3:1, reprueba WCAG AA).

Consecuencias: cada fase posterior (tipografia, radius, sombras, buttons, forms, feedback) exige posta segura y aprobacion del TEAM LEAD antes de avanzar. Al cerrar la paleta deben actualizarse Docs/05_DESIGN_SYSTEM.md, Docs/06_MIGRACION_DS.md y JKTurnosDictionary.md (hoy documentan `--jk-accent #2563eb`). Los hex hardcodeados detectados en inventario NO se corrigen en esta fase (deuda registrada). La Agenda conserva su semantica de color por Recurso (D-004/D-011): los colores de recurso viven en datos, no en tokens.

Reemplaza a: ninguna

Estado: VIGENTE

### D-036

Fecha: 2026-08-25

Decision: Capa decorativa de marca sistematizada dentro del DS, aprobada por TEAM LEAD tras aprobarse el nuevo login. Cuatro dispositivos: (1) FIRMA TRICOLOR — utility `.jk-signature` (pill 72x6 con `--jk-gradient-tricolor`) bajo titulos destacados; aplicada al login y al saludo del Command Center. (2) CHIPS CIRCULARES DE ICONO — iconos de acciones/features en circulos con tinte pastel y glifo oscurecido via `color-mix(...55%, black)`; aplicados a quick actions y features del login; los iconos cuadrados de suggestion cards pasan a circulo. (3) BRAND DEL SIDEBAR EN CHIP — el icono fallback vive en un chip 38px con superficie/borde/sombra (el logo por URL no cambia). (4) BLOBS PASTEL — halos radiales suaves en la bienvenida del Command Center y el panel del login. Se TOKENIZAN los colores de identidad decorativa porque ya tienen consumidores reales (cumple la regla de D-035): `--jk-coral #E88A6F`, `--jk-coral-light`, `--jk-butter #E6C868`, `--jk-butter-light`, `--jk-teal #5FA8AD`, `--jk-teal-light` y `--jk-gradient-tricolor`. Ajuste cromatico aprobado por TEAM LEAD en la misma sesion: superficies y nav toman tinte menta palida (`--jk-surface #F1FAFA`, `--jk-sidebar-bg #D7F0EF`, derivado de #AFEEEE aclarado).

Motivo: dar personalidad alegre y reconocible al sistema sin perder la calma pastel; centralizar la firma/chips como componentes reutilizables en vez de CSS suelto por pagina.

Consecuencias: los nuevos tokens son parte de la fuente unica de verdad (jk-tokens.css). La firma tricolor es utilidad global: cualquier titulo destacado puede usarla. No aplicar blobs ni firma en la Agenda (zona de foco). Docs/05 y Dictionary deben reflejar los tokens nuevos.

Reemplaza a: ninguna

Estado: VIGENTE

### D-037

Fecha: 2026-08-26

Decision: Plan AG - Gestion de Tenants, Suscripciones y Pagos de Plataforma + area Admin Global (plan completo aprobado por TEAM LEAD en AI/SESION_20260826_PLAN_SUSCRIPCIONES.md; postas AG-0..AG-10, una decision por posta con cierre formal). Estructura: (1) MODELO PLANO sin entidad Subscription en v1: la suscripcion son campos sobre Tenant (PlanCodigo, FechaInicioPeriodo, FechaVencimiento; nombres definitivos tras auditoria AG-1); historial = los propios pagos. (2) Catalogo de planes ESTATICO en codigo (patron Roles.cs): Mensual/Trimestral/Semestral/Anual; sin tabla ni CRUD de planes ni hardcodeo en controllers/views. (3) Pago de plataforma como entidad NUEVA separada del Pago de turno (D-028): registrar = confirmar (sin estados pendientes); renovacion anticipada extiende desde el vencimiento vigente. (4) SIN notificaciones nuevas NI SignalR: visibilidad via card persistente de Suscripcion en Configuracion/Negocio (nunca Command Center, D-034) + Dashboard Admin Global. Dias de gracia como constante de plataforma en codigo. Reuso obligatorio: GlobalRepository (D-022), [Authorize(Roles=AdminGlobal)], grupo Sistema del sidebar, migraciones aditivas (D-027).

Motivo: gestion comercial de la plataforma (altas, cobros, control de tenants) sin infraestructura externa (jobs/pasarelas/push) y minimizando riesgo por postas pequenas.

Consecuencias: AG-6 excluida del alcance (ver D-038). Los detalles funcionales finos se congelan en la posta AG-0 dentro del archivo del plan. Nombre definitivo de la entidad de pago de plataforma se fija en auditoria (AG-1/AG-3).

Reemplaza a: ninguna

Estado: VIGENTE (implementacion pendiente)

### D-038

Fecha: 2026-08-26

Decision: NO existe suspension/bloqueo automatico por vencimiento de suscripcion (posta AG-6 EXCLUIDA por TEAM LEAD). El estado de suscripcion (Activo/Por vencer/En gracia/Vencido) se DERIVA de fechas unicamente para MOSTRARLO (cards/dashboard); ningun mecanismo corta el acceso. La unica suspension efectiva es la desactivacion manual del tenant (posta AG-2).

Motivo: .NET Framework 4.7 MVC 5 no ofrece hosted services confiables en el hosting actual; un job diario que falla deja vencidos operando, mientras un chequeo derivado nunca falla. El TEAM LEAD prefiere gestionar vencidos manualmente apoyandose en el Dashboard Admin Global y evitar friccion en el desarrollo.

Consecuencias: RIESGO ACEPTADO por TEAM LEAD: un tenant vencido sigue plenamente operativo hasta desactivacion manual; el Dashboard Admin Global debe volver visible el vencimiento (contadores/alertas). Si en el futuro se desea enforcement, se agrega como posta independiente (gate en BaseController.OnActionExecuting) sin cambiar el modelo.

Reemplaza a: ninguna

Estado: VIGENTE

Al tomar una decision nueva, agregar una ficha con el ID siguiente y actualizar el indice.

Al reemplazar una decision, marcar la ficha anterior como REEMPLAZADA por D-XXX y agregar la nueva.

Registrar bajo confirmacion del TEAM LEAD.