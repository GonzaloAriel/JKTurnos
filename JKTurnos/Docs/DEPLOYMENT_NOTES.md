# Notas sobre Publicación y Base de Datos

Este archivo documenta configuraciones críticas para el despliegue y la inicialización de la base de datos de JKTurnos.

## Configuración de Base de Datos (Entity Framework)

Para evitar errores de tipo `There is already an object named 'Clientes'` o `AutomaticDataLossException`, la configuración debe ser consistente:

### 1. `Global.asax.cs`
Se debe desactivar el inicializador automático en el arranque para evitar que EF intente recrear las tablas cada vez que la aplicación se inicia si estas ya existen.

```csharp
// ASEGURAR QUE ESTO ESTÉ ASÍ:
Database.SetInitializer<JKContext>(null);
```

### 2. `Migrations/Configuration.cs`
Para permitir que Entity Framework maneje las migraciones automáticas y los cambios en el esquema (incluso si pueden causar pérdida de datos en entornos de desarrollo/pruebas), la configuración debe ser:

```csharp
public Configuration()
{
    AutomaticMigrationsEnabled = true;
    AutomaticMigrationDataLossAllowed = true;
}
```

> **Nota:** `AutomaticMigrationDataLossAllowed = true` es seguro y necesario para entornos de desarrollo y pruebas para evitar bloqueos por cambios en el modelo. En un entorno de producción real, esto debe manejarse con migraciones explícitas (`Add-Migration`) para evitar la pérdida accidental de datos.
