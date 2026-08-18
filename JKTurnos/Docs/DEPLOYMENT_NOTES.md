# Notas sobre Publicación y Base de Datos

Este archivo documenta configuraciones críticas para el despliegue y la inicialización de la base de datos de JKTurnos.

## 1. Configuración de Base de Datos (Entity Framework)

Para evitar errores de tipo `There is already an object named 'Clientes'` o `AutomaticDataLossException`, la configuración debe ser consistente:

### 1.1. `Global.asax.cs`
Se debe desactivar el inicializador automático en el arranque para evitar que EF intente recrear las tablas cada vez que la aplicación se inicia si estas ya existen.

```csharp
// ASEGURAR QUE ESTO ESTÉ ASÍ:
Database.SetInitializer<JKContext>(null);
```

### 1.2. `Migrations/Configuration.cs`
Para permitir que Entity Framework maneje las migraciones automáticas y los cambios en el esquema (incluso si pueden causar pérdida de datos en entornos de desarrollo/pruebas), la configuración debe ser:

```csharp
public Configuration()
{
    AutomaticMigrationsEnabled = true;
    AutomaticMigrationDataLossAllowed = true;
}
```

> **Nota:** `AutomaticMigrationDataLossAllowed = true` es seguro y necesario para entornos de desarrollo y pruebas para evitar bloqueos por cambios en el modelo. En un entorno de producción real, esto debe manejarse con migraciones explícitas (`Add-Migration`) para evitar la pérdida accidental de datos.

## 2. Configuración de Producción (`Web.config`)

Para el despliegue a producción, asegúrate de que estas configuraciones estén aplicadas:

### 2.1. Desactivar Debug y Custom Errors
```xml
<system.web>
    <!-- debug debe ser false en producción -->
    <compilation debug="false" targetFramework="4.8" />
    <httpRuntime targetFramework="4.8" />
    <!-- customErrors debe ser RemoteOnly o On -->
    <customErrors mode="RemoteOnly" />
</system.web>
```

### 2.2. Cadena de Conexión
Verifica que la cadena de conexión en `Web.config` apunte al servidor SQL correcto. Actualmente apunta a una instancia local de SQL Express:

```xml
<connectionStrings>
    <add name="JKTurnosConnection"
         connectionString="Server=.\SQLEXPRESS;Database=JKTurnosDB;Trusted_Connection=True;TrustServerCertificate=True;" providerName="System.Data.SqlClient"/>
</connectionStrings>
```
## 3. Seguridad de Tokens (Anti-Forgery)

Si recibes un `HttpAntiForgeryException` al loguearte o enviar formularios, es porque la aplicación reinicia sus claves de encriptación cada vez que se reinicia el servidor.

Para solucionarlo, debes incluir una `<machineKey>` estática en `Web.config` dentro de `<system.web>`:

```xml
<system.web>
    ...
    <machineKey validationKey="[TU_VALOR_HEX_LARG]" decryptionKey="[TU_VALOR_HEX_CORTO]" validation="SHA1" decryption="AES" />
</system.web>
```
> **Nota:** Genera tus propias claves para asegurar que el token sea consistente entre reinicios del servidor. Nunca uses `AutoGenerate` (comportamiento por defecto) en entornos de producción.

