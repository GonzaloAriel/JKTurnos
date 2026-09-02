# AUDITORIA - VIDRIERA PUBLICA DEL TENANT (MODULO M-13) - POSTA 1

FORMATO: 1.0
Estado: INFORME READ-ONLY (no se modifico codigo).
Fecha: 2026-09-02
Root: C:\Users\Gonzo\Desktop\JKTurnos\JKTurnos\JKTurnos\JKTurnos

Objetivo: responder que existe y que falta para construir la Vidriera sin crear
estructuras innecesarias.

## 1. ENTIDADES EXISTENTES REUTILIZABLES

### Tenant
Archivo: Models\Entities\Tenant.cs
- Extiende BaseEntity (Id, CreatedAt, UpdatedAt, IsActive).
- NO es ITenantEntity (es de plataforma, sin TenantId propio).
- Campos: Nombre, Subdominio (lowercase), PlanCodigo, FechaInicioPeriodo,
  FechaVencimiento, SuspendidoManual.
- Hallazgo: el Subdominio solo se usa para validacion de unicidad y busqueda.
  NO existe resolucion de tenant por host/slug. Hay que construirla.

### Servicio
Archivo: Models\Entities\Servicio.cs - ITenantEntity
- Nombre, Descripcion, Precio (decimal), DuracionMinutos (int).
- Campos exactos para la vidriera (servicios + precios + duracion).

### Recurso
Archivo: Models\Entities\Recurso.cs - ITenantEntity
- Nombre, Descripcion, Tipo (TipoRecurso enum), Color (#hex).
- TipoRecurso enum: Profesional, Sala, Equipo, Cancha, Consultorio, Cabina,
  Vehiculo, Box.

### Turno
Archivo: Models\Entities\Turno.cs - ITenantEntity
- ClienteId, ServicioId (nullable), RecursoId, EstadoTurnoId,
  FechaHoraInicio, FechaHoraFin, Observaciones.
- Navigation a Cliente/Servicio/EstadoTurno/Recurso.

### EstadoTurno / EstadosTurnoBase
Archivo: Models\Entities\EstadoTurno.cs + EstadosTurnoBase.cs
- Estados por-tenant con Codigo estable (PENDIENTE/EN_CURSO/FINALIZADO/CANCELADO).
- D-042: el turno Cancelado libera su slot. Semantica por Codigo, nunca por Nombre.

### RecursoServicio (many-to-many)
Archivo: Models\Entities\RecursoServicio.cs - ITenantEntity
- RecursoId, ServicioId, TenantId. Mapea que recursos prestan cada servicio.
- Servicio: RecursoServicioService.GetRecursoIdsPorServicio(servicioId).

### ConfiguracionTenant
Archivo: Models\Entities\ConfiguracionTenant.cs - ITenantEntity
- Identidad del negocio: NombreComercial, Telefono, Email, Direccion, LogoUrl,
  AvatarUrl. (Mas plantillas SMTP/WhatsApp que NO deben exponerse.)
- Una fila por tenant (ConfiguracionService.GetConfiguracion()).

## 2. LAYOUTS / VISTAS EXISTENTES

- Views\Shared\_Layout.cshtml : shell autenticado (sidebar + topbar). NO usar para la vidriera.
- Views\Shared\_LoginLayout.cshtml : layout standalone sin auth (DOCUMENTO HTML completo,
  CSS DS v2, jQuery+Bootstrap, secciones PageStyles/scripts). ES EL PATRON A REUTILIZAR
  para crear un _PublicLayout.cshtml (o similar).
- Views\_ViewStart.cshtml : setea _Layout.cshtml para TODAS las vistas. La vista publica
  debe override con su propio Layout en el top (igual que hace Login/Account).
- No existe Views\Vidriera\ ni Views\Public\.
- No existe NINGUN codigo de "vidriera"/"landing"/"ofertas" previo.

## 3. CALCULO DE DISPONIBILIDAD (GAP CRITICO)

TurnoService.cs:
- ValidarDisponibilidad(recursoId, inicio, fin, turnoExcluirId) : bool
  -> verifica si un rango especifico esta libre (excluye Cancelado, D-042).
- CalcularFin(inicio, servicioId) : calcula fin segun DuracionMinutos (default 30).
- GetTurnosDelDia(fecha), GetTurnosEnRango(desde, hasta) : consultas de agenda.
- NO EXISTE algoritmo de generacion de slots disponibles. Hay que crear
  VidrieraService.GetAvailableSlots(servicioId, fecha) (o similar) que:
  jornada -> paso (duracion servicio) -> recursos del servicio -> turnos ocupados
  -> listar slots libres. REUTILIZANDO ValidarDisponibilidad / los datos de Turno.

DashboardService tiene MinutosJornada = 690 (08:00-19:30 hardcodeado, D-012).
NOTA: la vidriera de v1 dependera de esta jornada fija hasta que M-01 (horarios
configurables) este consolidado. Documentar como limitacion.

## 4. RESOLUCION DE TENANT (GAP CRITICO)

- TenantProvider.GetTenantId() : lee claim "TenantId" del ClaimsIdentity autenticado.
  Lanza si no hay usuario. SOLO sirve para contexto autenticado.
- BaseRepository<TEntity>.GetAll() filtra SIEMPRE por TenantId del claim.
  NO usable para consulta publica anonima.
- NO existe resolucion por host/url/slug. Para la vidriera hay que crear un
  mecanismo anonimo que resuelva un Tenant por su slug (PublicRepository o
  consulta directa a JKContext filtrada por IsActive y slug).

## 5. PATRON CONTROLLER / SERVICIO

- MvcApplication en Global.asax.cs registra areas + routes + bundles + migraciones.
- RouteConfig.cs : SOLO ruta default {controller}/{action}/{id}. NO hay WebApi.
- Todos los controllers heredan de BaseController ([Authorize], TenantId por claims).
- Servicios se instancian en el constructor (new, sin DI container).
- JSON: Json(new { success=..., ... }) + [ValidateAntiForgeryToken] en POST.
- Para la vidriera: crear VidrieraController SIN heredar de BaseController y con
  atributo [AllowAnonymous] (o solo publico, read-only).

## 6. RUTAS

RouteConfig.cs tiene una unica ruta default. Para la vidriera se agrega una ruta
nombrada antes de la default, por ejemplo:
  routes.MapRoute("Vidriera", "vidriera/{slug}", new { controller="Vidriera",
  action="Index" });
(sin params opcionales que choquen con la default).

## 7. API / EXPOSICION

- NO hay ApiController ni WebApi. Todo es MVC.
- Version 1 de la vidriera: vista MVC + meta-refresh / fetch ligero de un partial
  o endpoint read-only (a decidir en Posta 4/6).

## 8. CONCLUSION PARA EL DISENO (Posta 2)

REUTILIZAR (sin tocar):
- Servicio, Recurso, Turno, EstadoTurno, RecursoServicio, ConfiguracionTenant.
- GlobalRepository (patron de consulta sin filtro de claims) como modelo para
  la consulta publica.
- _LoginLayout como base del layout publico.

CREAR (nuevo, minimo):
- Modelo VidrieraPublica (o campos en config existente) - decidir en Posta 2.
- Slug para resolver el tenant publicamente (nuevo campo unico).
- VidrieraController (publico, read-only) + VidrieraService (lectura + slots).
- Metodo GetAvailableSlots (motor de disponibilidad publica).
- Layout publico + vistas publicas.
- Seccion de gestion en el panel.

LIMITACION DE V1:
- Jornada fija 08:00-19:30 (D-012) hasta M-01. Documentar en la vista/publicidad.
