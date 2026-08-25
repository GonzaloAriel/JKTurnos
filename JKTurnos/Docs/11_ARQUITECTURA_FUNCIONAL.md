# ARQUITECTURA FUNCIONAL DE JKTURNOS

Version: 1.0

Fecha: 29/7/2026

Estado: Aprobado

---

# 1. Filosofia del producto

JKTurnos administra la disponibilidad de recursos a traves del tiempo.

No administra clientes.

No administra servicios.

No administra empleados.

Todo gira alrededor del calendario.

Los turnos son solamente una consecuencia de reservar tiempo.

---

# 2. Que es un Turno

Un turno es la reserva de un bloque de tiempo de un recurso para brindar un servicio.

No esta limitado a profesionales.

Debe funcionar igual para:

- odontologos
- peluquerias
- consultorios
- gimnasios
- canchas
- alquileres
- vehiculos
- boxes
- salas
- cualquier recurso reservable

El sistema no distingue entre un turno medico y una reserva de cancha.

La unica diferencia es como se configura el recurso.

---

# 3. Time Block

Time Block es un concepto interno de arquitectura.

Nunca sera visible para el usuario.

Es el componente conceptual que representa un bloque de tiempo dentro de la Agenda.

Un Time Block puede representar:

- un turno
- un horario disponible
- un bloqueo
- un almuerzo
- vacaciones
- mantenimiento
- walk-in
- cualquier ocupacion de tiempo

Todos los bloques se renderizan con el mismo componente visual.

La Agenda no distingue entre "tipos" de bloque a nivel visual.

---

# 4. La Agenda

## 4.1 Es el Home

La Agenda es el verdadero Home del sistema.

No existe un Dashboard tradicional.

Cuando el usuario ingresa al sistema debe abrir directamente la Agenda.

La Agenda es siempre el protagonista visual.

Nunca debe quedar oculta por paneles ni formularios.

El usuario debe poder pensar observando la Agenda.

## 4.2 Vistas

La Agenda ofrece tres vistas:

- Dia
- Semana
- Mes

La vista por defecto es Dia.

El usuario puede alternar entre vistas sin perder el contexto temporal.

## 4.3 Comportamiento

La Agenda siempre muestra el momento actual al abrirse.

El usuario puede navegar hacia adelante y atras.

La fecha actual debe estar siempre identificada visualmente.

---

# 5. Diseño visual del Time Block

Cada Time Block en la Agenda es una pequena tarjeta.

Debe verse como parte natural del calendario.

No como una lista.

## 5.1 Estructura de la tarjeta

**Banda superior**

- Color pastel del recurso (ocupa todo el ancho)
- Hora del bloque
- Menu de acciones (...)

**Cuerpo**

- Nombre del cliente
- Servicio
- Recurso (solo si aplica)

No agregar mas informacion en la tarjeta.

Todo el detalle vive en el Drawer.

## 5.2 Colores

El color representa el recurso.

Nunca el estado.

Ejemplos:

- Azul --> Dr. Juan
- Verde --> Dra. Ana
- Naranja --> Consultorio A

El color es configurable por el usuario en el modulo de Recursos.

## 5.3 Indicadores de estado

El estado del turno se representa mediante pequenos indicadores.

Nunca usando el color principal de la tarjeta.

Los indicadores deben ser sutiles:

- iconos
- puntos de color
- badges minimalistas

---

# 6. Drawer

## 6.1 Regla principal

Al hacer clic sobre un Time Block nunca se cambia de pantalla.

Siempre se abre un Drawer.

## 6.2 Lado de apertura

El Drawer aparece desde la izquierda.

No desde la derecha.

Motivo:

El usuario necesita seguir viendo la Agenda mientras analiza un turno.

La Agenda nunca desaparece.

## 6.3 Contenido del Drawer

El Drawer contiene:

- Informacion completa del turno
- Datos del cliente (nombre, telefono, email, notas)
- Servicio y recurso
- Estados (operativo + economico)
- Acciones disponibles segun contexto
- Historial de cambios

## 6.4 Drawer vs Sidebar

Existen dos paneles distintos en el sistema.

**Navigation Sidebar**

Menu principal del sistema.

Siempre visible (minimizable en desktop).

Contiene la navegacion general.

**Inspector Drawer**

Panel contextual que se abre al hacer clic en un Time Block.

Muestra la informacion del turno seleccionado.

No son el mismo componente.

Cuando el Drawer este abierto, el Navigation Sidebar podra minimizarse automaticamente para ganar espacio horizontal.

---

# 7. Acciones

El usuario no piensa en CRUD.

Piensa en acciones.

## 7.1 Acciones principales

**Reservar turno**

Crear un nuevo bloque de tiempo asignando cliente, servicio y recurso.

**Atender ahora**

Iniciar el turno actual. Marca el estado como "En curso".

**Reagendar**

Mover un turno existente a otro horario.

Arrastrar y soltar en la Agenda.

**Cancelar**

Cambiar el estado del turno a Cancelado.

Solicitar motivo opcional.

**Bloquear horario**

Crear un Time Block que representa indisponibilidad.

Puede ser almuerzo, vacaciones, mantenimiento, etc.

Todas las acciones trabajan sobre bloques de tiempo.

Ninguna accion abre una pantalla separada.

Todas ocurren desde la Agenda mediante Drawers o Drag & Drop.

---

# 8. Walk-in

## 8.1 Escenario

Un cliente entra sin reserva.

El usuario presiona "Atender ahora".

## 8.2 Con disponibilidad

Si existe tiempo disponible en la Agenda:

Se crea el turno inmediatamente con los datos minimos.

## 8.3 Sin disponibilidad

Si no existe tiempo disponible:

El sistema informa cual es el proximo turno programado.

El usuario decide que hacer:

- Mantener la agenda (no atender ahora)
- Mover el siguiente turno para liberar espacio
- Atender igualmente (sobreponer turnos)

La decision siempre es del usuario.

Nunca del sistema.

## 8.4 Registro minimo

Walk-in no debe exigir datos completos del cliente.

Solo nombre y telefono.

---

# 9. Cliente rapido

Durante la reserva solamente deben pedirse los datos minimos:

- Nombre
- Telefono

El resto puede completarse despues.

El sistema debe permitir crear un turno con un cliente nuevo en menos de 15 segundos.

Si el cliente ya existe, debe autocompletarse mediante busqueda.

---

# 10. Recurso unico

Si el negocio posee un solo recurso:

Nunca mostrar selector de recurso.

Debe quedar preseleccionado automaticamente.

Lo mismo aplica si existe un solo profesional.

Si el negocio tiene un solo recurso, el modal de reserva debe ocultar esa opcion.

La interfaz debe simplificarse sola.

---

# 11. Estados

Existen dos grupos distintos de estados.

Nunca deben mezclarse.

## 11.1 Estados operativos

Representan por donde pasa el turno en su ciclo de vida.

- Pendiente
- Confirmado
- En curso
- Finalizado
- Cancelado
- Reagendado

## 11.2 Estados economicos

Representan el estado de pago del turno.

- Sin senia
- Senia pagada
- Pago parcial
- Pago completo

## 11.3 Separacion

Los estados operativos y economicos son independientes.

Un turno puede estar "Finalizado" pero con "Pago pendiente".

La interfaz debe mostrar ambos grupos por separado.

---

# 12. Senia

La senia forma parte del nucleo del sistema.

No es un agregado futuro.

Debe contemplarse desde el diseno inicial.

El Drawer del turno debe incluir:

- Monto de senia
- Estado de senia
- Accion para registrar senia

La senia esta vinculada al estado economico del turno.

Si un turno tiene senia registrada, el estado economico cambia a "Senia pagada".

---

# 13. Bloqueos

Un Time Block tambien puede representar indisponibilidad.

Tipos de bloqueo:

- Almuerzo
- Vacaciones
- Capacitacion
- Mantenimiento
- Reunion
- Bloqueo manual (personalizado)

Todos utilizan el mismo componente visual que los turnos.

La diferencia es que los bloqueos no tienen cliente ni servicio asignados.

Los bloqueos se crean desde la Agenda arrastrando en la zona deseada.

Los bloqueos pueden ser recurrentes (ej. almuerzo de 12 a 13 todos los dias).

---

# 14. Principios de diseno

## 14.1 Prioridad visual

La Agenda siempre tiene prioridad visual.

Ningun otro elemento debe competir con ella.

## 14.2 Contexto temporal

El usuario nunca pierde el contexto temporal.

La fecha actual, la hora actual y el rango visible deben estar siempre presentes.

## 14.3 Fuente de verdad

El calendario es la fuente principal de informacion.

No existen listados de turnos que reemplacen a la Agenda.

## 14.4 Drawers, no pantallas

Los formularios nunca reemplazan a la Agenda.

Todo ocurre mediante Drawers.

La Agenda permanece visible en todo momento.

## 14.5 Adaptabilidad

El sistema debe adaptarse al negocio.

No el negocio al sistema.

## 14.6 Simplificacion automatica

La interfaz debe ocultar automaticamente la complejidad cuando no es necesaria.

Un negocio con un recurso no debe ver selectores de recurso.

## 14.7 Tiempo, no registros

El usuario administra tiempo.

No registros de base de datos.

Cada interaccion debe sentirse como organizar bloques de tiempo.

No como completar formularios.

---

# 15. Estilo visual

La inspiracion visual conceptual es Calendly.

No copiar.

Tomar unicamente conceptos:

- Limpieza
- Aire
- Simplicidad
- Uso del espacio
- Navegacion intuitiva

JKTurnos desarrolla identidad propia utilizando el Design System existente.

## 15.1 Reglas visuales

- La Agenda ocupa la mayor parte del viewport
- Los Time Blocks tienen bordes suaves y sombras sutiles
- Los colores son pastel, nunca saturados
- El fondo general es claro (#F6F1E8 del DS)
- Los textos usan la tipografia Inter del DS
- No existen bordes agresivos ni contrastes fuertes
- Todo debe sentirse aireado y calmo

## 15.2 Inspiracion del producto

- Apple (claridad, materiales, luz)
- Linear (precision, animaciones sutiles, minimalismo)
- Calendly (agenda como centro, bloque visual, navegacion temporal)

---

# 16. Reglas para el desarrollador

- No crear nunca una pagina "Listado de Turnos". La Agenda es el unico listado.
- No mover al usuario a otra pantalla al hacer clic en un turno. Siempre Drawer.
- No preguntar datos que puedan pedirse despues. Solo nombre y telefono al crear.
- No mostrar selectores innecesarios. Si hay un recurso, ocultarlo.
- No mezclar estados operativos con economicos. Son grupos separados.
- No usar el color del recurso para representar estado. El color representa recurso.
- No bloquear al usuario con configuraciones. La configuracion es progresiva.
- No crear formularios largos. Si el formulario tiene mas de 3 campos, esta mal.

---

# 17. Resumen conceptual

JKTurnos no es un sistema de turnos.

Es un sistema de gestion de disponibilidad.

La Agenda es el centro.

Los Time Blocks son la representacion visual del tiempo.

Los turnos son la consecuencia de reservar tiempo.

El usuario no completa formularios.

El usuario organiza su dia.

Todo lo demas es secundario.

---

Documento aprobado. Sirve como guia funcional para el desarrollo de Agenda, Turnos y experiencia principal del sistema.
