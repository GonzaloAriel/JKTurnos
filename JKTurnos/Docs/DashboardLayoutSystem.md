# DashboardLayoutSystem.md

# JKTurnos Dashboard Layout System
### Especificación Oficial del Sistema Visual y de Interacción

Versión: 1.0

Estado: Activo

Este documento define la arquitectura visual, la experiencia de usuario y las reglas de composición de JKTurnos.

Toda nueva pantalla, componente o módulo deberá respetar estas especificaciones.

---

# 1. Filosofía del Producto

JKTurnos no es un sistema administrativo tradicional.

No queremos construir un conjunto de CRUDs.

Queremos construir un Centro de Gestión Inteligente.

Cada pantalla debe ayudar al usuario a trabajar mejor, pensar menos y realizar sus tareas más rápido.

El sistema debe sentirse como un asistente que acompaña al usuario durante su jornada.

La interfaz debe transmitir:

- claridad
- orden
- calma
- velocidad
- confianza

Nunca debe sentirse pesada ni burocrática.

---

# 2. Filosofía de UX

Toda pantalla debe responder inmediatamente estas preguntas:

¿Dónde estoy?

¿Qué puedo hacer?

¿Cuál es la acción principal?

¿Qué debería hacer ahora?

El usuario nunca debe sentirse perdido.

Siempre debe existir una acción claramente destacada.

---

# 3. Arquitectura General

Toda pantalla utilizará la misma estructura.

Topbar

↓

Sidebar

↓

Page Header

↓

Contenido Principal

↓

Acciones secundarias

Nunca se modificarán estas zonas.

---

# 4. Layout Principal

La aplicación utiliza un AppShell.

El AppShell está compuesto por:

Sidebar

Topbar

Content Area

Overlay Mobile

Drawer Layer

Toast Layer

Modal Layer

Todos los módulos se renderizan únicamente dentro del Content Area.

---

# 5. Responsive

Desktop

Sidebar expandida

Topbar completa

Content ancho

Tablet

Sidebar colapsable

Topbar simplificada

Drawer lateral

Mobile

Sidebar oculta

Hamburguesa

Bottom Sheet

FAB para acciones rápidas

El comportamiento responsive pertenece al Layout, nunca a los módulos.

---

# 6. Identidad Visual

Inspiración

Apple

Notion

Linear

Stripe

Vercel

La prioridad es:

claridad > decoración

No se utilizan efectos excesivos.

No se utilizan gradientes fuertes.

No se utilizan sombras agresivas.

Todo debe sentirse limpio.

---

# 7. Sistema de Colores

Los colores provienen únicamente de:

jk-tokens.css

Nunca utilizar colores hardcodeados.

Siempre utilizar variables CSS.

Ejemplo

--jk-accent

--jk-bg

--jk-surface

--jk-border

--jk-text

--jk-success

--jk-warning

--jk-danger

---

# 8. Tipografía

Fuente principal

Inter

Jerarquía

H1

H2

H3

Body

Small

Label

Caption

Nunca utilizar tamaños arbitrarios.

Siempre utilizar la escala del Design System.

---

# 9. Espaciado

El sistema utiliza una escala basada en 8px.

Todos los márgenes y paddings deben respetar esta escala.

Nunca utilizar valores aleatorios.

---

# 10. Componentes

Todos los componentes reutilizables pertenecen al Design System.

Ejemplos

Botones

Cards

Inputs

Search

Table

Badge

Avatar

Drawer

Timeline

Toast

Modal

Page Header

Section Header

Action Card

KPI Card

Empty State

Floating Button

No se deben crear componentes duplicados.

---

# 11. Componentes Inteligentes

Los componentes deben resolver un problema.

No deben ser únicamente HTML reutilizado.

Ejemplo

ActionCard

Debe aceptar:

Título

Descripción

Icono

Color

URL

Estado

CTA

Badge

Los componentes deben crecer.

Las pantallas no.

---

# 12. Page Header

Toda pantalla comienza con un PageHeader.

Debe contener:

Título

Subtítulo

Acción Principal

Acciones Secundarias (opcional)

Nunca debe omitirse.

---

# 13. Empty States

Nunca mostrar una pantalla vacía.

Toda ausencia de datos debe explicar:

qué sucede

por qué sucede

qué hacer ahora

Todo Empty State debe incluir una acción.

Ejemplo

"No tienes clientes."

↓

"Crear Cliente"

---

# 14. Acciones

Siempre existe una única acción primaria.

Nunca deben existir dos botones primarios compitiendo entre sí.

Las acciones secundarias deben utilizar estilo Ghost.

---

# 15. Navegación

La Sidebar representa la estructura del negocio.

No representa tablas de base de datos.

Debe hablar el lenguaje del usuario.

Ejemplo

Agenda

Clientes

Servicios

Recursos

Negocio

Configuración

Nunca

CRUD

Tabla

Administración

---

# 16. Drawers

Los Drawers serán el método principal de edición.

Se utilizarán para:

editar cliente

editar turno

editar servicio

editar recurso

editar negocio

Evitar abrir páginas completas.

---

# 17. Modales

Los modales únicamente se utilizarán para:

confirmaciones

advertencias

acciones críticas

Nunca para formularios largos.

---

# 18. Tablas

Las tablas son un componente secundario.

Siempre que sea posible se priorizarán:

Cards

Timeline

Widgets

Solo utilizar tablas cuando la densidad de información lo justifique.

---

# 19. Dashboard

El Dashboard es el Command Center.

No es un panel de estadísticas.

Debe responder:

¿Qué debo hacer hoy?

¿Qué está ocurriendo?

¿Qué requiere mi atención?

Debe contener únicamente información accionable.

---

# 20. Filosofía del Home

Orden recomendado

Bienvenida

Acciones rápidas

Mi Día

Agenda

KPIs

Sugerencias

Actividad

Nunca convertir el Home en un reporte.

---

# 21. Sistema de Recursos

JKTurnos no administra empleados.

Administra Recursos.

Un recurso puede representar:

Profesional

Empleado

Consultorio

Cabina

Cancha

Sala

Vehículo

Equipo

Máquina

El lenguaje utilizado debe ser comprensible para cualquier actividad.

---

# 22. Evolución del Producto

El sistema debe adaptarse automáticamente al crecimiento del usuario.

Profesional Independiente

↓

Equipo

↓

Empresa

Sin cambiar la experiencia general.

---

# 23. Arquitectura Técnica

Backend

ASP.NET MVC 5

.NET Framework 4.7

Entity Framework 6

Frontend

Bootstrap

Design System propio

Razor Components

CSS Tokens

No introducir frameworks SPA.

---

# 24. Reglas para IA

Toda IA que genere código para JKTurnos deberá respetar estas reglas.

No crear HTML repetido.

No romper el Design System.

No introducir nuevos estilos fuera del sistema.

No utilizar colores hardcodeados.

No duplicar componentes.

Priorizar reutilización.

Priorizar legibilidad.

Mantener el backend existente.

---

# 25. Roadmap del Frontend

Fase 1

Design Tokens

✔

Fase 2

Layout

✔

Fase 3

Componentes

✔

Fase 4

Home

En desarrollo

Fase 5

Clientes

Pendiente

Fase 6

Servicios

Pendiente

Fase 7

Recursos

Pendiente

Fase 8

Agenda

Pendiente

Fase 9

Negocio

Pendiente

Fase 10

Analytics

Pendiente

---

# 26. Objetivo Final

El objetivo no es construir un software de turnos.

El objetivo es construir el Centro de Gestión más intuitivo para profesionales, comercios y empresas de servicios.

Cada decisión de diseño deberá acercar el producto a esa visión.