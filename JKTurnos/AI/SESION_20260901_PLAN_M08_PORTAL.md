# SESION 2026-09-01 — PLAN M-08 PORTAL DE RESERVAS ONLINE + WHATSAPP

OBJETIVO: Portal publico de reservas por subdominio. El cliente (SIN sesion) abre el
subdominio del negocio, elige recurso/fecha/hora, carga sus datos, se crea el turno
(Pendiente) y recibe un link `wa.me` precargado para confirmar su reserva por WhatsApp.
Cierra el loop de adquisicion de clientes para el SaaS.

ESTADO: EN PROGRESO. Pendiente confirmacion TEAM LEAD (CONSOLIDADO solo via 01_ESTADO.md).

## Decisiones tomadas (TEAM LEAD, 2026-09-01)

1. RESOLUCION DEL TENANT PUBLICO: exclusivamente desde el header `Host`
   (fallback `X-Forwarded-Host` porque IIS reescribe el host). Se toma el primer label
   como subdominio y se busca en `Tenant.Subdominio`. El dominio raiz se valida contra
   `Web.config` (appSetting `JKPortalDominioRaiz`; el primer label igual al dominio raiz
   => el subdominio es NULO). El TenantId resuelto es SOLO contexto del portal publico:
   JAMAS es un mecanismo de autorizacion. NO se tocan autenticacion ni logica privada.

2. CASOS DE BORDE (documentados ANTES de implementar):
   - Tenant NO EXISTE (subdominio sin Tenant activo): el portal responde 404 con pagina
     publica estatica "Este negocio no existe" (sin Login, sin leak de datos).
   - Tenant INACTIVO (IsActive=false => baja logica) o SuspendidoManual=true: 404 con
     "Este negocio no esta disponible en este momento".
   - Tenant existe e IsActive pero PORTAL NO HABILITADO (nuevo flag `PortalHabilitado`
     en ConfiguracionTenant): 404 con "Este negocio no tiene el portal de reservas
     habilitado aún". (Los existentes arrancan con PortalHabilitado = false.)
   - Todo 404 publico muestra una vista neutra sin layout privado ni branding sensible.
   Solo un tenant VALIDO y con portal habilitado renderiza el flujo de reserva.

3. FLUJO (minimo): recurso -> fecha/hora (solo slots libres) -> datos cliente (nombre,
   telefono) -> crear turno Pendiente -> pantalla de confirmacion con link `wa.me`.
   (El servicio quedo FUERA del flujo minimo por decision; al crear el turno se usa
   ServicioId = null => duracion por defecto 30 min, segun TurnoService.CalcularFin.)

4. LINK WHATSAPP: apunta al numero del CLIENTE con mensaje de confirmacion precargado
   (reusando plantilla/config del negocio via NotificacionService.GenerarLinkWhatsApp).
   El cliente abre WhatsApp y envia para confirmar. Sin envio programatico.

## Arquitectura (flujo Controller -> Service -> Repository, regla del proyecto)

- Nuevo `PublicTenantRepository` (Data/): consulta SOLO por Subdominio/TenantId con
  `new JKContext()` y filtros por IsActive/SuspendidoManual, SIN depender de claim.
  Usado exclusivamente por servicios publicos.
- Nuevo `PortalReservaService` (Services/Portales/): reglas de negocio publicas.
  - `ResolveTenant(host)`: parsea Host/X-Forwarded-Host, valida dominio raiz, busca
    Tenant por Subdominio y aplica los casos de borde (ver 2). Devuelve el Tenant o null.
  - `GetRecursos(tenantId)`: recursos activos del tenant.
  - `GetSlots(tenantId, recursoId, fecha)`: intervalos libres de 30 min en horario
    laboral (hora +/- rango), reusando la logica de solapamiento de TurnoService
    (Cancelado libera slot) para marcar libres/ocupados.
  - `Reservar(tenantId, recursoId, fecha, hora, nombre, telefono)`: crea (o reusa) el
    Cliente del tenant, crea el Turno Pendiente (ServicioId=null, 30 min) validando
    disponibilidad, y genera el link `wa.me` precargado. Audita.
  - Usa `new JKContext()` (patron ya presente en RecursoServicioService.Vincular) para
    evitar el claim del TenantProvider.

## Cambios de modelo (migracion)

- `ConfiguracionTenant` + `public bool PortalHabilitado { get; set; }` (default false).
- Nueva migracion `AddPortalPublico`: AddColumn dbo.ConfiguracionTenants PortalHabilitado.

## Archivos nuevos / editados

- NUEVO Data/PublicTenantRepository.cs
- NUEVO Services/Portales/PortalReservaService.cs
- NUEVO Controllers/PortalController.cs  (hereda de Controller, NO BaseController; [AllowAnonymous])
- NUEVO Models/ViewModels/PortalViewModel.cs (+ submodelos SlotVM)
- NUEVO Views/Portal/{Inicio,FechaHora,Datos,Confirmacion,NoDisponible}.cshtml
- EDIT Views/Configuracion/DatosNegocio.cshtml: checkbox "Habilitar portal de reservas"
- EDIT Controllers/ConfiguracionController.cs: actualiza PortalHabilitado en DatosNegocio
- EDIT Global.asax.cs / RouteConfig.cs: rutas del portal (fuera de la ruta default protegida)
- EDIT Web.config: appSetting `JKPortalDominioRaiz`
- EDIT Migrations/*: +AddPortalPublico
- Layout publico propio (Views/Portal/_LayoutPublico.cshtml) o vista sin layout del app.

## Nullabilidad y seguridad

- Todos los endpoints publicos: no aceptan auth, no exponen datos de otros tenants.
- Anti-forgery en los POST de reserva.
- El telefono se normaliza (mismo filtro que NotificacionService).

## Verificacion

- MSBuild Debug 0 errores (no usar `&&`; `; if ($?) {...}`).
- node check no aplica (sin cambios JS de agenda) — salvo que el portal use JS propio.
- Retest manual del TEAM LEAD en el navegador por subdominio.