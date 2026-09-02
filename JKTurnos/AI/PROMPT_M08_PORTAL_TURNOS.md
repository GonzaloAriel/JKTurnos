# M-08 PORTAL DE TURNOS ONLINE + WHATSAPP — IMPLEMENTACION Y DIAGNOSTICO

Fecha: 2026-09-01.
Tipo: documento de implementacion + diagnostico tecnico para la siguiente posta.
Estado: IMPLEMENTADO (backend + vistas + build OK), BLOQUEADO en la vía de prueba local
por un problema de certificado/SNI en IIS Express (no es bug de codigo del portal).

---

## 1. QUE ES M-08

Portal PUBLICO de reservas por SUBDOMINIO, SIN sesion. El cliente abre
`https://<subdominio>.jkturnos.com`, elige recurso -> fecha/hora libre -> carga sus
datos -> se crea el TURNO (estado PENDIENTE) -> recibe un link `wa.me` precargado
para confirmar la reserva por WhatsApp. Cierra el loop de adquisicion de clientes
para el SaaS multi-tenant JKTurnos.

Ejemplo de flujo:
1. Cliente visita `https://demo.jkturnos.com`
2. Resuelve tenant `Id=1` (Subdominio `demo`)
3. `GET /Portal/Inicio` -> lista de servicios (recursos) del tenant
4. POST `ElegirRecurso` -> `FechaHora` (input date + slots libres de 30 min)
5. POST `Slots` -> lista slots libres de la fecha
6. POST `ElegirHora` -> `Datos` (nombre, telefono, email, observaciones)
7. POST `Confirmar` -> crea Turno PENDIENTE + link WhatsApp -> `Confirmacion`
8. Cliente toca boton -> abre `wa.me/<telefono-negocio>?text=<mensaje-precargado>`

---

## 2. DECISIONES ESTRUCTURALES (TEAM LEAD, 2026-09-01)

1. **Resolucion de tenant publico SOLO por header `Host`** (fallback `X-Forwarded-Host`
   porque IIS/proxy reescribe el host). El primer label del host es el SUBDOMINIO y se
   busca en `Tenant.Subdominio`. El dominio raiz se valida contra Web.config
   (`JKPortalDominioRaiz`). El tenantId resuelto es SOLO contexto de navegacion,
   JAMAS un mecanismo de autorizacion.
2. **Casos neutros (404)**: tenant no existe -> "Este negocio no existe"; inactivo/
   suspendido -> "no disponible"; `PortalHabilitado=false` -> "portal no habilitado".
   Vista neutra, SIN layout privado.
3. **Flujo minimo**: ServicioId = null, duracion por defecto 30 min. Recurso/bloque
   elegido por el cliente. Estado inicial PENDIENTE.
4. **Link WhatsApp** al NUMERO DEL CLIENTE (reusa `NotificacionService.GenerarLinkWhatsApp`)
   con mensaje precargado explicando la reserva y pidiendo confirmacion.

---

## 3. CAPAS Y ARCHIVOS IMPLEMENTADOS

### 3.1 Modelo de datos (migracion)
- `ConfiguracionTenant` + `public bool PortalHabilitado { get; set; }` (default false).
- Migracion codificada: `Migrations/202609011421381_AddPortalPublico.cs`
  (`AddColumn dbo.ConfiguracionTenants PortalHabilitado`) + `.Designer.cs` + `.resx`.
- Seed (`Migrations/Configuration.cs`): tenant demo `Subdominio="local"` con
  `PortalHabilitado = true` listo para testear.

### 3.2 Servicio publico (reglas de negocio)
`Services/PortalReservaService.cs`:
- `ResolveTenantByHost(string host)`: `ExtractSubdominio(host)` -> busca
  `Tenant.Subdominio == subdominio` (JKContext nuevo por llamada, sin claim).
- `PortalDisponible(Tenant)`: existe && IsActive && !SuspendidoManual && config.PortalHabilitado.
- `GetConfiguracion(int tenantId)` / `GetRecursos(int tenantId)`.
- `GetSlotsLibres(tenantId, recursoId, fecha, ...)`: slots de 30 min en franja
  laboral, descarta pasados y ocupados (misma semantica que TurnoService; cancelados
  liberan su slot, D-042).
- `ObtenerOCrearCliente(tenantId, nombre, telefono, email)`: dedupe por telefono
  normalizado a digitos.
- `CrearReserva(...)`: valida recurso del tenant, disponibilidad, calcula fin
  (30 min/duracion servicio), asigna estado PENDIENTE, audita.
- `GenerarLinkConfirmacion(turnoId)` / `NormalizarTelefono` / `ExtractSubdominio`.

### 3.3 Controlador publico (sin sesion)
`Controllers/PortalController.cs`:
- `[AllowAnonymous]`, hereda de `Controller` (NO de BaseController, que depende del
  claim). Ctor `new PortalReservaService()`.
- `RequestHost`: `X-Forwarded-Host` o `Request.Url.Host`.
- `ResolveTenant()` -> `TenantContext {Ok, IsDisponible, TenantId, Subdominio,
  NegocioNombre, NegocioLogo, NegocioTelefono, NegocioDireccion}`; `NoExiste`/
  `NoHabilitado` derivados.
- Acciones: `Inicio` (GET), `ElegirRecurso`, `Slots`, `ElegirHora`, `Confirmar`
  (POST, `[ValidateAntiForgeryToken]`), `BuildInicioModel`, `ParseFecha`/`ParseHora`/
  `TryParseExact`.
- If `!Ok || !IsDisponible` -> `View("NoDisponible", ctx)`.

### 3.4 ViewModel
`Models/ViewModels/PortalViewModel.cs`:
- Tenant/branding: `TenantId`, `NegocioNombre/Logo/Telefono/Direccion`, `Subdominio`.
- Paso 1: `Recursos` (List<Recurso>), `RecursoId`.
- Paso 2: `Fecha` (string "yyyy-MM-dd"), `FechaLegible`, `SlotsLibres`, `HoraSeleccionada`.
- Paso 3: `Nombre`, `Telefono`, `Email`, `Observaciones`.
- Paso 4: `ReservaCreada`, `TurnoId`, `WhatsAppLink`, `FechaHoraLegible`, `RecursoNombre`.
- NOTA: `Fecha` es `string` (input type=date) y `FechaHoraLegible`/`FechaLegible`
  son strings pre-formateados por el controller (evita `.ToString(fmt)` sobre
  `DateTime?` en Razor, que rompe la compilacion de vistas con `MvcBuildViews=true`).

### 3.5 Vistas y layout publico
`Views/Portal/`:
- `_PortalLayout.cshtml`: layout publico (fonts/icons, design tokens, header con
  logo/negocio `ViewBag.PortalNegocio/PortalLogo`, footer).
- `Inicio.cshtml`, `FechaHora.cshtml`, `Datos.cshtml`, `Confirmacion.cshtml`,
  `NoDisponible.cshtml` (model = `PortalController.TenantContext`).
- `Content/Pages/jk-portal.css`: estilos del portal.
- Razor del proyecto: variable de modelo = `Model` (MAYUSCULA). Los bloques de
  control (`if`/`foreach`/`using`) dentro de `@using(...){ }` van SIN prefijo `@`
  (exit `@` para expresiones de marcado). Las vistas usan `@Model.X` y
  `@using(Html.BeginForm(...)) { if(...) foreach(...) }`.

### 3.6 Acceso raiz por subdominio
`Global.asax.cs` `Application_BeginRequest`: si el host resuelve a un tenant valido
con portal habilitado, reescribe el path raiz `~/` a `/Portal/Inicio`. NO toca auth.

### 3.7 Configuracion de Admin
- Checkbox `PortalHabilitado` en `Views/Configuracion/DatosNegocio.cshtml` (postea
  via `_FormLayout` -> `ConfiguracionTenant` -> POST `DatosNegocio`).
- Web.config: `<add key="JKPortalDominioRaiz" value="jkturnos.com"/>`.

### 3.8 Registros en csproj
`JKTurnos.csproj`: se registraron `PortalController.cs`, `PortalViewModel.cs`,
`PortalReservaService.cs`, migracion `AddPortalPublico` (+.Designer.cs +.resx).

### 3.9 Auditoria publica
`AuditService` + overload `RegistrarPublico(int tenantId, string accion, ...)` porque
el portal corre sin claim (no puede usar el TenantId del usuario autenticado).

---

## 4. COMO QUEDA EL FLUJO (controller <-> service <-> repo <-> EF)
- Controller decide que renderiza (`Inicio`/`FechaHora`/`Datos`/`Confirmacion`/
  `NoDisponible`) y valida ModelState.
- Service tiene todas las reglas de negocio (resolucion tenant, portalDisponible,
  slots, crear reserva, link WA).
- Repos/EF se acceden SOLO desde el service, con `JKContext` nuevo por llamada y
  scope = `TenantId` recien resuelto del host. Nunca se usa el claim.
- Controllers NO tocan EF directo.

---

## 5. ESTADO DE COMPILACION
- `MSBuild Debug` => 0 errores.
- `MSBuild /p:MvcBuildViews=true` => 0 errores (compila TODAS las vistas Razor,
  incluso las del portal). Verificado 2026-09-01.

---

## 6. EL PROBLEMA ACTUAL (bloqueo en PRUEBA LOCAL, NO es bug de codigo)

### Sintoma
- `https://localhost:44332/` responde 302 (la app corre normal).
- `https://demo.jkturnos.com:44332/` -> el navegador muestra `ERR_CONNECTION_RESET`;
  por curl devuelve `HTTP Error 400. The request hostname is invalid.`

### Causa raiz
El proyecto corre en IIS Express con bindings HTTPS `*:44332:localhost` (+ agregado
`*:44332:demo.jkturnos.com` en `.vs/JKTurnos.slnx/config/applicationhost.config`).
IIS Express solo matchea el hostname del binding si el CERTIFICADO de desarrollo
cubre ese nombre (SNI). El cert de desarrollo por defecto cubre SOLO `localhost`.
Para `demo.jkturnos.com` (mapeado a 127.0.0.1 en `hosts`), HTTP.sys/IIS Express
rechaza el hostname con 400 "Invalid Hostname" y el navegador termina en
`ERR_CONNECTION_RESET` (corta antes de llegar a ASP.NET).

El proceso IIS Express (PID 12776) se relanzo DESPUES de editar el binding
(config 12:51 -> proceso 12:53), o sea que el binding SI esta activo y aun asi
rechaza el hostname => confirma que el bloqueo es de CERTIFICADO/SNI, no de binding
ni de codigo.

### Opciones para desbloquear
A) Usar `localhost` para la prueba, ajustando la RESOLUCION de tenant de desarrollo
   para que el host real `localhost` resuelva al tenant seed `local`
   (cambio acotado y reversible en resolucion de dev; NO toca bindings ni certs).
   [IMPLEMENTADA 2026-09-01 — ver "Solucion A aplicada" abajo.]
B) Crear/registrar un certificado de desarrollo que cubra `demo.jkturnos.com`
   (SNI) en IIS Express para el bind 44332. Mas fiel al flujo real por subdominio,
   pero toca certificados y config IIS (mas fragile).
C) Probar por HTTP en el binding `*:52129:demo.jkturnos.com` (sin TLS), evitando el
   problema de certificado/SNI del HTTPS.

### SOLUCION A APLICADA (2026-09-01)
Fallback de desarrollo, acotado y reversible, dentro de
`PortalReservaService.ResolveTenantByHost`: si el host NO trae subdominio (p.ej.
`localhost`) y existe el appSetting `JKPortalHostDevSubdominio`, se usa ese valor
como subdominio fallback. Con `value="local"` en Web.config, `https://localhost:44332/`
resuelve al tenant seed `Subdominio='local'` (Id=3, PortalHabilitado=true).
- NO toca bindings ni certificados.
- Produccion intacta: un host con subdominio no se ve afectado; y si se vacia
  `JKPortalHostDevSubdominio`, el fallback se desactiva (host sin subdominio no
  resuelve tenant, como antes).
- Archivos: `Services/PortalReservaService.cs` (`ResolveTenantByHost`) + Web.config
  (`JKPortalHostDevSubdominio`). Aplica a la vez al `PortalController` y al
  `Global.asax` porque ambos usan `ResolveTenantByHost`.
- Build: MSBuild Debug y `/p:MvcBuildViews=true` = 0 errores.

### BUG CLAIM RESUELTO (2026-09-01): "TenantId no encontrado" al confirmar
Al pulsar "Confirmar reserva", el flujo llegaba a `GenerarLinkConfirmacion(turnoId)`
que delegaba en `NotificacionService.GenerarMensaje(turno)` ->
`ConfiguracionService.GetConfiguracion()`. Ese metodo filtra por el CLAIM `TenantId`
del usuario autenticado (`BaseRepository.GetAll()` -> `TenantProvider.GetTenantId()`
en `Infrastructure/TenantProvider.cs:19`). Como el portal M-08 es ANONIMO (sin
sesion/claim), se lanzaba `System.Exception: TenantId no encontrado`.

CORRECCION (patron ya usado por `AuditService.RegistrarPublico`): el portal pasa el
`tenantId` de forma EXPLICITA y carga la config SIN claim:
- `NotificacionService`: nuevo overload `GenerarLinkWhatsApp(Turno, ConfiguracionTenant)`
  y `GenerarMensaje(Turno, ConfiguracionTenant)` que usan la config pasada; los
  metodos de 1 arg originales (autenticados) siguen con su comportamiento (claim).
- `PortalReservaService.GenerarLinkConfirmacion(int tenantId, int turnoId)`: carga la
  config del tenant por `GetConfiguracion(tenantId)` (JKContext, sin claim) y la pasa.
- `PortalController.Confirmar`: `_service.GenerarLinkConfirmacion(ctx.TenantId, turno.Id)`.
- El flujo autenticado del drawer (`TurnosController.cs`) no se toca: usa el overload
  de 1 arg que conserva el claim.
- Build: MSBuild Debug y `/p:MvcBuildViews=true` = 0 errores.

### Aclaraciones de alcance
- El flujo del portal compila y corre; el unico bug en runtime fue el del CLAIM en el
  link de WhatsApp (seccion "BUG CLAIM RESUELTO"), ya corregido. El resto de la
  resolucion de tenant, rutas, vistas, ModelState y anti-forgery funcionaba bien.
- El binding de hostname custom en IIS Express es un vecindario fragile de
  certs/SNI; por eso el camino A (localhost + host real = subdominio local) es el
  de menor riesgo para la prueba local.
- NO se modifico BD: `PortalHabilitado=true` garantizado por seed SOLO para el
  tenant `local` (Id=3). Para el tenant `demo` (Id=1) depende del estado de la BD;
  revisar antes de probar si se elige dejar `demo` como subdominio de entrada.
- NO se tocaron certificados (sin autorizacion del TEAM LEAD).

---

## 7. PROXIMA POSTA (sugerida)
1. [HECHO] Solucion A implementada (fallback dev por localhost, seccion 6).
2. Probar el flujo por `https://localhost:44332/` (ver "Pasos para probar").
3. Verificar slot de 30 min contra franja laboral y descarte de ocupados.
4. Verificar ModelState (nombre/telefono vacios devuelven al paso Datos con datos
   conservados) y anti-forgery.
5. Verificar estados neutros 404 (tenant inexistente / no habilitado).
6. Crear el turno y confirmar que el link `wa.me` abre con mensaje precargado.
7. Documentar y pedir retest del TEAM LEAD.

## 8. PASOS PARA PROBAR (Solucion A — via localhost)
1. En Visual Studio, DETENER la app (para que IIS Express recargue el codigo/Web.config
   nuevos) y VOLVER A INICIAR (F5 / Ctrl+F5).
2. Abrir en el navegador: `https://localhost:44332/`
   (aceptar el aviso de certificado de desarrollo si aparece).
   - El rewritepath de raiz + el fallback dev resuelven al tenant `local` (PortalHabilitado=true)
     y deben renderizar el PORTAL publico (Paso 1: listado de recursos).
3. Flujo: elegir recurso -> Continuar.
4. Fecha y hora: elegir dia (input type=date) -> "Ver horarios" -> elegir hora libre.
5. Datos: completar nombre + telefono (email/observaciones opcionales) -> "Confirmar reserva".
6. Confirmacion: "Reserva creada" + boton "Confirmar por WhatsApp" (link wa.me con mensaje
   precargado). Verificar que el TURNO nacio en estado PENDIENTE en esa fecha/hora/recurso.
7. Validar negativos:
   - Nombre o telefono vacio -> vuelve a Datos con mensaje y datos conservados.
   - Probar un host sin fallback dev (vaciando JKPortalHostDevSubdominio) o un subdominio
     inexistente -> vista neutra "Este negocio no existe".