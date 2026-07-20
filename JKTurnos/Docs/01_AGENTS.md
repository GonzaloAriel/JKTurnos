# AGENTS.md

# JKTurnos

## Vision del producto

JKTurnos NO es un CRUD de turnos.

JKTurnos es un SaaS moderno orientado a la gestion del tiempo y la operacion diaria de pequeños y medianos negocios.

El objetivo principal es reducir la carga cognitiva del usuario.

El sistema debe sentirse como un asistente inteligente.

El usuario nunca debe sentir que esta configurando un software.

Debe sentir que esta trabajando.

La configuracion siempre debe ser progresiva.

La experiencia debe ser fluida.

---

# Filosofia del producto

Siempre pensar como Product Designer.

Antes de agregar una pantalla preguntarse:

"¿El usuario realmente necesita esto?"

Antes de agregar un formulario preguntarse:

"¿Podemos pedir esta informacion mas adelante?"

Antes de crear una configuracion preguntarse:

"¿Podemos inferir este dato automaticamente?"

Siempre priorizar:

Experiencia

Velocidad

Simplicidad

Claridad

---

# Stack Tecnologico

ASP.NET MVC 5

.NET Framework 4.7

Entity Framework 6

SQL Server Express

OWIN Cookie Authentication

Bootstrap

Repository Pattern

Monolito MVC

Multi-Tenant por TenantId

NO usar:

ASP.NET Identity

.NET Core

Minimal APIs

Dependency Injection

MediatR

CQRS

Microservicios

Mantener el proyecto simple.

---

# Arquitectura

La arquitectura debe mantenerse consistente.

Controllers

↓

Services

↓

Repositories

↓

Entity Framework

↓

SQL Server

Nunca acceder a la base desde un Controller.

Toda logica de negocio pertenece a Services.

Toda consulta pertenece a Repositories.

---

# Multi Tenant

Toda entidad del negocio implementa TenantId.

Nunca crear consultas que omitan TenantId.

Toda consulta debe respetar el aislamiento entre negocios.

Nunca utilizar SQL directo salvo que sea estrictamente necesario.

Si se necesita SQL complejo debe encapsularse en un Repository.

---

# Autenticacion

Se utiliza OWIN Cookie Authentication.

Claims actuales:

NameIdentifier

Name

Email

Role

TenantId

TenantProvider obtiene TenantId desde Claims.

Nunca consultar el Tenant en cada Request.

La Cookie contiene la identidad.

---

# BaseRepository

Debe utilizarse siempre.

Funciones actuales:

GetAll()

GetById()

Add()

Update()

Delete()

Filtrado automatico por:

TenantId

IsActive

Soft Delete obligatorio.

---

# Convenciones

Siempre indicar:

Archivo

Carpeta

Que hace

Como funciona

Codigo completo

Nunca asumir que existe un archivo.

Siempre verificar.

Nunca crear archivos duplicados.

Antes de modificar:

Buscar referencias.

Analizar impacto.

---

# Convenciones de codigo

Codigo limpio.

Metodos pequeños.

Responsabilidad unica.

Nombres descriptivos.

No duplicar logica.

No escribir codigo muerto.

No dejar comentarios innecesarios.

---

# Convenciones MVC

Los Controllers solo coordinan.

Los Services contienen reglas.

Los Repositories consultan datos.

Las Views solo muestran informacion.

---

# Convenciones UI

No usar tablas salvo que realmente agreguen valor.

Priorizar Cards.

Sidebar limpia.

Topbar limpia.

Mucho espacio.

Diseño minimalista.

Inspiraciones:

Apple

Linear

Stripe

Notion

Arc

Vercel

No copiar.

Solo inspirarse.

---

# Responsive

Mobile First.

Desktop.

Tablet.

Mobile.

La Sidebar:

Desktop fija.

Tablet colapsable.

Mobile Drawer.

---

# Experiencia de Usuario

El usuario nunca debe sentirse perdido.

Siempre mostrar la siguiente accion.

Evitar pantallas vacias.

Mostrar acciones utiles.

Mostrar estados vacios amigables.

No abusar de modales.

Priorizar Drawers.

---

# Filosofia del Home

El Home es un Command Center.

No es un Dashboard tradicional.

Debe responder:

¿Que debo hacer ahora?

No mostrar grandes reportes.

Mostrar acciones.

Mostrar agenda.

Mostrar prioridades.

---

# Recursos

El sistema NO gira alrededor de Empleados.

Gira alrededor de Recursos.

Un recurso puede ser:

Profesional

Empleado

Cancha

Consultorio

Cabina

Vehiculo

Sala

Equipo

Cualquier elemento que pueda recibir reservas.

La interfaz debe hablar de "Recursos".

Siempre explicar al usuario mediante ayudas.

---

# Onboarding

El usuario debe poder comenzar inmediatamente.

No obligarlo a configurar todo.

Primera accion:

Crear Turno.

Despues:

Agregar Cliente.

Despues:

Agregar Servicio.

Luego:

Configurar negocio.

La configuracion debe ser progresiva.

---

# Componentes reutilizables

Todo componente debe ser reutilizable.

Cards

Inputs

Botones

Sidebar

Topbar

Drawer

Toast

Badges

Timeline

Estados vacios

KPIs

No duplicar HTML.

---

# CSS

Separar responsabilidades.

jk-layout.css

jk-components.css

jk-dashboard.css

No utilizar estilos inline.

---

# JavaScript

Separar logica.

jk-layout.js

jk-dashboard.js

No escribir JavaScript dentro de Razor.

---

# Textos

Todos los textos deben escribirse SIN ACENTOS.

Motivo:

Compatibilidad de encoding.

---

# Antes de implementar

Siempre:

Analizar el proyecto.

Buscar referencias.

Leer archivos relacionados.

Entender la arquitectura.

---

# Antes de modificar

Nunca asumir.

Leer primero.

Analizar impacto.

Buscar quien consume ese codigo.

---

# Despues de implementar

Compilar.

Corregir errores.

Verificar referencias.

Verificar que no se rompa la arquitectura.

---

# Si existe una duda

NO tomar decisiones de arquitectura.

Detenerse.

Explicar el problema.

Proponer alternativas.

Esperar aprobacion.

---

# Objetivo final

Construir un SaaS moderno.

Escalable.

Simple.

Rapido.

Elegante.

Que cualquier usuario pueda utilizar sin capacitacion.

Cada decision debe responder a una pregunta:

"¿Esto hace mas facil el trabajo del usuario?"