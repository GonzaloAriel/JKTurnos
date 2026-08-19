/*
  JKTurnos - Vaciar la BD del hosting (UNA SOLA VEZ).
  Base: w370423_jkturnos (servidor sql2016).
  Borra TODAS las tablas (incluida __MigrationHistory). DESTRUCTIVO: pierde todos los datos.

  Por que? La BD actual fue creada con una cadena de migraciones vieja.
  La nueva version usa una migracion unica Inicial + auto-migracion (MigrateDatabaseToLatestVersion).
  Al borrar todo, la app recrea el esquema y el Seed en el primer acceso.
  Despues de esto, ya no hace falta vaciar nada mas: cada deploy actualiza la BD sola.
*/
USE [w370423_jkturnos];
GO

-- 1) Quitar todos los foreign keys (para poder dropear tablas en cualquier orden)
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = @sql +
    N'ALTER TABLE ' +
    QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
    N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(10)
FROM sys.foreign_keys fk;

IF @sql <> N''
    EXEC sp_executesql @sql;

-- 2) Dropear todas las tablas
SET @sql = N'';

SELECT @sql = @sql +
    N'DROP TABLE ' +
    QUOTENAME(OBJECT_SCHEMA_NAME(t.object_id)) + N'.' + QUOTENAME(t.name) + N';' + CHAR(10)
FROM sys.tables t;

IF @sql <> N''
    EXEC sp_executesql @sql;

-- 3) Verificar que no quede ninguna tabla
SELECT name FROM sys.tables;

GO