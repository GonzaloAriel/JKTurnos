# DISEÑO - VIDRIERA PUBLICA (M-13) - POSTA 3: PRIMERA VISTA FUNCIONAL

FORMATO: 1.0
Estado: Posta 3 CERRADA (3A+3B, 2026-09-02). Posta 4 (disponibilidad real) IMPLEMENTADA y VERIFICADA (2026-09-02) - diseno en seccion 13. Posta 5 (panel de gestion) IMPLEMENTADA y VERIFICADA (2026-09-02) - diseno y cierre en secciones 14-15. Extensión de la Vista Publica (mensaje/oferta/servicios con flags + logoo avatar) y REDISENO MONITOR (2026-09-02) en seccion 16. Pendientes: Postas 6-8.
Fecha: 2026-09-02
Base: D-042 (modelo aprobado) + CONTRATO (Posta 0) + AUDITORIA (Posta 1) + DISENO 2A.

Objetivo: primera version navegable de /vidriera/{slug} que resuelve tenant por
slug, valida habilitacion+tenant activo, muestra identidad + servicios reales,
es read-only y con layout publico independiente. SIN disponibilidad calculada ni
editor.

## 1. ALCANCE DE LA POSTA (3A + 3B)

INCLUYE:
- Resolucion por slug (unica entrada publica).
- Validaciones: existe / habilitada / tenant activo.
- ViewModel publico + layout publico + vista minima.
- Datos reales: identidad (ConfiguracionTenant) + servicios activos + precios
  condicionados por MostrarPrecios.
- Read-only total y aislamiento por TenantId (criterio explicito).

EXCLUYE (fuera de alcance, segun contrato):
- Calculo de slots / disponibilidad en tiempo real (Posta 4).
- Ofertas y mensajes editables o mostrados (se quedan en el modelo, no se renderizan en 3).
- Panel de administracion / publicacion-despublicacion desde UI (Posta 5).
- Reservas, login de clientes, WhatsApp, subdominios, SignalR, CMS.
- Migraciones adicionales NO necesarias (la entidad VidrieraPublica ya se crea en su propia migracion; la vista y el service no agregan schema).
- Rediseno general del sistema.

## 2. RUTA

RouteConfig.cs, ANTES de la ruta default (para que no la capture {controller}):

```
routes.MapRoute(
    name: "Vidriera",
    url: "vidriera/{slug}",
    defaults: new { controller = "Vidriera", action = "Index", slug = UrlParameter.Optional }
);
```

- El segmento es "vidriera" literal + {slug} como parametro (NO un TenantId).
- La ruta default `{controller}/{action}/{id}` NO se toca.
- /vidriera/{slug} NO acepta tenantId, id ni querystring de tenant.

## 3. CONTROLLER: VidrieraController

Archivo: Controllers/VidrieraController.cs

- NO hereda de BaseController (el publico anonimo no debe tener claims ni navegacion interna).
- Atributo [AllowAnonymous] y [OutputCache(...)] con NoStore/NoCache adecuadas (Posta 8 detalla cache; en 3 se pone NoStore para no servir datos stale en monitores).
- UNICA action: `Index(string slug)` (GET, read-only).
- Logica de resolución (servicio, seccion 4):
  1. `var vm = _service.GetVidrieraPublica(slug);`
  2. `if (vm == null) return HttpNotFound();`  (mismo patron que el resto de controllers)
  3. `return View(vm);`
- No hay ninguna otra action ni POST. Sin [ValidateAntiForgeryToken] (no hay forms).

- Sobre 404: se usa `HttpNotFound()` (convencion del proyecto, ejemplo ServiciosController.Edit). Alternativa evaluada: vista "NoDisponible.cshtml" dedicada. DECISION: v1 usa `HttpNotFound()` estandar (404) sobre una vista minima de error, sin pagina custom "no disponible". Se puede agregar vista de "no disponible" en Posta 8 (seguridad) si el TEAM LEAD lo pide.

## 4. SERVICE: VidrieraService

Archivo: Services/VidrieraService.cs

Responsabilidad: resolver la vidriera publica y armar el ViewModel. NO hereda
BaseController, NO usa BaseRepository (que depende del claim TenantId).

Resolucion del slug (solo lectura):
- Consulta a JKContext directamente (mismo mecanismo que TenantService para
  acceso controlado; NO se usa GlobalRepository<ConfiguracionTenant> que filtra
  por IsActive pero sirve como base conceptual... en realidad se consulta
  VidrieraPublica que es ITenantEntity, se filtra con where TenantId explicito).
- Metodo principal:
  ```
  public VidrieraPublicaViewModel GetVidrieraPublica(string slug)
  {
      // 1. Slug -> VidrieraPublica (Habilitada=true, IsActive) + Tenant activo
      using (var ctx = new JKContext())
      {
          var vidriera = ctx.VidrieraPublicas
              .FirstOrDefault(v => v.Slug == slug && v.IsActive && v.Habilitada);

          if (vidriera == null) return null;

          var tenant = ctx.Tenants.FirstOrDefault(t => t.Id == vidriera.TenantId && t.IsActive);
          if (tenant == null) return null;

          var tenantId = vidriera.TenantId;   // UNICA fuente de TenantId desde el slug

          // 2. Identidad
          var config = ctx.ConfiguracionesTenant
              .FirstOrDefault(c => c.TenantId == tenantId && c.IsActive);

          // 3. Servicios activos del MISMO tenantId
          var servicios = ctx.Servicios
              .Where(s => s.TenantId == tenantId && s.IsActive)
              .OrderBy(s => s.Nombre)
              .ToList();

          // 4. Armar ViewModel
          var vm = new VidrieraPublicaViewModel
          {
              Slug = vidriera.Slug,
              NombreNegocio = config?.NombreComercial ?? tenant.Nombre,
              LogoUrl = config?.LogoUrl,
              Telefono = config?.Telefono,
              Email = config?.Email,
              Direccion = config?.Direccion,
              MostrarPrecios = vidriera.MostrarPrecios,
              Servicios = servicios.Select(s => new VidrieraServicioVM
              {
                  Nombre = s.Nombre,
                  Descripcion = s.Descripcion,
                  Precio = s.Precio,
                  DuracionMinutos = s.DuracionMinutos
              }).ToList()
          };

          return vm;
      }
  }
  ```

AISLAMIENTO (regla clave):
- El TenantId se obtiene SOLO desde el slug (vidriera.TenantId).
- TODA consulta posterior (config, servicios) filtra por ese MISMO TenantId.
- NUNCA se acepta un TenantId del navegador (query, header, ruta). El controller
  solo recibe "slug". No hay parametro tenantId en ningun lado.

NOTA sobre el uso de JKContext directo: es la unica via para consulta publica
anonima porque BaseRepository leeria el claim. TenantService ya consulta JKContext
directo para casos controlados (GetByIdInactive, GetSuscripciones), asique NO
rompe convencion; queda documentado en la decision.

## 5. VIEWMODELS

### 5.1 Models/ViewModels/VidrieraPublicaViewModel.cs
```
public class VidrieraPublicaViewModel
{
    public string Slug { get; set; }
    // identidad
    public string NombreNegocio { get; set; }
    public string LogoUrl { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    // config de exhibicion
    public bool MostrarPrecios { get; set; }
    // contenido
    public List<VidrieraServicioVM> Servicios { get; set; } = new List<VidrieraServicioVM>();
}
```

### 5.2 Models/ViewModels/VidrieraServicioVM.cs
```
public class VidrieraServicioVM
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int DuracionMinutos { get; set; }
    public bool MostrarPrecio { get; set; }  // conveniencia para la vista
}
```

NOTA: NO exponer Id, TenantId ni campos internos de Servicio/Turno (seccion 5 de
Contrato). Solo datos publicos.

## 6. LAYOUT PUBLICO

Archivo: Views/Shared/_VidrieraLayout.cshtml (o _PublicLayout.cshtml)

- Se CREA un layout publico nuevo (No reutilizar _LoginLayout directamente para
  no arrastrar la estructura del login ni su CSS).
- Tecnicamente inspirado en _LoginLayout (DOCTYPE completo, head con DS v2 +
  jk-vidriera.css, body standalone) PERO sin elementos de login/sistema.
- Estructura minima:
  ```
  <html>
  <head>
    ... meta, title=@ViewBag.Title, fuentes, DS v2 (tokens/base/components/utilities) + jk-vidriera.css
  </head>
  <body>
    <div class="jk-vidriera">   @* contenedor publico de identidad *@
        @RenderBody()
    </div>
    @RenderSection("scripts", required:false)
  </body>
  </html>
  ```
- NO renderiza sidebar, topbar, _Sidebar, _Topbar, ni CSS/JS del panel.
- NO hay nav interna; el body publico solo muestra informacion.
- El title usa @ViewBag.Title (nombre del negocio).

CSS: Content/Pages/jk-vidriera.css (estilo publico, no confundible con el panel).

## 7. VISTA

Archivo: Views/Vidriera/Index.cshtml

- `@model JKTurnos.Models.ViewModels.VidrieraPublicaViewModel`
- `Layout = "~/Views/Shared/_VidrieraLayout.cshtml";`
- `ViewBag.Title = model.NombreNegocio;`

Contenido (solo lo aprobado):
1. Identidad:
   - logo (si LogoUrl) / fallback a inicial o icono.
   - NombreNegocio.
   - telefono/email/direccion si existen.
2. Servicios:
   - listado de Servicios (Nombre + Descripcion + Duracion).
   - Precio SOLO si MostrarPrecios=true.
- SIN secciones de oferta/mensaje/disponibilidad (excluidas en 3).

## 8. CAMBIOS A ARCHIVOS (resumen de diffs)

NUEVOS:
- Controllers/VidrieraController.cs
- Services/VidrieraService.cs
- Models/ViewModels/VidrieraPublicaViewModel.cs
- Models/ViewModels/VidrieraServicioVM.cs
- Views/Shared/_VidrieraLayout.cshtml
- Views/Vidriera/Index.cshtml
- Content/Pages/jk-vidriera.css

MODIFICADOS:
- Models/Entities/VidrieraPublica.cs  (entidad NUEVA, migracion AddVidrieraPublica - corresponde al modelo de Posta 2)
- Data/JKContext.cs (+DbSet<VidrieraPublica>)
- App_Start/RouteConfig.cs (+ruta Vidriera)
- JKTurnos.csproj (+Compile de las nuevas .cs; Content de las nuevas vistas/css)

NOTA migracion: la entidad VidrieraPublica y su tabla (Slug unico, TenantId unico)
se crean en su propa migracion "AddVidrieraPublica" (modelo Posta 2). Posta 3 NO
necesita migraciones adicionales (la vista/service leen lo que ya existe).

## 9. CRITERIOS DE ACEPTACION (Posta 3)

1. Caso feliz: /vidriera/{slug} con tenant activo + vidriera habilitada -> 200,
   muestra identidad + servicios; precio solo si MostrarPrecios.
2. Slug inexistente -> 404.
3. Vidriera.Habilitada=false -> 404.
4. Tenant.IsActive=false -> 404.
5. AISLAMIENTO (criterio formal): /vidriera/slug-A NUNCA muestra servicios/
   precios/config de otro tenant, aun manipulando querystrings o parametros.
6. Read-only: no hay POST, no hay enlaces a modulos internos, no abre login.
7. Funciona sin sesion ([AllowAnonymous]).
8. Build 0 errores, vistas Razor sin errores, backoffice intacto.

## 10. DECISIONES DE DISEÑO DE ESTA POSTA (para confirmar)

A) VidrieraService consulta JKContext directo (filtrado por TenantId), NO
   BaseRepository (depende del claim) ni GlobalRepository (pertenece a plataforma).
   -> unica via para consulta publica anonima. Validar.

B) 404 con HttpNotFound() estandar (sin vista custom "no disponible") en v1.
   -> Se puede sumar pagina dedicada en Posta 8 si hace falta.

C) Nombre del layout: _VidrieraLayout.cshtml (nuevo), no reutiliza _LoginLayout
   para no arrastrar estructura/login/CSS.

D) Vista solo identidad + servicios en 3; oferta/mensaje/disponibilidad
   quedan en el modelo y NO se renderizan en esta posta (futuras).

E) No se pasa nunca TenantId por URL; el controller solo recibe slug.

## 11. PENDIENTE PARA POSTAS SIGUIENTES

- Posta 4: calculo de slots / disponibilidad real.
- Posta 5: panel de gestion (publicar/despublicar, mensaje, oferta, flags).
- Posta 6: refresh periodico.
- Posta 7: modo monitor.
- Posta 8: seguridad/cache/aislamiento formal + vista "no disponible" opcional.

## 12. POSTA 3B - CIERRE FORMAL (2026-09-02, aprobado por TEAM LEAD)

La Posta 3 = 3A (funcional) + 3B (publicacion/cierre). Alcance de 3B coreografiado
para NO pisar las Postas siguientes (en particular el modo monitor = Posta 7):

### 12.1 Alcance de 3B
- Definicion formal del alcance restante de la Posta 3 (esta seccion).
- Revision y confirmacion de los 8 criterios de aceptacion de la Posta 3.
- Verificacion de compilacion de vistas Razor (`MvcBuildViews`) = parte del criterio 8.
- Presentacion legible en pantallas grandes/TV y en captura (criterio 7 del
  Contrato) SIN interactividad de modo monitor (eso es Posta 7).
- Cierre documental: actualizacion de decisiones (D004/D005) y del plan.

EXCLUIDO de 3B (sigue fuera, como en el resto de la Posta 3):
- Ofertas, mensaje destacado y disponibilidad (se mantienen en el modelo, NO se
  renderizan; postas 4/5).
- Modo monitor interactivo / presentacion automatica (Posta 7).
- Refresh periodico (Posta 6), panel de gestion (Posta 5), seguridad formal (Posta 8).

### 12.2 Verificacion de los criterios de aceptacion (Posta 3)

Verificado en runtime (IIS Express + SQL real, 2026-09-02):

| Criterio | Resultado |
|---|---|
| 1. /vidriera/jk (activo + habilitada) -> 200, identidad + servicios; precio solo si MostrarPrecios | OK (6 servicios; precio presente con MostrarPrecios=1 y ausente con MostrarPrecios=0) |
| 2. Slug inexistente -> 404 | OK (/vidriera/noexiste) |
| 3. Vidriera.Habilitada=false -> 404 | OK (/vidriera/deshabilitada) |
| 4. Vidriera.IsActive (fila)=false -> 404 | OK (/vidriera/inactiva) |
| 5. AISLAMIENTO: /vidriera/jk nunca muestra datos de otro tenant | OK (sin servicio 'Barberia' del tenant 2; todo filtrado por TenantId desde el slug) |
| 6. Read-only: sin POST, sin enlaces internos, no abre login | OK (layout publico standalone sin sidebar/login) |
| 7. Funciona sin sesion ([AllowAnonymous]) | OK (200 sin autenticacion) |
| 8. Build 0 errores + vistas Razor sin errores | OK (MSBuild 0 errores; MvcBuildViews=true 0 errores; backoffice /Account/Login intacto 200) |

### 12.3 Presentacion (criterio 7 del Contrato)
La vidriera se presenta centrada (`max-width:720px`) sobre `--jk-bg`, con logo/
inicial, nombre de negocio como <title> y listado de servicios. Es legible y rinde
bien en captura y en pantallas anchas (la columna centrada no se estira sin control).
No se agrego logica ni markup nuevo; el modo monitor dedicado queda a Posta 7.

### 12.4 Cierre
Posta 3 (3A + 3B) CERRADA y CONSOLIDADA. Pendientes: Postas 4-8.

## 13. POSTA 4 - DISPONIBILIDAD REAL (2026-09-02, decisiones aprobadas por TEAM LEAD)

Objetivo: mostrar en la vidriera los proximos slots libres calculados desde
Turno + EstadoTurno + Recurso, gobernados por el flag MostrarDisponibilidad.

### 13.1 Decisiones de diseno (aprobadas)

- A) La disponibilidad se presenta POR RECURSO (no por servicio). Razon: un turno
     se asigna a un Recurso (Turno.RecursoId), asi que la ocupacion se calcula
     directa y sin ambiguedad desde la fuente de verdad de turnos. Presentarlo por
     servicio exigiria agregar slots de recursos vinculados (RecursoServicio) y
     puede mostrar horarios que no respetan la duracion del servicio.
- B) Alcance temporal: HOY (jornada fija 08:00-19:30, D-012, limitacion v1 hasta
     M-01). Coherente con el backoffice (agenda por dia). No se recorren dias.
- C) Regla de ocupacion: IDENTICA a TurnoService.ValidarDisponibilidad (la fuente
     de verdad del negocio): una franja de 30 min esta ocupada si se solapa con un
     turno del recurso en estado NO Cancelado (Codigo != CANCELADO; D-040). El
     turno Cancelado libera su slot (D-042).
- D) La seccion se muestra SOLO si MostrarDisponibilidad=true.
- E) Estado vacio SIEMPRE indicado ("Sin turnos disponibles hoy") cuando no hay
     slots libres (recurso sin disponibilidad o jornada ocupada).

### 13.2 Contrato del metodo

```
public List<VidrieraDisponibilidadVM> GetAvailableSlots(int tenantId)
```

Devuelve por recurso activo del tenant: Nombre + lista de horas libres (HH:mm)
de 30 min desde "ahora" hasta las 19:30 de hoy. Un recurso SIN slots libres
devuelve la lista vacia (la vista muestra el estado vacio).

### 13.3 Archivos

NUEVOS:
- Models/ViewModels/VidrieraDisponibilidadVM.cs (recurso + slots libres)

MODIFICADOS:
- Services/VidrieraService.cs (GetAvailableSlots + cargar disponibilidad en
  GetVidrieraPublica cuando MostrarDisponibilidad)
- Models/ViewModels/VidrieraPublicaViewModel.cs (+ Disponibilidad, + MostrarDisponibilidad)
- Views/Vidriera/Index.cshtml (seccion Disponibilidad)
- Content/Pages/jk-vidriera.css (estilos de la seccion)
- JKTurnos.csproj (+Compile del nuevo viewmodel)

### 13.4 Criterios de aceptacion (Posta 4)

1. MostrarDisponibilidad=false -> NO se renderiza la seccion (200).
2. MostrarDisponibilidad=true -> se muestra por recurso con los slots libres de hoy.
3. Un recurso sin slots libres (jornada ocupada o sin turnos... ) -> estado vacio
   "Sin turnos disponibles hoy".
4. Aislamiento: la disponibilidad se filtra por TenantId (mismo tenant que el slug).
5. La regla de ocupacion respeta Cancelados (liberan slot) y es la misma del backoffice.
6. Build 0 errores, vistas Razor OK, seccion no rompe la vista cuando no hay recursos.

## 14. POSTA 5 - PANEL DE ADMINISTRACION DE LA VIDRIERA (2026-09-02, decisiones aprobadas por TEAM LEAD)

Objetivo: que el tenant Admin gestione su vidriera publica desde el backoffice
(crear/publicar, editar contenido, flags y despublicar) sin tocar la BD y sin que
la Vista Publica (read-only) exponga ningun camino de edicion.

### 14.1 Decisiones de diseno (aprobadas)

- A) Seccion de Gestion del panel, restringida a rol Admin
     (`[Authorize(Roles = Roles.Admin)]`). Controller nuevo VidrieraAdminController
     con dos actions: `Index` (GET) y `Guardar` (POST). NO es publico.
- B) Form propio: la vista `Views/VidrieraAdmin/Index.cshtml` postea directo a
     `Guardar` (no reutiliza _FormLayout); reutiliza _PageHeader y _FormActions y
     el sistema de badges/checkbox del DS v2 (.jk-checkbox, jk-badge-success,
     jk-badge-neutral).
- C) Slug editable UNICO GLOBAL: `NormalizarSlug` (minusculas, sin tildes ni
     espacios, max 100). `GuardarVidriera` rechaza el slug si YA lo usa OTRO tenant
     (excluye el propio registro) con el mensaje "Ese slug ya esta en uso. Elegi
     otro." y NO persiste nada. Asi no se rompe la resolucion por slug unico (D-042).
- D) Upsert por TenantId: si la fila no existe, `Guardar` la crea; si existe, la
     actualiza. Despublicada (Habilitada=false) o fila inexistente -> 404 publico.
- E) Auditoria E_Vidriera (Crear/Editar) con el usuario Admin logueado.
- F) La vista muestra un badge de estado (publicada / despublicada / sin vidriera)
     y el enlace de vista previa publica (`/vidriera/{slug}`).

### 14.2 Archivos

NUEVOS:
- Models/ViewModels/VidrieraAdminViewModel.cs
- Controllers/VidrieraAdminController.cs
- Views/VidrieraAdmin/Index.cshtml

MODIFICADOS:
- Services/VidrieraService.cs (GetVidrieraAdmin, GuardarVidriera, NormalizarSlug,
  GetUrlPublica, MapAdmin; fix actualId para el quirk EF6 de TargetException)
- Services/AuditService.cs (E_Vidriera)
- Views/Shared/_Sidebar.cshtml (item "Vidriera", icono bi-globe2, grupo Configuracion)
- JKTurnos.csproj (registro de los 3 nuevos)

### 14.3 Criterios de aceptacion (Posta 5)

1. Paneles Admin ven el item "Vidriera" y el form carga la fila actual (o estado vacio).
2. Crear/publicar un slug nuevo -> /vidriera/{slug} responde 200 con el contenido.
3. Slug ya usado por OTRO tenant -> mensaje "Ese slug ya esta en uso. Elegi otro."
   y NO se persiste nada (unicidad global).
4. Despublicar (Habilitada=false) -> /vidriera/{slug} responde 404.
5. Republicar y guardar flags -> la vista publica refleja el flag (p. ej. MostrarPrecios).
6. Las acciones Crear/Editar quedan en auditoria (entidad Vidriera, usuario Admin).
7. Sin vidriera + guardar -> crea la fila; el badge/referido del panel refleja publicacion.
8. Build 0 errores, vistas Razor OK, Vista Publica (read-only) intacta sin caminos de edicion.

## 15. POSTA 5 - CIERRE FORMAL (2026-09-02)

Verificado en runtime (IIS Express + SQL real, 2026-09-02, login como
gonzo@jkturnos.com Tenant 2 y jk@jkturnos.com Tenant 1):

| Criterio | Resultado |
|---|---|
| 1. Panel GET 200 + form con estado actual | OK |
| 2. Crear 'mi-negocio' -> /vidriera/mi-negocio 200 | OK |
| 3. Slug duplicado cross-tenant -> "Ese slug ya esta en uso. Elegi otro." + sin fila nueva | OK |
| 4. Unpublish -> /vidriera/mi-negocio 404 | OK |
| 5. Republish + flags -> 200 + flags persistidos en DB | OK |
| 6. Auditoria Crear/Editar (entidad Vidriera) | OK (filas 231 Crear / 234+236 Editar) |
| 7. Panel GET sin vidriera + guardar crea la fila | OK |
| 8. Build 0 errores + MvcBuildViews 0 errores + Vista Publica intacta | OK |

Posta 5 CERRADA y CONSOLIDADA. Pendientes: Postas 6-8.

## 16. VISUALIZACION DE LA VISTA PUBLICA + REDISENO MONITOR (2026-09-02, aprobadas por TEAM LEAD)

### 16.1 Extension del rendering (mensaje / oferta / servicios con flag)

La Vista Publica (Vidriera/Index.cshtml) originalmente solo renderizaba identidad,
servicios y disponibilidad: los datos de mensaje/oferta se guardaban desde el panel
(Posta 5) pero NUNCA se desplegaban. Se completo el pipeline:

- `Models/ViewModels/VidrieraPublicaViewModel.cs`: + Titulo, Descripcion,
  MensajePrincipal, OfertaTitulo, OfertaDescripcion y los flags
  MostrarServicios/MostrarPrecios/MostrarOfertas/MostrarMensaje.
- `Services/VidrieraService.cs GetVidrieraPublica`: mapea todos esos campos.
- `Views/Vidriera/Index.cshtml`: banner de Bienvenida (MensajePrincipal si
  MostrarMensaje), seccion Oferta (OfertaTitulo/Descripcion si MostrarOfertas),
  y la seccion Servicios queda condicionada a MostrarServicios.

### 16.2 Logo / avatar del negocio

La marca de la vidriera se resuelve con la MISMA precedencia del resto del sistema
(BaseController.ViewBag.ConfiguracionLogo, D-026/D-036): LogoUrl propio (logo del
negocio) > AvatarUrl (avatar precargado) > fallback marca circular con la inicial
sobre el gradiente tricolor. Antes solo se mapeaba LogoUrl, ignorando el avatar
precargado. Se verifico runtime en 3 casos (logo / solo avatar / ninguno -> fallback).

### 16.3 Rediseno MONITOR (pantalla LED 17", sin scroll)

Objetivo: que toda la vidriera entre en una pantalla LED de 17" (min 1024x768)
SIN necesidad de hacer scroll, prolija y ordenada, mas publicitaria, siguiendo
siempre el DS v2 (D-035/D-036).

`Content/Pages/jk-vidriera.css` reescrito:
- `.jk-vidriera` = `height:100vh` + `overflow:hidden` (marco fijo, sin scroll).
- Layout DASHBOARD de 2 columnas: grid `1.35fr 1fr`; izquierda = tarjeta Servicios,
  derecha = tarjeta Oferta (arriba) + Disponibilidad (abajo).
- Header: marca circular (logo/avatar/fallback) + nombre en Nunito display +
  titulo (tagline) + descripcion + contacto en chips.
- Banner de bienvenida con gradiente tricolor (coral/butter/teal, D-036).
- Tarjeta de oferta con acento/borde ambar (--jk-warning) y sello "Oferta".
- Slots de disponibilidad en teal (--jk-teal) como pills.
- Blob decorativo de marca y sombras calidas del DS.
- Listas con `overflow-y:auto` interno: nunca rompen el marco fijo del monitor.
- Responsive a columna simple (grid 1fr, body scrollable) <840px.

Verificado en 1024x768 via Chrome DevTools (CDP, headless Edge): `docScrollH==clientH`
(sin scroll page), sin overflow horizontal, y la lista de servicios cabe sin clip
interno (content 277px en contenedor 383px). Build 0 errores (MvcBuildViews).

### 16.4 Cierre

Rediseno MONITOR CONSOLIDADO (aprobado por TEAM LEAD). La Base de Datos se dejo
limpia (filas VidrieraPublicas = 0, tenant 3 sin logo/avatar). Pendientes M-13:
Posta 6 (refresh), 7 (modo monitor), 8 (seguridad). El "modo monitor" que en el
Contrato se presenta en pantallas grandes como parte de lo visual ya esta servido
por este rediseno; la Posta 7 sumaria la interactividad de presentacion automatica.

