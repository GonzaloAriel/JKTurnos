# TURNO LIFECYCLE

## Ciclo de Vida del Turno de JKTurnos

Version: 1.0

Fecha: 29/7/2026

Estado: Aprobado

---

# Preambulo

Este documento responde una sola pregunta:

**¿Que le puede pasar a un turno durante toda su vida?**

Documenta cada estado, cada transicion, cada regla y cada consecuencia.

No habla de implementacion.

No habla de base de datos.

Habla del comportamiento del negocio.

Sirve como guia para la construccion del TurnoService y de toda la logica
asociada al ciclo de vida del turno.

---

# 1. Filosofia del ciclo de vida

Un turno nace, vive y muere.

Durante su vida puede cambiar de estado, de horario o de recurso.

Cada cambio tiene:

- una accion que lo origina
- un responsable
- una regla que lo valida
- una consecuencia inmediata
- un registro de auditoria

El ciclo de vida debe ser predecible.

El usuario nunca debe preguntarse "¿que pasa si hago esto?"

El sistema debe comportarse de forma logica y consistente.

---

# 2. Estados del Turno

## 2.1 Estados operativos

| Estado      | Significado                                          |
|-------------|------------------------------------------------------|
| Pendiente   | El turno fue creado pero aun no fue confirmado       |
| Confirmado  | El turno fue confirmado por el cliente o el operador |
| En curso    | El servicio esta siendo brindado en este momento     |
| Finalizado  | El servicio fue completado                           |
| Cancelado   | El turno fue cancelado antes de finalizar            |
| Reagendado  | El turno fue movido a otra fecha u horario           |

## 2.2 Estados economicos

| Estado         | Significado                                        |
|----------------|----------------------------------------------------|
| Sin senia      | No se registro ningun pago                         |
| Senia pagada   | Se registro una senia                              |
| Pago parcial   | Se registro un pago parcial                        |
| Pago completo  | El servicio fue pagado en su totalidad             |

Los estados operativos y economicos son independientes.

Un turno puede estar Finalizado con Pago completo o Finalizado con Pago
pendiente.

Ambos grupos coexisten pero no se mezclan.

---

# 3. Mapa completo de transiciones

## 3.1 Transiciones operativas

```
                        NACE EL TURNO

                    Disponible (franja horaria)
                             |
                        [Reservar]
                             |
                         Pendiente
                        /    |    \
                       /     |     \
                 [Confirmar] |  [Cancelar]
                     /       |        \
                    /        |         \
              Confirmado  Reagendado  Cancelado
                  |    \       |
                  |     \      |
           [Iniciar]  [Cancelar] [Confirmar]
                  |        \        |
                  |         \       |
              En curso   Cancelado  Confirmado
                  |
           [Finalizar]
                  |
              Finalizado
```

### Transiciones explicitas

| Desde        | Accion        | Hasta        | Valido |
|--------------|---------------|--------------|--------|
| Disponible   | Reservar      | Pendiente    | Siempre |
| Pendiente    | Confirmar     | Confirmado   | Siempre |
| Pendiente    | Cancelar      | Cancelado    | Siempre |
| Pendiente    | Reagendar     | Reagendado   | Siempre |
| Confirmado   | Iniciar       | En curso     | Cuando sea la fecha/hora |
| Confirmado   | Cancelar      | Cancelado    | Siempre |
| Confirmado   | Reagendar     | Reagendado   | Siempre |
| En curso     | Finalizar     | Finalizado   | Siempre |
| En curso     | Cancelar      | Cancelado    | Solo por administrador |
| Reagendado   | Confirmar     | Confirmado   | Siempre |
| Reagendado   | Cancelar      | Cancelado    | Siempre |
| Reagendado   | Reagendar     | Reagendado   | Siempre (nuevo cambio) |

### Transiciones bloqueadas

| Desde       | Accion      | Motivo                                   |
|-------------|-------------|------------------------------------------|
| Cancelado   | Cualquiera  | Un turno cancelado no puede reactivarse  |
| Finalizado  | Cualquiera  | Un turno finalizado no puede modificarse |
| En curso    | Reagendar   | No se reagenda un turno en curso         |
| En curso    | Confirmar   | Ya esta mas alla de confirmado           |

## 3.2 Transiciones economicas

```
                   Sin senia
                  /    |    \
                 /     |     \
          [Senia] [Pago parcial] [Pago completo]
            /         |            \
           /          |             \
    Senia pagada  Pago parcial   Pago completo
          |            |
          |            |
    [Pago parcial] [Pago completo]
          |            |
          v            v
    Pago parcial   Pago completo
          |
    [Pago completo]
          |
          v
    Pago completo
```

### Transiciones explicitas

| Desde          | Accion        | Hasta           |
|----------------|---------------|-----------------|
| Sin senia      | Pagar senia   | Senia pagada    |
| Sin senia      | Pago parcial  | Pago parcial    |
| Sin senia      | Pago completo | Pago completo   |
| Senia pagada   | Pago parcial  | Pago parcial    |
| Senia pagada   | Pago completo | Pago completo   |
| Pago parcial   | Pago completo | Pago completo   |

### Reglas economicas

- No se puede registrar un pago menor al monto ya registrado
- No se puede superar el precio total del servicio
- Una senia solo puede registrarse una vez
- El estado economico no bloquea las transiciones operativas
- Un turno puede finalizar con cualquier estado economico

---

# 4. Flujos especiales

## 4.1 Walk-in

El walk-in representa un cliente que llega sin turno previo.

### Flujo

```
Cliente llega sin turno
         |
   [Atender ahora]
         |
   ¿Hay disponibilidad?
    /                  \
   Si                  No
   |                    |
   v                    v
Crear turno        Informar al usuario
   |                    |
   v                    v
En curso           Usuario decide:
   |                 - Mantener agenda
   v                 - Mover siguiente turno
Finalizado           - Atender igualmente
```

### Reglas

- Walk-in salta el estado Pendiente
- El turno nace directamente en "En curso"
- No exige datos completos del cliente
- Solo requiere nombre y telefono
- Si no hay disponibilidad, la decision final es del usuario

## 4.2 Reagendamiento

Cuando un turno cambia de fecha u horario.

### Flujo

```
Turno existente (Pendiente o Confirmado)
         |
   [Reagendar]
         |
   v
   Seleccionar nueva fecha/hora
         |
   v
   ¿El nuevo horario esta disponible?
    /                  \
   Si                  No
   |                    |
   v                    v
Estado: Reagendado  Informar conflicto
   |                    |
   v                    v
   [Confirmar]     Sugerir horario alternativo
   |
   v
   Confirmado
```

### Reglas

- El turno original conserva su historial
- El nuevo horario no debe superponerse con otro turno del mismo recurso
- Al reagendar, el turno pasa a "Reagendado"
- Requiere confirmacion posterior (automatica o manual segun configuracion)
- Se debe registrar el motivo del reagendamiento
- El historial debe conservar el horario anterior y el nuevo

## 4.3 Cancelacion

### Flujo

```
Turno existente (Pendiente, Confirmado o En curso*)
         |
   [Cancelar]
         |
   v
   Solicitar motivo de cancelacion
         |
   v
   ¿El turno tiene senia o pago?
    /                  \
   No                  Si
   |                    |
   v                    v
   Cancelado        Registrar deuda pendiente*
                        |
                        v
                    Cancelado
```

* Solo administrador puede cancelar un turno "En curso"
* Si hay senia, debe registrarse si se reembolsa o se retiene

### Reglas

- Toda cancelacion requiere motivo
- El motivo debe ser seleccionable (con opcion "Otro")
- Motivos posibles:
  - Cliente cancelo
  - Cliente no asistio
  - Profesional cancelo
  - Problema tecnico
  - Otro (texto libre)
- Si el turno tenia senia, preguntar:
  - Reembolsar senia
  - Retener senia
- El estado economico no cambia automaticamente al cancelar
- Queda registrado si hay saldo pendiente

---

# 5. Acciones del ciclo de vida

## 5.1 Reservar

### Quien
- Usuario autenticado
- (Futuro) Cliente via reserva online

### Que hace
Crea un nuevo turno en estado Pendiente.

### Requisitos
- Cliente existe (o se crea minimo con nombre y telefono)
- Servicio seleccionado
- Recurso seleccionado (o preseleccionado si es unico)
- Fecha y hora validas
- El horario esta disponible para el recurso

### Consecuencias
- El bloque de tiempo queda reservado
- El recurso no puede recibir otro turno en ese horario
- (Futuro) Se envia notificacion de confirmacion pendiente

### Validaciones
- La fecha no puede ser pasada (excepto walk-in)
- La hora debe estar dentro del horario laboral del recurso
- No debe existir otro turno en el mismo horario para el mismo recurso

## 5.2 Confirmar

### Quien
- Usuario autenticado
- (Futuro) Cliente via enlace de confirmacion

### Que hace
Cambia el estado de Pendiente a Confirmado.

### Requisitos
- El turno debe estar en estado Pendiente o Reagendado

### Consecuencias
- El turno queda firme en la Agenda
- (Futuro) Se programa recordatorio

## 5.3 Iniciar

### Quien
- Usuario autenticado

### Que hace
Cambia el estado de Confirmado a En curso.

### Requisitos
- El turno debe estar en estado Confirmado
- Debe ser la fecha del turno o posterior

### Consecuencias
- El bloque de tiempo se marca como "ocupado ahora"
- (Futuro) Se notifica al cliente que el servicio comenzo

### Validaciones
- No se puede iniciar un turno antes de su fecha/hora
- No se puede iniciar un turno de otro recurso sin permisos

## 5.4 Finalizar

### Quien
- Usuario autenticado

### Que hace
Cambia el estado de En curso a Finalizado.

### Requisitos
- El turno debe estar en estado En curso

### Consecuencias
- El servicio se marca como completado
- El recurso queda disponible para el proximo turno
- (Futuro) Se genera registro para caja

## 5.5 Cancelar

### Quien
- Usuario autenticado
- (Futuro) Cliente via enlace de cancelacion

### Que hace
Cambia el estado a Cancelado.

### Requisitos
- El turno no debe estar Finalizado
- Debe registrarse un motivo
- Si el turno esta En curso, solo administrador puede cancelar

### Consecuencias
- El bloque de tiempo queda disponible nuevamente
- Se registra el motivo de cancelacion
- Si habia senia, se define si se reembolsa o retiene
- (Futuro) Se notifica al cliente

## 5.6 Reagendar

### Quien
- Usuario autenticado
- (Futuro) Cliente via enlace de reagendamiento

### Que hace
Cambia la fecha y/u hora del turno.

### Requisitos
- El turno no debe estar En curso ni Finalizado ni Cancelado
- El nuevo horario debe estar disponible
- Debe registrarse el motivo del cambio

### Consecuencias
- El horario original queda disponible
- El nuevo horario queda reservado
- El estado pasa a Reagendado
- El historial registra el cambio

## 5.7 Pagar senia

### Quien
- Usuario autenticado

### Que hace
Registra una senia.

### Requisitos
- El monto debe ser menor al precio total del servicio
- No debe existir una senia previa

### Consecuencias
- El estado economico cambia a Senia pagada
- (Futuro) Se registra en caja

## 5.8 Registrar pago

### Quien
- Usuario autenticado

### Que hace
Registra un pago parcial o completo.

### Requisitos
- El monto acumulado no debe superar el precio del servicio
- El monto debe ser mayor al ya registrado

### Consecuencias
- El estado economico se actualiza
- (Futuro) Se registra en caja

---

# 6. Reglas generales

## 6.1 Reglas operativas

- Ningun turno puede existir sin Cliente, Servicio y Recurso
- Ningun turno puede superponerse con otro del mismo recurso
- Un turno cancelado no puede reactivarse
- Un turno finalizado no puede modificarse
- Solo un administrador puede cancelar un turno en curso
- El reagendamiento preserva el historial completo del turno original
- Cada cambio de estado debe registrar fecha, hora y responsable

## 6.2 Reglas economicas

- Los estados economicos son independientes de los operativos
- No se puede registrar un pago mayor al precio del servicio
- No se puede registrar una senia si ya existe una
- La senia no reembolsada queda como deuda del cliente
- Un turno cancelado con senia retenida mantiene el registro contable

## 6.3 Reglas de disponibilidad

- Un recurso no puede tener dos turnos en el mismo horario
- El sistema no debe permitir crear un turno en horario bloqueado
- El sistema no debe permitir crear un turno en el pasado
- (Futuro) El sistema debe respetar el horario laboral del recurso

---

# 7. Auditoria

Cada accion sobre un turno debe generar un registro de auditoria.

## Datos de auditoria por evento

| Dato             | Descripcion                                    |
|------------------|------------------------------------------------|
| Fecha y hora     | Momento exacto de la accion                    |
| Responsable      | Usuario que realizo la accion                  |
| Accion           | Tipo de accion (crear, confirmar, cancelar...) |
| Estado anterior  | Estado del turno antes de la accion            |
| Estado posterior | Estado del turno despues de la accion          |
| Detalle          | Informacion adicional segun la accion          |

## Eventos que generan auditoria

- Creacion del turno
- Cada cambio de estado operativo
- Cada cambio de estado economico
- Cada reagendamiento (con horario anterior y nuevo)
- Cada cancelacion (con motivo)
- Cada pago registrado (con monto)

---

# 8. Notificaciones futuras

Cuando el modulo de comunicacion este implementado, cada accion debera
disparar notificaciones.

## Mapa de notificaciones

| Accion           | Notificacion                         | Destinatario     |
|------------------|--------------------------------------|------------------|
| Reservar         | Turno creado (pendiente de confirmar)| Cliente          |
| Confirmar        | Turno confirmado                     | Cliente          |
| Recordatorio     | Recordatorio de turno proximo        | Cliente          |
| Reagendar        | Turno reagendado (nuevo horario)     | Cliente          |
| Cancelar         | Turno cancelado                      | Cliente          |
| Iniciar          | Servicio en curso                    | Cliente          |
| Finalizar        | Servicio completado                  | Cliente          |
| Recordatorio     | Recordatorio (X horas antes)        | Cliente          |

Los canales de notificacion seran definidos cuando el modulo se implemente.

---

# 9. Matriz de permisos

| Accion           | Admin | Operador | (Futuro) Cliente |
|------------------|-------|----------|------------------|
| Reservar         | Si    | Si       | Si               |
| Confirmar        | Si    | Si       | Si               |
| Iniciar          | Si    | Si       | No               |
| Finalizar        | Si    | Si       | No               |
| Cancelar         | Si    | Si (*)   | Si (**)          |
| Reagendar        | Si    | Si       | Si (**)          |
| Pagar senia      | Si    | Si       | No               |
| Registrar pago   | Si    | Si       | No               |

(*) Operador no puede cancelar turnos En curso
(**) Cliente solo puede cancelar/reagendar dentro de la ventana definida por
     el negocio

---

# 10. Reglas de validacion por accion

## Reservar

```
- Fecha no puede ser pasada
- Hora debe ser valida (dentro de horario del recurso)
- Recurso debe estar activo
- Servicio debe estar activo
- Cliente debe existir (o crearse con datos minimos)
- No debe existir conflicto de horario con el mismo recurso
```

## Confirmar

```
- Turno debe estar en estado Pendiente o Reagendado
```

## Iniciar

```
- Turno debe estar en estado Confirmado
- Fecha debe ser hoy o posterior
```

## Finalizar

```
- Turno debe estar en estado En curso
```

## Cancelar

```
- Turno no debe estar Finalizado
- Motivo obligatorio
- Si esta En curso, solo Admin
- Si hay senia, preguntar reembolso/retencion
```

## Reagendar

```
- Turno no debe estar En curso, Finalizado ni Cancelado
- Nueva fecha/hora debe ser valida
- Nuevo horario debe estar disponible
- Motivo obligatorio
```

---

# 11. Estados finales

Un turno llega a su estado final cuando ocurre cualquiera de estas
condiciones:

### Finalizado

El servicio fue completado exitosamente.

No se puede modificar.

Solo se puede consultar.

El registro permanece para historial, reportes y contabilidad.

### Cancelado

El turno no se realizo.

No se puede reactivar.

El registro permanece para historial y reportes.

Si tenia senia retenida, el registro contable se mantiene.

---

# 12. Diagrama completo

```
                              NACE
                                |
                            Pendiente
                           /    |    \
                          /     |     \
                    Confirmo  Reagendo  Cancelan
                    /          |           \
                   /           |            \
              Confirmado  Reagendado     Cancelado
              /    |           |          [FINAL]
             /     |           |
        Inicio  Reagendo   Confirman
          |        |           |
          v        v           v
      En curso  Reagendado  Confirmado
          |        |
          v        v
     Finalizado  (ciclo)
       [FINAL]


                  ESTADOS ECONOMICOS

               Sin senia
             /    |    \
            /     |     \
        Senia  Parcial  Completo
          |      |        [FINAL]
          v      v
       Parcial Completo
          |      [FINAL]
          v
      Completo
       [FINAL]


         El turno puede finalizar en cualquier
         estado economico.

         Un turno cancelado puede tener senia
         retenida o reembolsada.
```

---

# 13. Resumen de reglas

- Un turno nace siempre en Pendiente (excepto walk-in)
- Un walk-in nace en En curso
- Un turno cancelado no se reactiva
- Un turno finalizado no se modifica
- Toda cancelacion requiere motivo
- Todo reagendamiento registra el horario anterior y el nuevo
- Los estados economicos no bloquean los operativos
- La senia solo puede registrarse una vez
- Cada accion genera auditoria
- Cada accion puede generar una notificacion (futuro)

---

# 14. Conclusion

Este documento define el ciclo de vida completo del turno de JKTurnos.

Cubre cada estado, cada transicion, cada regla y cada consecuencia.

Sirve como guia oficial para:

- La implementacion del TurnoService
- La validacion de cada accion
- El diseno de la interfaz del Drawer
- La logica de notificaciones futuras
- La matriz de permisos del sistema

Cualquier cambio en el ciclo de vida debe actualizar este documento antes de
ser implementado.

---

Documento aprobado. Sirve como especificacion oficial del ciclo de vida del
Turno de JKTurnos.
