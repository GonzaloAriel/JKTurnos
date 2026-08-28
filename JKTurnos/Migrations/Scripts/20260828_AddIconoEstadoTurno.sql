-- =====================================================================
-- Migracion: AddIconoEstadoTurno
-- Fecha: 2026-08-28
-- Proposito: Agregar columna Icono a la tabla EstadoTurnoes y hacer
--            backfill de estados existentes con iconos geometricos.
-- =====================================================================

-- 1. Agregar columna Icono (nullable para no romper datos existentes)
ALTER TABLE dbo.EstadoTurnoes ADD Icono NVARCHAR(50) NULL;
GO

-- 2. Backfill: asignar iconos a los estados base genericos
UPDATE dbo.EstadoTurnoes SET Icono = 'bi-clock'      WHERE Nombre = 'Pendiente';
UPDATE dbo.EstadoTurnoes SET Icono = 'bi-play-fill'   WHERE Nombre = 'En curso';
UPDATE dbo.EstadoTurnoes SET Icono = 'bi-check2'      WHERE Nombre = 'Finalizado';
UPDATE dbo.EstadoTurnoes SET Icono = 'bi-x'           WHERE Nombre = 'Cancelado';
GO

-- 3. Backfill: estados legacy (Confirmado -> bi-play-fill, Atendido -> bi-check2)
UPDATE dbo.EstadoTurnoes SET Icono = 'bi-play-fill'   WHERE Nombre = 'Confirmado';
UPDATE dbo.EstadoTurnoes SET Icono = 'bi-check2'      WHERE Nombre = 'Atendido';
GO

-- 4. Default para estados que no coinciden con ninguno de los anteriores
UPDATE dbo.EstadoTurnoes SET Icono = 'bi-circle-fill' WHERE Icono IS NULL;
GO

-- 5. (Opcional) Hacer la columna NOT NULL despues del backfill
-- ALTER TABLE dbo.EstadoTurnoes ALTER COLUMN Icono NVARCHAR(50) NOT NULL;
-- GO

-- =====================================================================
-- Verificacion: ejecutar despues de aplicar
-- =====================================================================
/*
SELECT 
    t.Nombre AS Tenant,
    e.Nombre AS Estado,
    e.Color,
    e.Icono
FROM dbo.EstadoTurnoes e
JOIN dbo.Tenants t ON e.TenantId = t.Id
WHERE e.IsActive = 1
ORDER BY t.Nombre, e.Nombre;

-- Resultado esperado para cada tenant:
-- Pendiente  | #ffc107 | bi-clock
-- En curso   | #0d6efd | bi-play-fill
-- Finalizado | #198754 | bi-check2
-- Cancelado  | #dc3545 | bi-x
*/
