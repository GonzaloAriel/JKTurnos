# Guía: Cambio de Entorno (Local <-> Producción)

Esta guía detalla los cambios necesarios para alternar entre el entorno de desarrollo local y el entorno de hosting (producción) para la aplicación JKTurnos.

## Resumen de cambios

| Archivo | Local (Desarrollo) | Producción (Host) |
| :--- | :--- | :--- |
| `Web.config` | `debug="true"`, `customErrors="Off"` | `debug="false"`, `customErrors="RemoteOnly"` |
| `Web.config` | Cadena conexión local | Cadena conexión del host |
| `Global.asax.cs` | (Generalmente `null`) | `Database.SetInitializer<JKContext>(null);` |

---

## 1. Configuración de `Web.config`

### 1.1. Modo Debug y Errores
Para desarrollo, queremos ver detalles técnicos. Para producción, debemos ocultarlos por seguridad.

*   **Local:**
    ```xml
    <compilation debug="true" targetFramework="4.8" />
    <customErrors mode="Off" />
    ```
*   **Producción:**
    ```xml
    <compilation debug="false" targetFramework="4.8" />
    <customErrors mode="RemoteOnly" />
    ```

### 1.2. Cadena de Conexión
Asegúrate de que la conexión apunte al servidor correcto.

*   **Local:**
    ```xml
    <connectionStrings>
        <add name="JKTurnosConnection" connectionString="Server=.\SQLEXPRESS;Database=JKTurnosDB;..." />
    </connectionStrings>
    ```
*   **Producción:**
    Debes cambiar `Server` y posiblemente `Database`, `User` y `Password` según los datos que te proporcione tu proveedor de hosting.

---

## 2. Configuración de `Global.asax.cs`

Para evitar errores de "Ya existe un objeto con nombre X" en el host, la configuración recomendada es deshabilitar el inicializador automático.

```csharp
// ASEGURAR QUE ESTÉ ASÍ EN AMBOS ENTORNOS SI LA BD YA EXISTE:
Database.SetInitializer<JKContext>(null);
```

Si por alguna razón en local **necesitas** que la BD se recree automáticamente al cambiar el modelo, puedes usar temporalmente:
```csharp
// SOLO PARA DESARROLLO SI QUIERES RECREAR LA BD
Database.SetInitializer(new MigrateDatabaseToLatestVersion<JKContext, Configuration>());
```

---

## 3. Configuración de Migraciones (`Migrations/Configuration.cs`)

Esta configuración debe permanecer así para ambos entornos, ya que permite que Entity Framework maneje cambios en el esquema sin bloqueos:

```csharp
public Configuration()
{
    AutomaticMigrationsEnabled = true;
    AutomaticMigrationDataLossAllowed = true;
}
```
