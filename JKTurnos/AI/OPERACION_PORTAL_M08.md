# Operación y Producción del Portal de Reservas (M-08)

Documento operativo para volver a ejecutar el sistema en local, llevar el portal a
producción y comunicar la dirección del portal a los clientes del negocio.

Referencias: `AI/PROMPT_M08_PORTAL_TURNOS.md` (implementación y diagnóstico) y
`AI/SESION_20260901_PLAN_M08_PORTAL.md` (decisión Solución A).

---

## 1. Volver a correr el sistema en local "como estaba"

El modo dev actual usa la **Solución A**: un incluye un fallback que resuelve el tenant
`local` cuando el host no trae subdominio (p. ej. `localhost`). Este modo es **reversible**.

### 1.1 Ejecutar normalmente (dev activo)

1. Verificar que estas claves existan en `Web.config` (`configuration/appSettings`):
   - `<add key="JKPortalDominioRaiz" value="jkturnos.com"/>`
   - `<add key="JKPortalHostDevSubdominio" value="local"/>`
2. Conectar a SQL Server Express `.\SQLEXPRESS`, base `JKTurnosDB`.
3. La BD debe tener un tenant con `Subdominio = 'local'` y `PortalHabilitado = true`.
   El seed (`Migrations/Configuration.cs`) crea ese tenant (Id=3).
4. F5 en Visual Studio (IIS Express, `https://localhost:44332/`).
5. Entrar a `https://localhost:44332/` → se resuelve el portal del tenant `local`.
   (El fallback de `ResolveTenantByHost` usa el appSetting `JKPortalHostDevSubdominio`.)

### 1.2 Revertir a producción a nivel de código (sin fallback dev)

Para que el portal vuelva a resolverse **solo por subdominio** (comportamiento real
de producción) y NO por `localhost`:

| Acción | Antes | Después |
|---|---|---|
| `Web.config` → `JKPortalHostDevSubdominio` | `local` | **vacío / eliminar** |
| `Web.config` → `JKPortalDominioRaiz` | `jkturnos.com` | `jkturnos.com` (se conserva) |

Con `JKPortalHostDevSubdominio` vacío, `ResolveTenantByHost` devuelve `null` cuando el
host no trae subdominio (líneas 39-48 de `PortalReservaService.cs`): un visitante de
`localhost` verá el flujo "recurso no disponible" (o 404 neutro), nunca un portal por defecto.

> Nota: esto NO elimina el portal del proyecto; solo desactiva el atajo de desarrollo.
> Para probar el portal real por subdominio en local habría que configurar boning con
> el host `demo.jkturnos.com` (ver limitación del certificado en la sección 3).

### 1.3 Reconstruir la BD desde cero (si hiciera falta)

1. `Update-Database -ConfigurationTypeName JKTurnos.Migrations.Configuration -Verbose`
2. Verificar que el tenant `local` tiene `PortalHabilitado = true`.

---

## 2. Consideraciones para producción del portal

### 2.1 Dominio y subdominios

- El portal se sirve en un **subdominio por negocio** bajo `JKPortalDominioRaiz`
  (`jkturnos.com`). Ejemplos: `demo.jkturnos.com`, `gon.jkturnos.com`.
- `ExtractSubdominio(host)` deduce el tenant a partir del subdominio del host. En
  producción el host SIEMPRE debe traer subdominio (`<tenant>.jkturnos.com`).
- El dominio raíz configurado debe coincidir con el real, o el extractor no podrá
  distinguir `demo.jkturnos.com` de `jkturnos.com`.

### 2.2 DNS y certificados

- **DNS**: crear un registro wildcard `*.jkturnos.com` (A) apuntando al servidor de
  producción, para que cada tenant imprima su propio subdominio sin configurar uno por uno.
- **TLS/HTTPS**: puerto 443. El certificado SSL debe cubrir `*.jkturnos.com` (wildcard).
  Sin ello, el navegador mostrará advertencia de certificado y el flujo WhatsApp se verá mal.
- IIS Express local devuelve 400 "Invalid Hostname" con hosts custom por SNI/certificado;
  en IIS de producción con wildcard esto se resuelve. El código no tiene ese problema.

### 2.3 Aplicación web (IIS) / hosting

- Publicar la app en un sitio IIS con binding `*.jkturnos.com:443`.
- Cambiar la cadena de conexión en `Web.config` de producción (sección
  `<connectionStrings>`) a la instancia de SQL Server productiva.
- `JKPortalHostDevSubdominio` debe estar **vacío** en producción (ver 1.2).
- `JKPortalDominioRaiz` = `jkturnos.com` (o el dominio real adquirido).

### 2.4 Base de datos

- Cada tenant con el portal activo debe tener:
  - `Tenants.Subdominio` = su subdominio.
  - `ConfiguracionesTenant.PortalHabilitado = true` (el admin lo activa desde
    `Configuracion/DatosNegocio` con el checkbox "Portal publico").
  - Recursos (`Recursos`) y configuración de WhatsApp del negocio correctos (el link
    de confirmación sale del número configurado del tenant).

### 2.5 Seguridad / datos

- El flujo del portal es **anónimo** (`[AllowAnonymous]`): no autentica clientes.
  Se registra auditoría pública vía `AuditService.RegistrarPublico(tenantId, ...)`.
  No dependas del claim de usuario en acciones del portal.
- Validar `AntiForgeryToken` se mantiene en todos los POST del portal.
- El link WhatsApp usa el número configurado del tenant (overloads de
  `NotificacionService` que reciben `ConfiguracionTenant`) — no filtra por claim.

### 2.6 Checklist de liberación

- [ ] `JKPortalHostDevSubdominio` vacío en el Web.config de producción.
- [ ] `JKPortalDominioRaiz` correcto.
- [ ] Cadena de conexión apuntando a la BD productiva.
- [ ] DNS wildcard + certificado wildcard instalado.
- [ ] Tenants dados de alta con subdominio + `PortalHabilitado=true`.
- [ ] Recursos y WhatsApp de cada tenant configurados.
- [ ] Prueba end-to-end en staging con un subdominio real antes de pasar clientes.

---

## 3. Flujo del usuario para pasar la dirección del portal a sus clientes

### 3.1 Cómo la obtiene el negocio

- Al activar el portal (`PortalHabilitado=true`), el sistema asigna/expone la URL:
  `https://<subdominio>.jkturnos.com/`
  donde `<subdominio>` es el valor de `Tenants.Subdominio`.
- Ej.: el negocio con `Subdominio = demo` comparte `https://demo.jkturnos.com/`.

### 3.2 Qué compartir con los clientes

El negocio difunde la URL del portal. Los clientes, desde el celular:

1. Abren `https://<subdominio>.jkturnos.com/` (web, sin instalar nada).
2. Ven la portada del negocio (logo + nombre).
3. **Elegí un servicio** → presionan la "Continuar".
4. **Elegí fecha y hora** → eligen día, pul san "Ver horarios" y tocan la hora libre.
5. **Completá tus datos** → nombre, teléfono y (opcional) email y observaciones.
6. **Confirmar reserva** → se abre WhatsApp con el mensaje prearmado hacia el teléfono
   del negocio y el cliente envía "Confirmar por WhatsApp".

### 3.3 Cómo difundirla (sugerencias para el admin)

- **QR**: generar un código QR de la URL y mostrarlo en el local / flyers para que el
  cliente escanee con la cámara del celular.
- **Link en la biografía de Instagram/redes**: pegar la URL en el link de la bio.
- **Firma / mensajes**: incluir la URL en facturas, recibos o mensajes de confirmación.
- **Boton WhatsApp / menú de negocios** en línea del negocio.

### 3.4 Roles y recordatorio

- El portal es **solo lectura para el cliente**: no requiere cuenta.
- Todo el flujo termina en **WhatsApp**: es el cliente quien envía el mensaje final
  de confirmación al número del negocio (un solo toque). No hay registro en el sistema
  hasta que el negocio confirme por WhatsApp como en el flujo interno actual.

---

## Depuración rápida (FAQ local)

| Síntoma | Causa probable | Solución |
|---|---|---|
| `localhost` no muestra el portal | `JKPortalHostDevSubdominio` vacío | Ponerlo en `local` (solo dev) |
| Botón WhatsApp y botones sin estilo | Falta `jk-components.css` en `_PortalLayout` | Ya agregado: recompilar |
| "No hay horarios libres" siempre al entrar | Primera visita sin fecha consultada | Flag `SlotsConsultados` ya controla el mensaje |
| 400 Invalid Hostname con `demo.jkturnos.com` | Certificado/SNI de IIS Express | Usar Solución A en dev; en prod wildcard |