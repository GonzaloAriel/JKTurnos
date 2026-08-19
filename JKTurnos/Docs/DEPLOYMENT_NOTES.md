# Publicación y Base de Datos

Guía práctica para saber **qué configurar al trabajar en local** y **qué cambiar al publicar**. Si algo no figura aquí, no hay que tocarlo.

---

## Resumen en una tabla

| Archivo / Configuración | LOCAL (desarrollo) | PRODUCCIÓN (publicar) | Cómo se aplica |
|---|---|---|---|
| `Global.asax.cs` → `Database.SetInitializer(new MigrateDatabaseToLatestVersion<JKContext, Configuration>())` | Igual | **Igual, no tocar** | Manual |
| `Migrations/Configuration.cs` → `AutomaticMigrationsEnabled` | `false` | `false` | Ya cambiado |
| `Migrations/Configuration.cs` → `AutomaticMigrationDataLossAllowed` | `false` | `false` | Ya cambiado |
| `Web.config` → `compilation debug` | `true` | `false` | Automático (transform de Release) |
| `Web.config` → `customErrors mode` | `RemoteOnly` | `On` | Manual o transform |
| `Web.config` → cadena de conexión | SQL Express local | Servidor SQL del hosting | Manual o transform |
| `Web.config` → `machineKey` | Igual | **Igual, no tocar** | Manual |

---

## Qué NO se toca nunca

- **`Global.asax.cs`**: `Database.SetInitializer(new MigrateDatabaseToLatestVersion<JKContext, Configuration>())` debe quedarse así en local y en producción. Es lo que hace que la app **actualice sola la base de datos** al arrancar: aplica las migraciones pendientes y ejecuta el Seed (idempotente). No volver a `SetInitializer(null)` (ahí la app no crea nada).
- **`machineKey`** (ya existe en `Web.config`): es estática a propósito. Si la cambias, los tokens anti-forgery y las sesiones se invalidan en cada reinicio del servidor.

---

## 1. Para trabajar en LOCAL (desarrollo)

**No hay que cambiar nada: los archivos ya están listos para local.**

### Qué significa cada cosa en local
- `AutomaticMigrationsEnabled = false` + `AutomaticMigrationDataLossAllowed = false`: el esquema se controla con migraciones explícitas. Sin pérdida de datos accidental.
- Cadena de conexión local (solo si querés volver a desarrollo): `Server=.\SQLEXPRESS;Database=JKTurnosDB`.
- `debug="true"`: muestra el detalle de errores mientras desarrollás.

### Crear / actualizar la base de datos local
Como el inicializador está desactivado, la BD **no se crea sola**: hay que correr `Update-Database` manualmente desde la Consola del Administrador de Paquetes:

```
Update-Database
```

- Con migraciones explícitas, `Update-Database` aplica las migraciones pendientes al esquema. La BD local ya está al día (no hace falta volver a correrla salvo nuevos cambios).
- Si querés un control fino, escribí una migración explícita antes:

```
Add-Migration NombreDeLaMigracion
Update-Database
```

El `Seed` se ejecuta con cada `Update-Database` y crea:
- Tenant demo `local`.
- `admin@jkturnos.com` / `123456` (rol Admin) y `global@jkturnos.com` / `123456` (rol AdminGlobal).
- `ConfiguracionTenant`, 3 estados de turno, y datos demo (clientes, servicios, recursos, turnos).

---

## 2. Para PUBLICAR (producción)

Antes de publicar, revisá estos 3 puntos:

### a) `Web.config`
1. **`debug`**: debe quedar `false`. Ya está resuelto: `Web.Release.config` elimina el atributo `debug` automáticamente al publicar en modo **Release**. Solo asegurate de publicar en **Release** y no en Debug.
2. **`customErrors`**: cambiá `mode="RemoteOnly"` por `mode="On"` para que los usuarios no vean el detalle de errores.
3. **Cadena de conexión**: ya apunta al servidor SQL del hosting (`Server=sql2016;Database=w370423_jkturnos;...`). Ese valor viaja en el publish tal cual está en `Web.config`; no hace falta transform.

### b) `Migrations/Configuration.cs`
Cambiá el constructor para **desactivar las migraciones automáticas** y controlar el esquema con migraciones explícitas:

```csharp
public Configuration()
{
    AutomaticMigrationsEnabled = false;
    AutomaticMigrationDataLossAllowed = false;
}
```

Con esto, `Update-Database` solo aplica las migraciones explícitas que escribiste (`Add-Migration`), y nunca va a borrar columnas o datos por accidente.

### c) `machineKey`
No lo cambies. Ya está configurada.

---

## 3. Cómo crear / actualizar la base de datos de producción

> **La aplicación actualiza sola la base de datos.** `Global.asax.cs` usa `MigrateDatabaseToLatestVersion`: en cada arranque aplica las migraciones pendientes y corre el Seed. Para implementar un módulo nuevo alcanza con `Add-Migration` en tu PC + publicar: la BD del hosting se actualiza sola en el primer request. No se borra la BD, no se corre SQL a mano.

### Primera vez (la BD del hosting ya tiene tablas viejas)

La BD actual fue creada con otra cadena de migraciones. Como la cadena se reescribió en una **migración única `Inicial`**, hay que vaciarla **una sola vez**:

1. En el panel del hosting (phpMyAdmin / SQL Manager de `w370423_jkturnos`), ejecutá el script `Docs/host_vaciar.sql` (borra todas las tablas, incluida `__MigrationHistory`). Alternativa manual: drop + recreate de la BD si el panel lo permite.
2. Publicá la aplicación (modo **Release**).
3. Primer acceso a la app → crea el esquema completo (migración `Inicial`) y el Seed: tenant `JK Demo`, `admin@jkturnos.com` / `123456` (Admin), `global@jkturnos.com` / `123456` (AdminGlobal), config y 3 estados.
4. Ingresá con `global@jkturnos.com` / `123456` para crear tenants.

> Si preferís crear el esquema a mano antes de publicar: corré `Docs/host_schema.sql` (migración única `Inicial`) y después `Docs/host_bootstrap.sql` (datos base, idempotente). Con la auto-migración activa no hace falta; ambos scripts son la vía alternativa.

### De ahí en adelante (cada módulo)

1. En tu PC: `Add-Migration NombreDelCambio` (genera la migración con su `Designer.cs`) y verificá en local con `Update-Database`.
2. Publicá en **Release**.
3. Primer request en el hosting → la app aplica la migración nueva sola. Sin scripts, sin borrar nada.

---

## 4. Checklist final antes de publicar

- [ ] Publicar en modo **Release** (garantiza `debug=false`).
- [ ] `customErrors mode="On"` en producción (hoy `Web.Release.config` deja `RemoteOnly`; ajustar a mano en el hosting o en el transform).
- [ ] Cadena de conexión apuntando al servidor de producción (ya está: `sql2016` / `w370423_jkturnos`).
- [ ] `Global.asax.cs` con `MigrateDatabaseToLatestVersion` (ya está).
- [ ] `AutomaticMigrationsEnabled = false` en `Configuration.cs` (ya está).
- [ ] Primera vez: vaciar la BD del hosting (borrar todas las tablas) antes de publicar.
- [ ] `machineKey` intacta.