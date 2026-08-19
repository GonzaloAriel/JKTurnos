/*
  JKTurnos - Datos base para hosting.
  Base: w370423_jkturnos (servidor sql2016).
  Ejecutar DESPUES de host_schema.sql (o sobre una BD con el esquema ya creado).
  Idempotente: puede correrse varias veces sin duplicar datos.

  Crea:
    - Tenant 'JK Demo' (Subdominio 'local')
    - Usuario admin@jkturnos.com  (Admin, tenant 1, pass 123456)
    - Usuario global@jkturnos.com (AdminGlobal, TenantId 0, pass 123456)
    - Configuracion del tenant
    - 3 Estados de turno (Pendiente / Confirmado / Atendido)
*/
USE [w370423_jkturnos];
GO

-- 1. TENANT
IF NOT EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE Subdominio = N'local')
BEGIN
    INSERT INTO [dbo].[Tenants] ([Nombre], [Subdominio], [CreatedAt], [UpdatedAt], [IsActive])
    VALUES (N'JK Demo', N'local', GETDATE(), NULL, 1);
END

DECLARE @tenantId INT = (SELECT Id FROM [dbo].[Tenants] WHERE Subdominio = N'local');

-- 2. USERS
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE Email = N'admin@jkturnos.com' AND TenantId = @tenantId)
BEGIN
    INSERT INTO [dbo].[Users] ([Email], [PasswordHash], [Nombre], [Role], [TenantId], [CreatedAt], [UpdatedAt], [IsActive])
    VALUES (N'admin@jkturnos.com', N'$2a$11$4M2MsCBZXnkVZWBQdS79DuBtJWZQCQRlIriSB8yjJ9op7SRcTy6.6', N'Administrador', N'Admin', @tenantId, GETDATE(), NULL, 1);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE Email = N'global@jkturnos.com')
BEGIN
    INSERT INTO [dbo].[Users] ([Email], [PasswordHash], [Nombre], [Role], [TenantId], [CreatedAt], [UpdatedAt], [IsActive])
    VALUES (N'global@jkturnos.com', N'$2a$11$4M2MsCBZXnkVZWBQdS79DuBtJWZQCQRlIriSB8yjJ9op7SRcTy6.6', N'Administrador Global', N'AdminGlobal', 0, GETDATE(), NULL, 1);
END

-- 3. CONFIGURACION TENANT
IF NOT EXISTS (SELECT 1 FROM [dbo].[ConfiguracionTenants] WHERE TenantId = @tenantId)
BEGIN
    INSERT INTO [dbo].[ConfiguracionTenants] ([TenantId], [NombreComercial], [Telefono], [Email], [Direccion], [LogoUrl], [CreatedAt], [UpdatedAt], [IsActive])
    VALUES (@tenantId, N'JKTurnos Demo', N'', N'admin@jkturnos.com', N'', NULL, GETDATE(), NULL, 1);
END

-- 4. ESTADOS DE TURNO
IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadoTurnoes] WHERE TenantId = @tenantId AND Nombre = N'Pendiente')
BEGIN
    INSERT INTO [dbo].[EstadoTurnoes] ([TenantId], [Nombre], [Color], [CreatedAt], [UpdatedAt], [IsActive])
    VALUES (@tenantId, N'Pendiente', N'#ffc107', GETDATE(), NULL, 1);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadoTurnoes] WHERE TenantId = @tenantId AND Nombre = N'Confirmado')
BEGIN
    INSERT INTO [dbo].[EstadoTurnoes] ([TenantId], [Nombre], [Color], [CreatedAt], [UpdatedAt], [IsActive])
    VALUES (@tenantId, N'Confirmado', N'#0d6efd', GETDATE(), NULL, 1);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[EstadoTurnoes] WHERE TenantId = @tenantId AND Nombre = N'Atendido')
BEGIN
    INSERT INTO [dbo].[EstadoTurnoes] ([TenantId], [Nombre], [Color], [CreatedAt], [UpdatedAt], [IsActive])
    VALUES (@tenantId, N'Atendido', N'#198754', GETDATE(), NULL, 1);
END

GO