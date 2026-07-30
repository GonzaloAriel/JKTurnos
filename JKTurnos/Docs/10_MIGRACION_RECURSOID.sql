/*
  JKTurnos - Migracion: Agregar RecursoId a Turnoes
  Fecha: 27/7/2026

  Instrucciones:
  Opcion A (recomendada): Eliminar la base JKTurnosDB y ejecutar la app.
    La app recreara la BD desde cero con el nuevo esquema y los datos demo.

  Opcion B (datos existentes): Ejecutar este script en SQL Server Management Studio
    antes de iniciar la app. Luego la app detectara que el modelo coincide con
    la BD y ejecutara el Seed normalmente.

  ================================================================
*/

-- Paso 1: Agregar columna como nullable
ALTER TABLE dbo.Turnoes ADD RecursoId int NULL;

-- Paso 2: Asignar el primer recurso activo a los turnos existentes
UPDATE dbo.Turnoes
SET RecursoId = (SELECT TOP 1 Id FROM dbo.Recursoes WHERE IsActive = 1)
WHERE RecursoId IS NULL;

-- Paso 3: Hacer la columna no nullable
ALTER TABLE dbo.Turnoes ALTER COLUMN RecursoId int NOT NULL;

-- Paso 4: Agregar FK e indice
ALTER TABLE dbo.Turnoes
  ADD CONSTRAINT FK_Turnoes_Recursoes
  FOREIGN KEY (RecursoId) REFERENCES dbo.Recursoes(Id);

CREATE INDEX IX_Turnoes_RecursoId ON dbo.Turnoes(RecursoId);

-- Paso 5: Agregar estado Cancelado si no existe
IF NOT EXISTS (SELECT 1 FROM dbo.EstadoTurnoes WHERE Nombre = 'Cancelado')
BEGIN
    INSERT INTO dbo.EstadoTurnoes (TenantId, Nombre, Color, CreatedAt, IsActive)
    SELECT TOP 1 Id, 'Cancelado', '#6c757d', GETDATE(), 1 FROM dbo.Tenants;
END

PRINT 'Migracion completada: RecursoId agregado a Turnoes.';
