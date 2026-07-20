# ROADMAP.md

# JKTurnos

## Estado del proyecto

Estado general:

🟢 En desarrollo

Arquitectura:

Estable

Objetivo:

Construir un SaaS moderno de gestion de turnos adaptable a cualquier actividad.

---

# Vision

JKTurnos no busca ser un software de turnos.

Busca convertirse en el centro operativo del negocio.

Debe adaptarse al usuario y no al reves.

Debe sentirse como un asistente inteligente.

La configuracion siempre debe ser progresiva.

La interfaz debe ayudar al usuario a trabajar.

---

# Estado actual

## Infraestructura

Estado: Finalizado

✔ ASP.NET MVC 5

✔ .NET Framework 4.7

✔ Entity Framework 6

✔ SQL Server

✔ OWIN

✔ Claims

✔ Multi Tenant

✔ Repository Pattern

✔ Password Hasher

✔ TenantProvider

✔ BaseRepository

✔ BaseController

---

## Seguridad

Estado: Finalizado

✔ Login

✔ Logout

✔ Cookie Authentication

✔ Claims

✔ TenantId

✔ Roles

---

## Base de datos

Estado: Estable

Implementado:

✔ Tenant

✔ User

✔ ConfiguracionTenant

✔ Cliente

✔ Servicio

---

## CRUD

Estado

Clientes

✔ Completo

Servicios

✔ Completo

---

# Layout

Estado:

En desarrollo

Implementado

✔ Sidebar

✔ Topbar

✔ Layout Base

✔ Responsive inicial

Pendiente

☐ Sidebar Mobile Drawer

☐ Bottom Navigation

☐ Buscador Global

☐ Notificaciones

☐ Perfil

☐ Breadcrumb

---

# Home

Estado:

En desarrollo

Objetivo

Construir el Command Center del sistema.

Debe responder:

¿Que debo hacer ahora?

Implementado

✔ Bienvenida

✔ Acciones rapidas

✔ KPIs iniciales

Pendiente

☐ Mi dia

☐ Agenda inteligente

☐ Actividad reciente

☐ Recordatorios

☐ Acciones sugeridas

☐ Widgets

☐ Skeleton Loading

---

# Design System

Estado

En desarrollo

Implementado

✔ Cards

✔ Sidebar

✔ Topbar

✔ Botones iniciales

Pendiente

☐ Drawers

☐ Toasts

☐ Floating Labels

☐ Empty States

☐ Timeline

☐ Badges

☐ Chips

☐ Dialogos

☐ Loading

☐ Componentes reutilizables

---

# Modulos

## Clientes

Estado

✔ Completo

Pendiente

☐ Historial

☐ CRM ligero

☐ Busqueda inteligente

☐ Ultimo turno

☐ Proximo turno

---

## Servicios

Estado

✔ Completo

Pendiente

☐ Categoria

☐ Color

☐ Duracion avanzada

☐ Precio por recurso

---

## Recursos

Estado

No iniciado

Importante

El sistema no gira alrededor de empleados.

Gira alrededor de Recursos.

Ejemplos

Profesional

Empleado

Cancha

Consultorio

Cabina

Vehiculo

Sala

Equipo

Cualquier elemento reservable.

---

## Agenda

Estado

No iniciado

Debe ser el corazon del sistema.

Vistas

Dia

Semana

Mes

Timeline

Funciones

Drag and Drop

Crear Turno

Mover Turno

Editar Turno

Drawer lateral

Filtros

Recursos

Servicios

Estados

---

## Turnos

Estado

No iniciado

Debe ser el flujo principal del sistema.

Acciones

Nuevo

Editar

Cancelar

Completar

Confirmar

Cobrar

Seña

Notas

Historial

---

## Configuracion

Estado

No iniciado

Debe ser progresiva.

Nunca obligatoria.

El usuario configura mientras trabaja.

---

## Dashboard

Estado

Fusionado con Home

No existira un dashboard clasico.

El Home sera el Command Center.

---

# Filosofia UX

Siempre preguntar

¿Que necesita hacer el usuario ahora?

No mostrar configuraciones innecesarias.

No pedir datos que puedan cargarse despues.

No crear formularios gigantes.

Mostrar acciones.

Guiar.

Simplificar.

---

# Responsive

Desktop

Sidebar fija

Tablet

Sidebar colapsable

Mobile

Drawer

Bottom Navigation

FAB

---

# Orden de desarrollo

## Fase 1

✔ Infraestructura

✔ Seguridad

✔ Clientes

✔ Servicios

✔ Layout Base

---

## Fase 2

Home

Design System

Responsive

Componentes reutilizables

---

## Fase 3

Recursos

Agenda

Turnos

Calendario

---

## Fase 4

Configuracion del negocio

Onboarding

Recordatorios

Notificaciones

---

## Fase 5

Reportes

Estadisticas

Ingresos

Exportaciones

---

## Fase 6

Portal de reservas

Reservas online

WhatsApp

Email

Pagos

---

## Fase 7

Admin Global

Gestion SaaS

Tenants

Facturacion

Planes

Suscripciones

---

# Reglas del proyecto

Nunca romper la arquitectura.

Nunca duplicar logica.

Nunca crear consultas sin TenantId.

Nunca implementar funciones sin revisar AGENTS.md.

Toda nueva funcionalidad debe respetar:

Arquitectura

UX

Design System

Responsive

Multi Tenant

---

# Objetivo Version 1.0

Que un profesional independiente pueda:

✔ Crear clientes

✔ Crear servicios

✔ Crear turnos

✔ Organizar su agenda

✔ Cobrar

✔ Consultar ingresos

✔ Administrar su negocio desde cualquier dispositivo.

---

# Vision futura

JKTurnos debe poder administrar cualquier sistema de reservas.

No debe depender de una profesion.

Debe adaptarse a cualquier negocio basado en tiempo.

La plataforma debe crecer mediante recursos configurables, permitiendo administrar desde un profesional independiente hasta organizaciones con multiples recursos, manteniendo siempre una experiencia simple, moderna y centrada en el usuario.