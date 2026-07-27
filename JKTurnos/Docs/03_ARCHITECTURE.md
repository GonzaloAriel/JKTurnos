# ARCHITECTURE.md

# Arquitectura de JKTurnos

Version

1.0

---

# Objetivo

Este documento define la arquitectura tecnica del sistema.

Toda nueva funcionalidad debe respetar estas reglas.

No modificar la arquitectura sin aprobacion.

---

# Stack

ASP.NET MVC 5

.NET Framework 4.7

Entity Framework 6

SQL Server Express

Bootstrap

OWIN Cookie Authentication

Repository Pattern

Monolito MVC

Multi Tenant

---

# Arquitectura General

Usuario

↓

Controller

↓

Service

↓

Repository

↓

Entity Framework

↓

SQL Server

Nunca romper este flujo.

---

# Responsabilidades

## Controller

Recibir Requests.

Validar ModelState.

Llamar Services.

Retornar Views.

Nunca contener logica de negocio.

---

## Service

Contener todas las reglas del negocio.

Validaciones.

Calculos.

Procesos.

Nunca acceder directamente a la interfaz.

---

## Repository

Acceso a datos.

Consultas.

Persistencia.

Filtrado por Tenant.

Nunca contener reglas del negocio.

---

## Models

Representan entidades de la base de datos.

No contienen logica.

---

# Multi Tenant

Todo el sistema trabaja mediante TenantId.

Cada entidad pertenece a un Tenant.

Nunca mezclar informacion entre tenants.

Todo acceso debe pasar por BaseRepository.

---

# Flujo de autenticacion

Usuario

↓

Login

↓

Validacion BD

↓

Claims

↓

ClaimsIdentity

↓

OWIN

↓

Cookie

↓

Browser

↓

Nueva Request

↓

OWIN valida Cookie

↓

User.Identity

↓

TenantProvider

↓

Repository

↓

Datos filtrados

---

# Claims utilizadas

NameIdentifier

Name

Email

Role

TenantId

---

# TenantProvider

Responsabilidad

Obtener TenantId desde User.Identity.

Nunca consultar TenantId desde la base.

---

# BaseRepository

Debe filtrar automaticamente

TenantId

IsActive

Implementa

GetAll()

GetById()

Add()

Update()

Delete()

Soft Delete

---

# BaseController

Responsabilidad

Compartir informacion comun.

No agregar logica especifica de negocio.

---

# DbContext

JKContext

Responsabilidades

DbSets

Configuracion EF

Migraciones

SaveChanges()

---

# Migraciones

AutomaticMigrationsEnabled = true

Seed obligatorio.

---

# Seguridad

OWIN Cookie Authentication.

Sin ASP.NET Identity.

Password BCrypt.

Claims.

Authorize.

---

# Convenciones

Controllers

Services

Repositories

Infrastructure

Helpers

Models

ViewModels

Views

Mantener esta estructura.

---

# Reglas

No usar SQL directo.

No duplicar codigo.

No crear dependencias circulares.

No romper el flujo MVC.

Toda nueva funcionalidad debe respetar esta arquitectura.

---

Arquitectura validada - 26/7/2026

Módulos implementados utilizando el patrón oficial:

✓ Clientes
✓ Servicios
✓ Recursos

Conclusión:

El patrón CRUD oficial queda aprobado para el proyecto JKTurnos.

Todo nuevo módulo CRUD deberá seguir esta estructura.

No crear nuevos componentes salvo necesidad comprobada.

No duplicar HTML existente.

Priorizar siempre reutilización sobre especialización.