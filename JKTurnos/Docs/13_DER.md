# DER

## Modelo Entidad-Relacion de JKTurnos

Version: 1.0

Fecha: 29/7/2026

Estado: Aprobado

---

# Preambulo

El presente documento define el Modelo Entidad-Relacion oficial de JKTurnos.

Representa unicamente la informacion persistente del sistema.

No representa la interfaz.

No representa componentes visuales.

No representa la experiencia de usuario.

El modelo de datos debe mantenerse independiente de la implementacion.

Este documento esta alineado con:

- 09_PRODUCT_DESIGN.md (filosofia del producto)
- 10_CORE_DOMAIN_MODEL.md (modelo conceptual del nucleo)

No contradice ninguno de esos documentos.

Sirve como referencia definitiva para el diseno de la base de datos.

---

# 1. Dominio Core

Entidades fundamentales del sistema. Sin ellas no existe funcionamiento posible.

## 1.1 Tenant

### Responsabilidad

Representa una instancia independiente del sistema. Cada Tenant es un negocio
con sus propios datos, configuracion y operacion.

### Relaciones

Es la entidad raiz del sistema. Todas las entidades de negocio pertenecen a un
Tenant.

### Datos principales

- Identificador unico
- Nombre del negocio
- Subdominio para acceso

### Justificación

Sin Tenant no existe aislamiento entre negocios. Es la base del modelo
multi-inquilino.

## 1.2 User

### Responsabilidad

Representa una persona que opera el sistema. Posee credenciales de acceso y un
rol que determina sus permisos.

### Relaciones

Pertenece a un Tenant.

### Justificación

El sistema necesita identificar quien realiza cada accion. El User es la
identidad operativa del sistema.

## 1.3 ConfiguracionTenant

### Responsabilidad

Almacena la configuracion general del negocio: nombre comercial, direccion,
telefono, email, logo y preferencias de funcionamiento.

### Relaciones

Pertenece a un Tenant. Existe una relacion de uno a uno.

### Justificación

Cada negocio posee datos generales que deben persistirse y consultarse
frecuentemente. Separar la configuracion del Tenant permite evolucionar ambos
conceptos de forma independiente.

## 1.4 Cliente

### Responsabilidad

Representa una persona que recibe servicios del negocio. Almacena sus datos de
contacto y preferencias.

### Relaciones

Pertenece a un Tenant.

Puede tener muchos Turnos asociados.

### Observaciones

Un Cliente puede existir aunque nunca reserve un Turno. Esto es importante
para auditoria e historial.

## 1.5 Servicio

### Responsabilidad

Define una actividad que el negocio ofrece. Establece la duracion, el precio y
las caracteristicas del servicio.

### Relaciones

Pertenece a un Tenant.

Puede estar presente en muchos Turnos.

No representa un Turno. Representa la definicion del servicio.

### Justificación

Sin Servicio no es posible definir cuanto tiempo ocupara un Turno ni que
actividad se realizara.

## 1.6 Recurso

### Responsabilidad

Representa cualquier elemento reservable del negocio. Es el centro de la
disponibilidad. Sin Recurso no existe reserva posible.

### Relaciones

Pertenece a un Tenant.

Puede tener muchos Turnos asociados.

### Ejemplos de Recurso

- Profesional (Dr. Juan, Dra. Ana)
- Sala (Sala A, Sala B)
- Consultorio (Consultorio 1, Consultorio 2)
- Box (Box 5, Box 6)
- Cancha (Cancha 3, Cancha 4)
- Equipo (Equipo de rayos, Resonador)
- Vehiculo (Furgoneta X, Auto Y)

### Aclaracion fundamental

El sistema nunca depende del tipo de recurso.

Un recurso es un recurso. No existe logica distinta para profesionales,
canchas o vehiculos.

El modelo debe ser el mismo para cualquier rubro.

## 1.7 EstadoTurno

### Responsabilidad

Define las etapas operativas por las que atraviesa un Turno durante su ciclo
de vida.

> Actualizado 2026-08-28 (D-040): el catalogo de estados es FIJO del sistema
> (no persistido como CRUD configurable por tenant). Se siembra con la entidad
> `EstadoTurno` (con columna `Codigo` estable) y la semantica se resuelve por
> `Codigo`, nunca por `Nombre`. La matriz de transiciones es UNICA y vive en
> `TurnoEstadoMotor`.

### Relaciones

Cada Turno posee exactamente un Estado operativo.

### Estados operativos (catalogo fijo, D-040)

- Pendiente (`PENDIENTE`)
- En curso (`EN_CURSO`)
- Finalizado (`FINALIZADO`)
- Cancelado (`CANCELADO`)

(Se eliminan Confirmado y Reagendado de la version anterior.)

### Situacion temporal (calculada, NO es estado)

`SituacionTemporal` informa el estado del reloj (`demorado`,
`tiempo-excedido`, `a-tiempo`) sobre el estado persistido. Es calculada e
informativa; no es una fila de `EstadoTurno` y nunca muta el estado.

### Aclaracion

Esta entidad documenta unicamente estados operativos.

Los estados economicos (senia, pago parcial, pago completo) pertenecen al
Dominio Financiero y se modelaran cuando ese dominio se incorpore al sistema.

Ambos grupos son independientes. Un Turno puede estar Finalizado con pago
pendiente.

## 1.8 Turno

### Responsabilidad

Es la entidad central del sistema.

Representa la reserva de un bloque de tiempo de un recurso para brindar un
servicio a un cliente.

### Relaciones

- Pertenece a un Tenant
- Posee exactamente un Cliente
- Posee exactamente un Servicio
- Posee exactamente un Recurso
- Posee exactamente un Estado operativo

Un Turno utiliza un unico Recurso, un unico Servicio y pertenece a un unico
Cliente.

### Justificación

El Turno es la consecuencia de que un cliente solicite un servicio que requiere
un recurso en un momento determinado. Sin Turno no existe reserva.

Ningun Turno puede existir sin Cliente, Servicio y Recurso.

---

# 2. Dominio Financiero (Planificado)

Entidades previstas para la gestion economica del sistema.

Aun no forman parte del nucleo.

## 2.1 Pago

### Responsabilidad prevista

Registrar los cobros asociados a un Turno.

## 2.2 Senia

### Responsabilidad prevista

Registrar anticipos vinculados al estado economico del Turno.

## 2.3 MovimientoCaja

### Responsabilidad prevista

Registrar ingresos y egresos del negocio en un periodo determinado.

## 2.4 Caja

### Responsabilidad prevista

Representar el cierre diario o periodico del negocio.

### Aclaracion

Ninguna de estas entidades forma parte del sistema actual.

Se documentan para establecer la direccion futura del modelo.

---

# 3. Dominio Disponibilidad (Planificado)

Entidades previstas para la gestion de horarios y bloqueos.

Aun no forman parte del nucleo.

## 3.1 HorarioLaboral

### Responsabilidad prevista

Definir la disponibilidad regular de cada Recurso: dias y horas en que el
recurso esta disponible para recibir turnos.

## 3.2 BloqueoHorario

### Responsabilidad prevista

Registrar periodos de indisponibilidad programada: almuerzo, vacaciones,
mantenimiento, capacitacion, reuniones.

## 3.3 ExcepcionHorario

### Responsabilidad prevista

Registrar desviaciones puntuales al horario laboral: un recurso que no
trabaja un dia especifico o que extiende su horario.

### Aclaracion

Actualmente la disponibilidad se gestiona implicitamente mediante la existencia
o ausencia de turnos. Estas entidades se incorporaran cuando el sistema
requiera gestion de horarios explicita.

---

# 4. Dominio Comunicacion (Planificado)

Entidades previstas para la interaccion automatica con clientes.

Seran modulos independientes.

## 4.1 Notificacion

### Responsabilidad prevista

Registrar eventos de comunicacion enviados al cliente: confirmaciones,
recordatorios, cambios de estado.

## 4.2 Recordatorio

### Responsabilidad prevista

Programar envios automaticos previos a la cita.

## 4.3 ReservaOnline

### Responsabilidad prevista

Registrar turnos creados directamente por el cliente sin intervencion del
usuario.

---

# 5. Dominio Auditoria (Planificado)

Entidades previstas para el registro historico de operaciones.

Permitiran reconstruir acciones del sistema.

## 5.1 Historial

### Responsabilidad prevista

Registrar cambios de estado, modificaciones de datos y acciones relevantes
sobre entidades del sistema.

## 5.2 ArchivoAdjunto

### Responsabilidad prevista

Almacenar documentos e imagenes asociados a clientes o turnos.

## 5.3 Bitacora

### Responsabilidad prevista

Registrar eventos de seguridad y acceso al sistema.

---

# 6. Relaciones

Las relaciones entre las entidades del modelo son las siguientes:

**Tenant**

- Un Tenant posee muchos Usuarios
- Un Tenant posee muchos Clientes
- Un Tenant posee muchos Servicios
- Un Tenant posee muchos Recursos
- Un Tenant posee muchos Turnos
- Un Tenant posee muchos Estados de Turno
- Un Tenant posee una Configuracion

**Cliente**

- Un Cliente pertenece a un Tenant
- Un Cliente puede tener muchos Turnos

**Servicio**

- Un Servicio pertenece a un Tenant
- Un Servicio puede estar presente en muchos Turnos

**Recurso**

- Un Recurso pertenece a un Tenant
- Un Recurso puede tener muchos Turnos
- Un Turno utiliza un unico Recurso

**EstadoTurno**

- Un EstadoTurno se siembra por tenant como catalogo FIJO del sistema (D-040); la semantica es global y se resuelve por `Codigo`, nunca por `Nombre`
- Un EstadoTurno puede estar presente en muchos Turnos
- Un Turno posee un unico Estado

**Turno**

- Un Turno pertenece a un Tenant
- Un Turno pertenece a un unico Cliente
- Un Turno utiliza un unico Servicio
- Un Turno utiliza un unico Recurso
- Un Turno posee un unico Estado

---

# 7. Diagrama DER Conceptual

El siguiente diagrama representa las entidades persistentes del sistema y sus
relaciones. No representa atributos ni detalles de implementacion.

                           TENANT
                              |
      +-----------------------+-----------------------+
      |             |         |         |             |
      |             |         |         |             |
  Usuario    Configuracion  Cliente  Servicio    Recurso
      |             |         |         |             |
      +-------------+---------+---------+-------------+
                                        |
                                       Turno
                                        |
                                  EstadoTurno


                            DOMINIOS PLANIFICADOS


    FINANCIERO         DISPONIBILIDAD      COMUNICACION      AUDITORIA

      Pago          HorarioLaboral        Notificacion       Historial
      Senia         BloqueoHorario        Recordatorio    ArchivoAdjunto
  MovimientoCaja  ExcepcionHorario     ReservaOnline        Bitacora
      Caja


---

# 8. Entidades que NO pertenecen al DER

Los siguientes conceptos NO son entidades persistentes.

No deben aparecer nunca en un modelo de datos.

Pertenecen exclusivamente a la capa de presentacion.

- **Agenda**: es una vista construida a partir de datos persistentes. No se
  almacena.
- **Time Block**: es un concepto interno de interfaz. No tiene representacion
  en base de datos.
- **Drawer**: es un componente de interfaz. No almacena informacion.
- **Timeline**: es una representacion visual del historial. No es una entidad.
- **Floating Action Button**: es un elemento de navegacion. No es persistente.
- **Calendar View**: es un modo de visualizacion. No almacena datos.
- **Status Badge**: es un indicador visual. No tiene representacion propia.
- **Color Selector**: es un componente de formulario. No es una entidad.

Ninguna entidad del DER debe crearse para satisfacer una necesidad de interfaz.

Las entidades representan el negocio. No la experiencia de usuario.

---

# 9. Decisiones de modelado

**La Agenda no se almacena.**

La Agenda es una vista calculada a partir de turnos, recursos y horarios.
No existe una tabla "Agenda".

**La disponibilidad se calcula.**

No existe una entidad "Disponibilidad". La disponibilidad se determina
evaluando los turnos existentes y los bloqueos registrados.

**Time Block no es una entidad.**

Time Block es un concepto de interfaz. No debe modelarse como tabla.

**Los recursos representan cualquier elemento reservable.**

No existen tablas separadas para profesionales, salas o equipos.
Todo es Recurso.

**Los turnos nunca existen sin recurso.**

Un Turno sin Recurso no tiene sentido en el modelo.
El Recurso es obligatorio.

**Los estados operativos y economicos son independientes.**

No deben combinarse en una sola entidad.
Son conceptos distintos que evolucionan por separado.

**El sistema debe ser independiente del rubro del negocio.**

No deben existir tablas especificas para odontologia, peluqueria o canchas.
El modelo debe funcionar para cualquier rubro sin cambios estructurales.

**No duplicar informacion que pueda calcularse.**

Si un dato puede obtenerse a partir de otros datos existentes, no debe
almacenarse por separado.

**Priorizar simplicidad sobre especializacion.**

Antes de crear una nueva entidad debe evaluarse si el modelo existente ya
resuelve la necesidad.

---

# 10. Roadmap del modelo

| Entidad             | Estado       | Dominio        |
|---------------------|--------------|----------------|
| Tenant              | Core         | Organizacion   |
| User                | Core         | Organizacion   |
| ConfiguracionTenant | Core         | Organizacion   |
| Cliente             | Core         | Negocio        |
| Servicio            | Core         | Negocio        |
| Recurso             | Core         | Negocio        |
| EstadoTurno         | Core         | Negocio        |
| Turno               | Core         | Negocio        |
| Pago                | Planificada  | Financiero     |
| Senia               | Planificada  | Financiero     |
| MovimientoCaja      | Planificada  | Financiero     |
| Caja                | Planificada  | Financiero     |
| HorarioLaboral      | Futura       | Disponibilidad |
| BloqueoHorario      | Futura       | Disponibilidad |
| ExcepcionHorario    | Futura       | Disponibilidad |
| Notificacion        | Futura       | Comunicacion   |
| Recordatorio        | Futura       | Comunicacion   |
| ReservaOnline       | Futura       | Comunicacion   |
| Historial           | Futura       | Auditoria      |
| ArchivoAdjunto      | Futura       | Auditoria      |
| Bitacora            | Futura       | Auditoria      |

---

# 11. Principios del Modelo de Datos

**El modelo debe representar el negocio y no la interfaz.**

Ninguna entidad debe crearse para resolver un problema visual.

**Cada entidad debe tener una unica responsabilidad.**

Si una entidad acumula responsabilidades multiples, debe dividirse.

**Las relaciones deben ser simples y explicitas.**

Relaciones innecesariamente complejas indican problemas de diseno.

**El modelo debe soportar multiples rubros sin modificaciones estructurales.**

Odontologia, peluqueria, canchas y talleres utilizan el mismo modelo.

**La disponibilidad es un calculo, no una entidad.**

No se persiste disponibilidad. Se persisten turnos y bloqueos. La
disponibilidad se obtiene por diferencia.

**La Agenda se construye utilizando la informacion persistida.**

La Agenda es una vista. Depende de los datos del dominio, no al reves.

**Todo crecimiento futuro debera respetar el nucleo definido por el Core
Domain Model.**

Las extensiones se construyen alrededor del nucleo, no sobre el nucleo.

**Toda entidad nueva debe declarar su dominio de pertenencia.**

Ninguna entidad puede incorporarse sin indicar a que dominio pertenece (Core,
Financiero, Disponibilidad, Comunicacion, Auditoria) y por que no puede
resolverse reutilizando una entidad existente.

---

# 12. Conclusion

Este documento constituye la referencia oficial del modelo de datos de
JKTurnos.

Define las entidades persistentes del sistema, sus responsabilidades y sus
relaciones.

Cualquier nueva funcionalidad debera integrarse respetando la filosofia
definida en:

- 09_PRODUCT_DESIGN.md (filosofia del producto)
- 10_CORE_DOMAIN_MODEL.md (modelo conceptual del nucleo)
- El presente DER (modelo de datos)

No deberan incorporarse entidades nuevas sin justificar su responsabilidad
dentro del dominio correspondiente y sin demostrar que no pueden resolverse
con entidades existentes.

---

Documento aprobado. Sirve como referencia oficial del modelo de datos de
JKTurnos.
