# TURNO LIFECYCLE

## Ciclo de Vida del Turno de JKTurnos

Version: 2.0

Fecha: 28/8/2026

Estado: Aprobado

---

# Preambulo

Este documento responde una sola pregunta:

**¿Que le puede pasar a un turno durante toda su vida?**

Documenta cada estado, cada transicion, cada regla y cada consecuencia.

No habla de base de datos.

Habla del comportamiento del negocio.

Sirve como guia para la construccion del TurnoService y de toda la logica
asociada al ciclo de vida del turno.

> Actualizado 2026-08-28 (D-040): el catalogo de estados operativos es ahora
> un conjunto FIJO del sistema (4 estados: Pendiente, En curso, Finalizado,
> Cancelado). Se eliminan los estados Confirmado y Reagendado y las acciones
> Confirmar/Reagendar que describia la version 1. La semantica se resuelve por
> `Codigo` estable, nunca por `Nombre`. La matriz canonica vive en
> `Services/TurnoEstadoMotor.cs` (regla unica).

---

# 1. Filosofia del ciclo de vida

Un turno nace, vive y muere.

Durante su vida puede cambiar de estado.

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

## 2.1 Estados operativos (catalogo FIJO del sistema, D-040)

| Codigo      | Estado      | Significado                                 |
|-------------|-------------|---------------------------------------------|
| PENDIENTE   | Pendiente   | El turno fue creado y esta por atender      |
| EN_CURSO    | En curso    | El servicio esta siendo brindado en este momento |
| FINALIZADO  | Finalizado  | El servicio fue completado                  |
| CANCELADO   | Cancelado   | El turno fue cancelado antes de finalizar   |

Los estados NO son configurables por tenant: son un catalogo fijo del sistema
(dado por D-040). La semantica se resuelve por `Codigo` (constantes en
`EstadosTurnoBase`); el `Nombre` es solo de display.

## 2.2 Situacion temporal (calculada, NO es un estado)

Ademas del estado persistido, el sistema informa una **situacion temporal**
calculada sobre la base del reloj (P7, `SituacionTemporal`). No es un estado:
el reloj informa pero NUNCA muta el estado persistido.

| Clave           | Significado                                            |
|-----------------|--------------------------------------------------------|
| demorado        | El turno no fue iniciado y su hora ya paso             |
| tiempo-excedido | El turno esta En curso y supero su horario de fin      |
| a-tiempo        | Dentro de horario (sin alerta)                         |

Las claves `demorado` y `tiempo-excedido` son de **alarma** y se pintan en las
superficies (Agenda, Home, Dashboard, Drawer) como indicador de color.

## 2.3 Estados economicos

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

# 3. Matriz de transiciones operativas (canonica)

La matriz canonica es UNICA y vive en `Services/TurnoEstadoMotor.cs`
(`EsTransicionPermitida`). Toda transicion se valida contra ella.

```
                NACE EL TURNO
                      |
                 [Reservar / Crear]
                      |
                  Pendiente
                 /        \
          [Iniciar]     [Cancelar]
             /               \
            v                 v
       En curso  ---------->  Cancelado [FINAL]
         |
    [Finalizar]
         |
         v
    Finalizado [FINAL]
```

### Transiciones permitidas

| Desde      | Accion    | Hasta      | Nota                          |
|------------|-----------|------------|-------------------------------|
| Pendiente  | Iniciar   | En curso   | Solo desde Pendiente          |
| Pendiente  | Cancelar  | Cancelado  | -                             |
| En curso   | Finalizar | Finalizado | Solo desde En curso           |
| En curso   | Cancelar  | Cancelado  | -                             |

### Transiciones bloqueadas

| Desde      | Hasta       | Motivo                                        |
|------------|-------------|-----------------------------------------------|
| Pendiente  | Finalizado  | Debe pasar por En curso primero               |
| En curso   | Pendiente   | No hay retroceso                              |
| Finalizado | Cualquiera  | Terminal: no se modifica                      |
| Cancelado  | Cualquiera  | Terminal: no se reactiva                      |

Nota: en la implementacion actual del motor, `Cancelar` acepta origen
Pendiente o En curso (guardia de estado origen). `Finalizar` solo desde
En curso. `Iniciar` solo desde Pendiente. No existen acciones Confirmar ni
Reagendar (eliminadas en D-040).

## 3.1 Transiciones economicas

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

> Nota: walk-in no esta implementado en el alcance actual (D-040). Se documenta
> como regla de negocio a futuro.

## 4.2 Cancelacion

### Flujo

```
Turno existente (Pendiente o En curso*)
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

* Solo administrador puede cancelar un turno "En curso" (matriz de permisos)
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
- Un turno Finalizado no puede cancelarse (matriz canonica)

---

# 5. Acciones del ciclo de vida

Las acciones operativas expuestas por la UI son las del estado persistido:
**Iniciar**, **Finalizar** y **Cancelar** (endpoints ajax
`IniciarAtencion`/`FinalizarAtencion`/`CancelarAtencion`). La situacion
temporal es informativa y se muestra, no se opera.

## 5.1 Reservar / Crear

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

### Validaciones
- La fecha no puede ser pasada (excepto walk-in)
- La hora debe estar dentro del horario laboral del recurso
- No debe existir otro turno en el mismo horario para el mismo recurso

## 5.2 Iniciar

### Quien
- Usuario autenticado

### Que hace
Cambia el estado de Pendiente a En curso.

### Requisitos
- El turno debe estar en estado Pendiente

### Consecuencias
- El bloque de tiempo se marca como "ocupado ahora"

### Validaciones
- No se puede iniciar un turno que no este Pendiente
- No se puede iniciar un turno de otro recurso sin permisos

## 5.3 Finalizar

### Quien
- Usuario autenticado

### Que hace
Cambia el estado de En curso a Finalizado.

### Requisitos
- El turno debe estar en estado En curso

### Consecuencias
- El servicio se marca como completado
- El recurso queda disponible para el proximo turno

## 5.4 Cancelar

### Quien
- Usuario autenticado
- (Futuro) Cliente via enlace de cancelacion

### Que hace
Cambia el estado a Cancelado.

### Requisitos
- El turno debe estar Pendiente o En curso (no Finalizado)
- Debe registrarse un motivo
- Si el turno esta En curso, solo administrador puede cancelar

### Consecuencias
- El bloque de tiempo queda disponible nuevamente
- Se registra el motivo de cancelacion
- Si habia senia, se define si se reembolsa o retiene

## 5.5 Pagar senia

### Quien
- Usuario autenticado

### Que hace
Registra una senia.

### Requisitos
- El monto debe ser menor al precio total del servicio
- No debe existir una senia previa

### Consecuencias
- El estado economico cambia a Senia pagada

## 5.6 Registrar pago

### Quien
- Usuario autenticado

### Que hace
Registra un pago parcial o completo.

### Requisitos
- El monto acumulado no debe superar el precio del servicio
- El monto debe ser mayor al ya registrado

### Consecuencias
- El estado economico se actualiza

---

# 6. Reglas generales

## 6.1 Reglas operativas

- Ningun turno puede existir sin Cliente, Servicio y Recurso
- Ningun turno puede superponerse con otro del mismo recurso
- Un turno cancelado no puede reactivarse
- Un turno finalizado no puede modificarse
- Solo un administrador puede cancelar un turno en curso
- Cada cambio de estado debe registrar fecha, hora y responsable
- El catalogo de estados es FIJO (D-040) y se resuelve por Codigo, nunca por Nombre
- No existe Confirmar: un turno nace "Pendiente" y se atendera directamente
- No existe Reagendar como cambio de estado (se agenda eliminando/creando)

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

---

# 7. Auditoria

Cada accion sobre un turno debe generar un registro de auditoria.

## Datos de auditoria por evento

| Dato             | Descripcion                                    |
|------------------|------------------------------------------------|
| Fecha y hora     | Momento exacto de la accion                    |
| Responsable      | Usuario que realizo la accion                  |
| Accion           | Tipo de accion (crear, iniciar, cancelar...)   |
| Estado anterior  | Estado del turno antes de la accion            |
| Estado posterior | Estado del turno despues de la accion          |
| Detalle          | Informacion adicional segun la accion          |

## Eventos que generan auditoria

- Creacion del turno
- Cada cambio de estado operativo (via TurnoEstadoMotor)
- Cada cambio de estado economico
- Cada cancelacion (con motivo)
- Cada pago registrado (con monto)

---

# 8. Notificaciones futuras

Cuando el modulo de comunicacion este implementado, cada accion debera
disparar notificaciones.

## Mapa de notificaciones

| Accion           | Notificacion                         | Destinatario     |
|------------------|--------------------------------------|------------------|
| Crear/Reservar   | Turno por atender                    | Cliente          |
| Recordatorio     | Recordatorio de turno proximo        | Cliente          |
| Cancelar         | Turno cancelado                      | Cliente          |
| Iniciar          | Servicio en curso                    | Cliente          |
| Finalizar        | Servicio completado                  | Cliente          |

Los canales de notificacion seran definidos cuando el modulo se implemente.

---

# 9. Matriz de permisos

| Accion           | Admin | Operador | (Futuro) Cliente |
|------------------|-------|----------|------------------|
| Crear/Reservar   | Si    | Si       | Si               |
| Iniciar          | Si    | Si       | No               |
| Finalizar        | Si    | Si       | No               |
| Cancelar         | Si    | Si (*)   | Si (**)          |
| Pagar senia      | Si    | Si       | No               |
| Registrar pago   | Si    | Si       | No               |

(*) Operador no puede cancelar turnos En curso
(**) Cliente solo puede cancelar dentro de la ventana definida por el negocio

---

# 10. Reglas de validacion por accion

## Reservar / Crear

```
- Fecha no puede ser pasada
- Hora debe ser valida (dentro de horario del recurso)
- Recurso debe estar activo
- Servicio debe estar activo
- Cliente debe existir (o crearse con datos minimos)
- No debe existir conflicto de horario con el mismo recurso
```

## Iniciar

```
- Turno debe estar en estado Pendiente
- Fecha del turno controlada por la situacion temporal (informativa)
```

## Finalizar

```
- Turno debe estar en estado En curso
```

## Cancelar

```
- Turno debe estar Pendiente o En curso (no Finalizado)
- Motivo obligatorio
- Si esta En curso, solo Admin
- Si hay senia, preguntar reembolso/retencion
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
                           /        \
                    [Iniciar]     [Cancelar]
                      /              \
                     v                v
                 En curso        Cancelado [FINAL]
                   |
             [Finalizar]
                   |
                   v
             Finalizado [FINAL]


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
- El catalogo de estados es FIJO (D-040) y se resuelve por Codigo
- No existen Confirmar ni Reagendar (eliminados en D-040)
- La situacion temporal es calculada e informativa, nunca muta el estado
- Toda cancelacion requiere motivo
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
