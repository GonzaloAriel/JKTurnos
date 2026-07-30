# CORE DOMAIN MODEL

## Modelo Conceptual del Nucleo de JKTurnos

Version: 1.0

Fecha: 29/7/2026

Estado: Aprobado

---

# Preambulo

JKTurnos administra la disponibilidad de recursos a traves del tiempo.

No administra clientes.

No administra servicios.

No administra turnos.

Los turnos son una consecuencia de reservar tiempo.

Toda la arquitectura del sistema gira alrededor de ese principio.

El presente documento define los conceptos fundamentales del negocio, sus
responsabilidades y sus relaciones. No describe implementacion tecnica.

Sirve como puente entre la filosofia del producto y el futuro modelo de datos.

Cualquier nueva funcionalidad debera respetar este nucleo antes de incorporarse
al producto.

---

# 1. Capa de Organizacion

Conceptos que permiten que exista un negocio dentro del sistema pero que no
participan directamente en una reserva.

## 1.1 Tenant

Representa una instancia independiente del sistema.

Cada Tenant es un negocio con sus propios datos, configuracion y operacion.

Los Tenants estan aislados entre si. Un Tenant no puede ver informacion de otro.

Ejemplos de Tenant:

- Una clinica odontologica
- Una peluqueria
- Un centro de estetica
- Un complejo de canchas
- Un taller mecanico
- Un consultorio particular

## 1.2 Usuario

Representa una persona que opera el sistema.

Pertenece a un Tenant.

Posee un rol que determina que acciones puede realizar.

No es un recurso. Un Usuario puede ser tambien un Recurso, pero son conceptos
distintos.

## 1.3 Configuracion

Representa las preferencias y datos generales del negocio.

Incluye informacion como nombre comercial, direccion, telefono y opciones de
funcionamiento.

La configuracion es progresiva: el usuario comienza a trabajar con valores por
defecto y ajusta cuando lo necesita.

---

# 2. Capa de Negocio

Conceptos que representan las entidades fundamentales del dominio.

## 2.1 Cliente

Representa una persona que recibe un servicio.

Pertenece a un Tenant.

Sus datos minimos son nombre y telefono. El resto puede registrarse
progresivamente.

Un Cliente puede tener historial de turnos, preferencias y notas asociadas.

## 2.2 Servicio

Representa una actividad que el negocio ofrece.

Pertenece a un Tenant.

Define una duracion y un precio.

Puede tener color, descripcion y categoria.

Ejemplos de Servicio:

- Consulta odontologica (40 min)
- Corte de cabello (30 min)
- Alquiler de cancha (60 min)
- Evaluacion mecanica (45 min)
- Clase de yoga (50 min)

## 2.3 Recurso

Representa cualquier elemento que pueda reservarse para brindar un servicio.

Pertenece a un Tenant.

Es el centro de la disponibilidad. Sin recurso no existe reserva posible.

Ejemplos de Recurso:

- Dr. Juan Martinez (profesional)
- Consultorio A (espacio fisico)
- Cancha 3 (instalacion)
- Furgoneta X (vehiculo)
- Box 5 (espacio de trabajo)
- Equipo de rayos (maquinaria)

Un negocio puede tener uno o multiples recursos.

Cuando existe un solo recurso, el sistema debe simplificar la interfaz
automaticamente.

El concepto de Recurso reemplaza al concepto tradicional de "Empleado".
JKTurnos no administra empleados. Administra recursos.

## 2.4 Turno

Representa la reserva de un bloque de tiempo.

Es la consecuencia de que un Cliente solicite un Servicio que requiere un
Recurso en un momento determinado.

### Definicion oficial

Un Turno representa la reserva de un bloque de tiempo de un recurso para
brindar un servicio a un cliente.

Esta definicion funciona para cualquier actividad:

- consultorios medicos
- peluquerias
- gimnasios
- canchas deportivas
- talleres
- alquileres
- boxes
- salas
- vehiculos
- cualquier recurso reservable

El modelo nunca depende de un rubro especifico.

## 2.5 Estado del Turno

Representa la situacion actual de un turno dentro de su ciclo de vida.

Pertenece a un Tenant.

Existen dos grupos de estados, funcionalmente independientes:

### Estados operativos

- Pendiente
- Confirmado
- En curso
- Finalizado
- Cancelado
- Reagendado

### Estados economicos

- Sin senia
- Senia pagada
- Pago parcial
- Pago completo

Ambos grupos coexisten pero no se mezclan. Un turno puede estar "Finalizado"
con "Pago pendiente".

---

# 3. Capa de Interfaz (UI Concepts)

Los conceptos de esta seccion NO pertenecen al dominio del negocio.

No representan informacion persistente.

Representan unicamente la experiencia de usuario.

Se incluyen en este documento porque definen como el usuario percibe el nucleo
del sistema y porque cualquier implementacion debe respetar su comportamiento.

## 3.1 Agenda

La Agenda representa graficamente la disponibilidad de recursos en el tiempo.

No almacena informacion.

Se construye utilizando los datos del dominio: recursos, turnos, bloqueos y
horarios.

La Agenda es el Home del sistema. No existe un dashboard tradicional.

Cuando el usuario ingresa a JKTurnos, la Agenda es lo primero que ve.

## 3.2 Time Block

Time Block es un concepto interno de arquitectura.

Nunca sera visible para el usuario como termino.

Representa cualquier bloque de tiempo mostrado dentro de la Agenda.

Puede representar:

- un turno
- un bloqueo (almuerzo, vacaciones, mantenimiento)
- un horario disponible
- un walk-in

El componente visual es siempre el mismo.

Solo cambia el contenido que muestra.

A nivel de interfaz no existen tipos distintos de bloques.
Existen bloques con datos distintos.

## 3.3 Drawer

El Drawer es un inspector contextual.

Se abre cuando el usuario hace clic sobre un Time Block.

Nunca reemplaza la Agenda.

Aparece desde la izquierda.

La Agenda permanece visible mientras el Drawer esta abierto.

El usuario toma decisiones observando simultaneamente el Drawer y el
calendario.

El Drawer contiene la informacion completa del turno, los datos del cliente,
las acciones disponibles y el historial.

## 3.4 Floating Action Button (FAB)

Boton flotante visible en dispositivos moviles.

Permite crear un nuevo turno rapidamente sin navegar a otra seccion.

## 3.5 Timeline

Representacion visual del historial de un turno dentro del Drawer.

Muestra los cambios de estado, las fechas y los responsables en orden
cronologico.

---

# 4. Capa de Extensiones Futuras

Conceptos que aun no pertenecen al nucleo pero creceran alrededor de el.

Todos dependen de los conceptos definidos en la Capa de Negocio.

- **Pagos**: procesamiento de cobros vinculado a un Turno
- **Senias**: registro de anticipos vinculado al estado economico del Turno
- **Caja**: cierre diario y conciliacion de pagos
- **Notificaciones**: recordatorios y comunicaciones con el Cliente
- **Reserva Online**: portal publico donde el Cliente reserva sin intervencion
  del Usuario
- **Reportes**: estadisticas sobre ocupacion, ingresos y performance
- **Integraciones**: conexion con calendarios externos, WhatsApp, email

Ninguna extension debe modificar el nucleo definido en este documento.

Deben construirse alrededor del nucleo respetando la filosofia del sistema.

---

# 5. Relaciones Conceptuales

Las relaciones entre los conceptos del dominio son las siguientes:

- Un **Cliente** solicita un **Servicio**
- El **Servicio** requiere un **Recurso**
- El **Recurso** reserva un bloque de **tiempo**
- La reserva genera un **Turno**
- El **Turno** atraviesa **Estados** durante su ciclo de vida
- El **Turno** se representa visualmente mediante un **Time Block**
- Los **Time Blocks** conforman la **Agenda**
- La **Agenda** utiliza el **Drawer** para mostrar detalle
- Un **Usuario** opera el sistema dentro de un **Tenant**
- Un **Tenant** posee una **Configuracion**

No existen relaciones directas entre Cliente y Agenda.

No existen relaciones directas entre Servicio y Time Block.

Todo pasa a traves del Turno.

---

# 6. Diagrama Conceptual

El siguiente diagrama representa las relaciones conceptuales del dominio.

No representa una base de datos ni un modelo de implementacion.

                  ORGANIZACION

       Tenant -- Usuario -- Configuracion


===============================================


               NUCLEO DEL NEGOCIO

    Cliente
        \
         \  solicita
          \
           v
    Servicio ---> Turno <--- Recurso
                      |
                      v
              Estado del Turno


===============================================


              EXPERIENCIA (UI)

              Time Block
                   |
                   v
                Agenda
                   |
                Drawer


===============================================


             EXTENSIONES FUTURAS

    Pagos  Senias  Caja  Notificaciones
    Reserva Online  Reportes  Integraciones


---

# 7. Principios del Core Domain Model

**El tiempo es el activo principal del sistema.**

Todo el modelo existe para administrar tiempo.

**Los recursos administran disponibilidad.**

La disponibilidad se define por recurso. Sin recurso no existe agenda.

**Los turnos representan reservas de tiempo.**

Un turno no es un registro administrativo. Es un bloque de tiempo reservado.

**La Agenda nunca almacena informacion.**

La Agenda es una vista. Se construye a partir de los datos del dominio.

**Los Time Blocks representan visualmente el tiempo.**

Un bloqueo, un turno y un horario disponible se representan con el mismo
componente.

**La interfaz se construye sobre el dominio y no al reves.**

No existen conceptos de interfaz que modifiquen el modelo de negocio.

**El modelo debe servir para cualquier rubro.**

Odontologia, peluqueria, canchas, alquileres. El nucleo no distingue entre
ellos.

**La simplicidad debe prevalecer sobre la especializacion.**

Antes de agregar un concepto nuevo debe evaluarse si el modelo existente ya lo
resuelve.

**El nucleo debe poder crecer sin romper su filosofia.**

Las extensiones se construyen alrededor del nucleo, no sobre el nucleo.

---

# 8. Conclusion

Este documento define el modelo conceptual del nucleo de JKTurnos.

Establece los conceptos fundamentales, sus responsabilidades y sus relaciones.

No describe implementacion.

No describe tecnologia.

Describe el dominio del negocio.

Este modelo es la referencia oficial para:

- El diseno del modelo de datos (DER)
- Las decisiones de arquitectura del sistema
- La incorporacion de nuevas funcionalidades
- La comunicacion entre el equipo de producto y el equipo tecnico

Toda nueva funcionalidad debera respetar este nucleo antes de incorporarse al
producto.

Cualquier concepto que no se encuentre en este documento debe evaluarse contra
los principios definidos en la seccion anterior antes de ser incorporado.

---

Documento aprobado. Sirve como referencia oficial del modelo conceptual del
nucleo de JKTurnos.
