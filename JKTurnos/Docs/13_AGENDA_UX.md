# AGENDA UX

## Manual de Experiencia de Usuario de la Agenda de JKTurnos

Version: 1.0

Fecha: 29/7/2026

Estado: Aprobado

---

# Preambulo

Este documento responde una sola pregunta:

**¿Como debe sentirse y funcionar la Agenda?**

No habla de entidades.

No habla de base de datos.

No habla de logica interna.

Habla unicamente de experiencia de usuario.

La Agenda es la pantalla mas importante del sistema.

Todo lo demas es secundario.

---

# 1. Filosofia de la Agenda

La Agenda no es una pantalla del sistema.

La Agenda es el sistema.

Cuando un usuario abre JKTurnos debe sentir que abre su dia.

No que abre un software.

La Agenda debe transmitir:

- claridad
- control
- calma
- velocidad

El usuario debe poder responder una sola pregunta mirando la Agenda:

**¿Que tengo hoy?**

No necesita buscar informacion.

No necesita navegar a otra seccion.

La informacion esta ahi, frente a el.

La Agenda es un espejo de su jornada laboral.

Si el dia esta despejado, la Agenda se ve despejada.

Si el dia esta ocupado, la Agenda se ve ocupada.

Pero nunca debe verse caotica.

---

# 2. La Agenda como Home del sistema

No existe un Dashboard tradicional.

Cuando el usuario inicia sesion, la Agenda es lo primero que ve.

No hay pantalla de bienvenida.

No hay metricas de inicio.

No hay graficos.

Hay una Agenda.

El usuario debe poder crear, mover y consultar turnos sin abandonar la Agenda.

La Agenda es el centro operativo.

Todo surge desde ella.

Todo regresa a ella.

---

# 3. Layout general

La pantalla de la Agenda se divide en tres zonas:

## Zona superior

Navegacion temporal.

Contiene:

- Boton "Hoy" para volver al dia actual
- Flechas de navegacion (anterior / siguiente)
- Fecha o rango visible en formato claro
- Selector de vista (Dia, Semana, Mes)

## Zona izquierda

Selector de recursos.

Lista vertical de recursos del negocio.

Cada recurso aparece con su color identificativo y su nombre.

El usuario puede mostrar u ocultar recursos individuales.

## Zona central (principal)

La cuadricula de tiempo.

Ocupa la mayor parte de la pantalla.

Es la unica zona que realmente importa.

---

# 4. Time Blocks

Cada bloque de tiempo en la Agenda es una pequena tarjeta.

No existen filas de tabla.

No existen listas verticales.

Existen tarjetas dentro de una cuadricula de tiempo.

## Estructura visual de la tarjeta

**Banda superior** (color del recurso)

Ocupa el ancho completo de la tarjeta.

Incluye:

- La hora de inicio y fin
- Un menu de acciones en el extremo derecho (tres puntos)

**Cuerpo**

- Nombre del cliente
- Nombre del servicio
- Recurso (solo si hay mas de uno)

## Comportamiento

- Hover: la tarjeta se eleva levemente
- Clic: abre el Drawer izquierdo
- Arrastrar: permite mover el bloque a otro horario
- Arrastrar desde el borde inferior: permite redimensionar la duracion

## Altura

La altura de la tarjeta debe representar visualmente la duracion del bloque.

Un bloque de 60 minutos debe ocupar el doble de espacio que uno de 30 minutos.

La relacion debe ser proporcional y precisa.

---

# 5. Drawer izquierdo

Al hacer clic sobre un Time Block, nunca se cambia de pantalla.

Siempre se abre un Drawer desde la izquierda.

## Reglas del Drawer

- Aparece sobre la Agenda, no la reemplaza
- La Agenda permanece visible a la derecha
- El Navigation Sidebar se minimiza automaticamente si es necesario
- El Drawer tiene un ancho fijo que no debe superar el 40% del viewport
- Se cierra al hacer clic fuera de el o al presionar Escape

## Contenido del Drawer

El Drawer se divide en secciones verticales:

**Encabezado**

- Nombre del cliente
- Estado actual del turno (indicador visible)
- Boton de cerrar

**Cuerpo**

- Datos del cliente (nombre, telefono, email)
- Servicio y recurso asignado
- Fecha y hora
- Observaciones
- Historial de cambios (Timeline)

**Acciones**

- Botones de accion segun el estado actual
- Las acciones aparecen en la parte inferior del Drawer
- La accion principal esta destacada

---

# 6. Sidebar

Existen dos paneles laterales distintos.

## Navigation Sidebar

Menu principal del sistema.

Contiene la navegacion entre modulos.

En desktop permanece visible (minimizable).

Cuando el Drawer se abre, el Navigation Sidebar se minimiza automaticamente
para liberar espacio.

## Inspector Drawer

Panel contextual que se abre al hacer clic en un Time Block.

Siempre aparece desde la izquierda.

Sobre el Navigation Sidebar minimizado.

No son el mismo componente.

No deben confundirse.

---

# 7. FAB (Floating Action Button)

## Desktop

No existe FAB en desktop.

Las acciones principales estan accesibles desde la interfaz.

## Mobile

Aparece un FAB en la esquina inferior derecha.

Permite crear un nuevo turno rapidamente.

Al presionarlo, abre un modal o Drawer ligero con los campos minimos:

- Nombre del cliente
- Telefono
- Servicio (selector)
- Recurso (autopreseleccionado si es unico)

---

# 8. Navegacion entre dias y semanas

## Vista Dia

Muestra las horas del dia seleccionado en la columna central.

Cada hora tiene una altura fija y proporcionada.

El usuario puede hacer clic en una hora vacia para crear un turno en ese
momento.

## Vista Semana

Muestra los siete dias de la semana en columnas.

Cada columna representa un dia.

Los Time Blocks se distribuyen en la columna del dia correspondiente.

## Vista Mes

Muestra el mes completo en cuadricula.

Cada dia muestra indicadores de ocupacion.

Al hacer clic en un dia, la Agenda cambia automaticamente a vista Dia.

## Navegacion

Flechas izquierda/derecha: avanzan o retroceden segun la vista actual.

Boton "Hoy": vuelve al dia actual en cualquier vista.

Atajos de teclado:

- Flecha izquierda: dia/semana anterior
- Flecha derecha: dia/semana siguiente
- T: volver a Hoy

---

# 9. Filtros

El panel izquierdo de recursos funciona como filtro.

Cada recurso tiene un checkbox o toggle visual.

Al desactivar un recurso, sus Time Blocks se ocultan de la Agenda.

Al activarlo, reaparecen.

Los filtros deben ser instantaneos.

Sin recarga de pagina.

Sin animaciones lentas.

---

# 10. Colores

## Regla principal

El color del Time Block representa el recurso.

Nunca el estado.

## Paleta

Los colores son configurados por el usuario en el modulo de Recursos.

Deben ser pastel o suaves.

Nunca saturados.

## Indicadores de estado

El estado se representa mediante elementos sutiles:

- Un punto de color en la esquina de la tarjeta
- Un icono pequeno (check, reloj, cancelado)
- Un badge minimalista

El indicador de estado nunca debe competir con el color del recurso.

## Indicador de situacion temporal (D-040 / P8)

Ademas del estado persistido, la Agenda/Home/Dashboard/Drawer informan la
**situacion temporal** calculada (`SituacionTemporal`: `demorado`,
`tiempo-excedido`, `a-tiempo`) junto al indicador de estado, como un icono de
alarma de color:

- `demorado` (naranja): el turno no se inicio y su hora ya paso
- `tiempo-excedido` (rojo): el turno En curso supero su horario de fin
- `a-tiempo`: sin indicador de alarma

Es informativo (el reloj informa, el operador decide): nunca modifica el
estado persistido del turno.

## Contraste

El texto sobre la banda de color debe tener contraste suficiente.

Si el fondo es claro, el texto es oscuro.

Si el fondo es oscuro, el texto es claro.

---

# 11. Responsive

## Desktop (> 1024px)

- Navigation Sidebar visible
- Agenda ocupa el resto del espacio
- Drawer aparece desde la izquierda
- Filtro de recursos visible

## Tablet (768px - 1024px)

- Navigation Sidebar colapsable (iconos sin texto)
- Agenda ocupa el ancho completo
- Filtro de recursos en Drawer lateral

## Mobile (< 768px)

- Navigation Sidebar oculto (Drawer de hamburguesa)
- Agenda ocupa el 100% del ancho
- Time Blocks mas compactos
- FAB visible
- Filtro de recursos en Drawer o Bottom Sheet
- Navegacion temporal simplificada

## Regla responsive

El comportamiento responsive pertenece al Layout.

No a la Agenda.

La Agenda solo debe preocuparse de su contenido.

El Layout decide como mostrar la Agenda segun el dispositivo.

---

# 12. Inspiracion visual: Calendly

La inspiracion visual para la Agenda es Calendly.

No copiar.

Tomar unicamente conceptos:

- Limpieza visual extrema
- Uso generoso del espacio
- Tarjetas que parecen adhesivos sobre un calendario
- Navegacion temporal clara y minima
- Sensacion de que el tiempo es tangible

JKTurnos debe desarrollar identidad propia utilizando su Design System.

## Conceptos que NO tomar de Calendly

- No copiar la paleta de colores
- No copiar la tipografia
- No copiar la disposicion de elementos
- No copiar los iconos

JKTurnos tiene su propio DS.

La Agenda debe respetar:

- Variables CSS de jk-tokens.css
- Componentes del DS existentes
- Tipografia Inter
- Paleta del DS

---

# 13. Reglas visuales

- La cuadricula de tiempo usa lineas horizontales suaves (cada hora)
- La hora actual tiene una linea roja o de acento que cruza la Agenda
- Los Time Blocks tienen bordes redondeados (8px)
- Las sombras son minimas, apenas perceptibles
- El fondo del calendario es blanco o gris muy claro
- Las horas de la columna izquierda usan texto secundario
- Los fines de semana (si aplica) tienen fondo ligeramente distinto
- No existen bordes agresivos
- No existen contrastes fuertes
- Todo se siente aireado

---

# 14. Acciones disponibles

## Desde la Agenda (sin abrir Drawer)

- Hacer clic en espacio vacio: crear nuevo turno en ese horario
- Arrastrar bloque: mover turno a otro horario
- Arrastrar borde inferior: redimensionar duracion
- Hover sobre bloque: vista previa rapida (tooltip)

## Desde el Drawer

- Editar datos del turno
- Cambiar estado (Iniciar, Finalizar, Cancelar)
- Ver historial
- Registrar senia / pago
- Agregar observaciones

> Actualizado 2026-08-28 (D-040): las acciones de estado son las de la matriz
> canonica (`TurnoEstadoMotor`): Iniciar (Pendiente -> En curso), Finalizar
> (En curso -> Finalizado), Cancelar (Pendiente/En curso -> Cancelado). No
> existe Confirmar (el turno nace "Pendiente"); Reagendar dejo de ser un
> cambio de estado (no existe el estado Reagendado).

## Acciones masivas (futuro)

- Seleccion multiple de bloques
- Mover multiples turnos
- Bloquear franja horaria recurrente

---

# 15. Estados vacios

## Sin turnos en el dia

La Agenda no debe mostrar una pantalla vacia.

Debe mostrar:

- La cuadricula de tiempo vacia
- Un mensaje sutil: "No tienes turnos hoy"
- Invitacion a crear el primer turno: boton "Reservar turno"

## Sin recursos

Si el negocio no tiene recursos configurados:

- Mensaje: "Necesitas agregar al menos un recurso"
- Accion: "Crear recurso" que redirige al modulo de recursos

## Sin clientes

Al crear un turno, si no existen clientes:

- El formulario de creacion permite ingresar nombre y telefono
- El cliente se crea automaticamente al guardar el turno

---

# 16. Accesibilidad

- Toda accion debe ser accesible por teclado
- Tab index claro en Time Blocks y Drawer
- Atajos de teclado documentados
- Contraste suficiente en todos los elementos
- Los indicadores de estado no deben depender solo del color
- Deben incluir icono o texto ademas del color
- ARIA labels en componentes interactivos
- La Agenda debe ser navegable con lector de pantalla
- El Drawer debe atrapar el foco mientras esta abierto
- Escape cierra el Drawer

---

# 17. Principios UX de la Agenda

**La Agenda es el centro del sistema.**

Todo lo demas es un desvio temporal.

**El usuario nunca pierde el contexto temporal.**

La fecha actual, la hora actual y el rango visible estan siempre presentes.

**La Agenda nunca desaparece.**

Drawers, modales y formularios se superponen, nunca reemplazan.

**El tiempo es visual.**

Cada bloque de 30 minutos ocupa la mitad que un bloque de 60 minutos.

**La accion mas rapida gana.**

Crear un turno debe tomar menos de 15 segundos.

**El color significa recurso.**

Nunca estado.

**Menos es mas.**

Cada elemento en la Agenda debe tener una razon para existir.

**El usuario administra bloques de tiempo.**

No formularios.

No registros.

No tablas.

**La Agenda es un espejo.**

Refleja el dia del usuario.

Si el dia es simple, la Agenda es simple.

---

# 18. Resumen

La Agenda de JKTurnos debe sentirse como:

- Una hoja de calendario en blanco al comenzar el dia
- Una vista aerea de la jornada laboral
- Un tablero donde el usuario organiza tiempo
- Un asistente que muestra lo que importa

No debe sentirse como:

- Un sistema administrativo
- Una tabla de base de datos
- Un formulario web
- Un listado de registros

El exito de la Agenda se mide en una pregunta:

**¿El usuario puede entender su dia en menos de 5 segundos?**

Si la respuesta es si, la Agenda funciona.

Si la respuesta es no, hay que simplificar.

---

Documento aprobado. Sirve como manual de construccion de la pantalla mas
importante de JKTurnos.
