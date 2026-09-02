# MANUAL GENERICO - COMO IMPLEMENTAR UNA VIDRIERA O VITRINA PUBLICA

> Tutorial reutilizable, desacoplado del proyecto concreto. Explica la METODOLOGIA,
> las TECNOLOGIAS y el PASO A PASO (archivos) que se usaron para construir la
> "Vidriera Publica" (M-13) en el sistema JKTurnos. Sirve como plantilla para
> reproducir una vitrina de iguales caracteristicas en cualquier otro sistema que
> la necesite.
>
> ALCANCE: secciones 1-9 = implementacion BASE (Postas 1-5, ya consolidada).
> Secciones 10-11 = extensiones de vitrina/monitor: **Posta 6 (refresh periodico)**
> y **Posta 7 (modo monitor automatico)**, presentadas como METODOLOGIA generica
> de implementacion.

---

## 1. QUE ES UNA VIDRIERA (requisito funcional)

Una **vidriera publica** es una pagina web de SOLO LECTURA, accesible sin login,
que muestra una "vitrina digital" de un negocio: su identidad, sus servicios y su
disponibilidad del dia. Caracteristicas no negociables que implementamos:

- **Unica entrada publica por URL**: normalmente `/vidriera/{slug}`.
- **Read-only total**: no hay formularios, no se escribe nada desde el publico.
- **Aislamiento por tenant/negocio**: la URL identifica UN negocio y NUNCA expone
  datos de otro.
- **Contenido edible desde un panel privado** (backoffice) y mostrado en una vista
  publica separada.
- **Modo vitrina / monitor**: la pagina se ve bien en una pantalla LED/TV de
  17" en adelante, sin scroll.

> Metodologia de fondo: **separar la VISTA PUBLICA (anonima, read-only) del PANEL
> DE GESTION (autenticado)**. Un solo servicio (la fuente de verdad) alimenta
> ambas, garantizando que no haya dos sistemas de datos ni duplicacion.

---

## 2. TECNOLOGIAS UTILIZADAS

Este manual se escribio sobre un stack concreto, pero el patron es portable:

| Capa | Tecnologia (JKTurnos) | Rol |
|---|---|---|
| Framework | ASP.NET MVC 5 (.NET Framework 4.7) | Router + Controllers + Vistas Razor |
| ORM | Entity Framework 6 | Acceso a datos / modelo entidad |
| BD | SQL Server Express | Persistencia |
| Autenticacion | OWIN Cookie Auth | Solo para el panel (la vista publica es anonima) |
| Hashing clave | BCrypt.Net | (opcional, de contexto) |
| Estilos | CSS plano + "Design System" de tokens (`--jk-*`) | Identidad visual consistente |
| Iconos/fuentes | Bootstrap Icons + Google Fonts | Iconografia y tipografia de la vista |
| Versionado | Git repo | Seguimiento |

> La electronica mas importante no es la tecnologia: es el **contrato de capas**
> (Controller coordina -> Service contiene reglas -> Modelo/ORM persiste) y la
> separacion publico/privado.

---

## 3. PATRON DE ARQUITECTURA (por que funciona)

El flujo es una unica cadena que NUNCA se rompe:

```
URL publica  ->  Controller (coordinador)  ->  Service (reglas)  ->  Entidad/BD
                                    ^                                    |
                                    +---------- ViewModel (solo lo publico)
```

Reglas que aplicamos:

1. **El Controller solo coordina**: recibe el `slug`, llama al Service, devuelve
   `View(vm)` o `404`.
2. **El Service es la fuente de verdad**: resuelve el negocio por slug, filtra por
   su tenant y arma el ViewModel. Contiene TODA la logica.
3. **El ViewModel publico solo expone datos publicos**: no filtra `Id`, `TenantId`
   ni campos internos.
4. **Aislamiento por tenant**: el `TenantId` se obtiene SOLO desde el slug de la
   URL. Nunca desde query/header. Toda consulta posterior filtra por ese mismo id.
5. **La vista publica es anonima** (`[AllowAnonymous]`); el **panel es
   autenticado** (`[Authorize(Roles="Admin")]`).

---

## 4. MODELO DE DATOS (lo que necesita una vidriera)

Se agrego UNA entidad que es la CONFIGURACION de la vitrina (1:1 con el negocio),
sin duplicar servicios/turnos/precios (eso se consulta en tiempo real):

| Campo | Que es | Tipo |
|---|---|---|
| Slug | identificador publico unico (lower, sin espacios) | string, UNIQUE |
| Habilitada | interruptor de publicacion | bool |
| Titulo / Descripcion | tagline y presentacion | string (opcional) |
| MensajePrincipal | banner de bienvenida / aviso | string (opcional) |
| OfertaTitulo / OfertaDescripcion | promo destacada | string (opcional) |
| MostrarServicios | mostrar la seccion de servicios | bool |
| MostrarPrecios | mostrar el precio de cada servicio | bool |
| MostrarDisponibilidad | mostrar los slots del dia | bool |
| MostrarOfertas / MostrarMensaje | mostrar las secciones de oferta/mensaje | bool |

> Principio clave (comercial): **los contenidos opcionales NUNCA impiden publicar**.
> La ausencia de precio/oferta/mensaje no bloquea nada. El contenido principal son
> los servicios (todos los activos del negocio).

---

## 5. PASO A PASO (archivos por capa, en orden)

A continuacion, los archivos que se crearon/modificaron y en que orden, para
reproducir la funcionalidad. La numeracion es el orden logico de implementacion.

### PASO 1 - La entidad (modelo de datos)

**Archivo nuevo**: `Models/Entities/*VidrieraPublica.cs`
- Propiedades de la tabla de configuracion (tabla de la seccion 4).
- Herencia de una base comun (tenant + soft delete `IsActive`, si el proyecto la tiene).
- 1:1 con el negocio via FK `TenantId`.

**Archivo modificado**: `Data/JKContext.cs` (o el DbContext de tu ORM)
- Agregar `DbSet<...VidrieraPublica>`.
- Configurar los indices UNIQUE (Slug global, TenantId) en el mapeo (`OnModelCreating`).

### PASO 2 - La migracion (schema)

Segun tu stack: aqui se uso **auto-migracion de EF6** porque el mecanismo
`Add-Migration` estaba fuera de alcance del host y fabricar el snapshot a mano era
frágil. Para una tabla NUEVA (operacion aditiva, sin perdida de datos) la auto-
migracion la crea al arrancar con los indices del mapeo.
> Decisión documentada (D-043). En otro proyecto puedes usar tu flujo de
> migraciones normal; lo importante es que la tabla y los UNIQUE queden creados.

### PASO 3 - La ruta (URL publica)

**Archivo modificado**: `App_Start/RouteConfig.cs`
- Registrar una ruta ANTES de la ruta default `{controller}/{action}/{id}`:
  ```
  routes.MapRoute(
      name: "Vidriera",
      url: "vidriera/{slug}",
      defaults: new { controller = "Vidriera", action = "Index", slug = UrlParameter.Optional }
  );
  ```
- Asi la ruta default no captura `/vidriera/{algo}` como si fuera un controller.
- Unica llave de resolucion: el `slug`.

### PASO 4 - El ViewModel publico

**Archivo nuevo**: `Models/ViewModels/*VidrieraPublicaViewModel.cs`
- SOLO campos publicos: slug, identidad (nombre, logo, telefono, email, direccion),
  flags de visibilidad, contenido manual y el listado de servicios + disponibilidad.
- MAS un `VidrieraServicioVM` (nombre, descripcion, duracion, precio, mostrarPrecio)
  y un `VidrieraDisponibilidadVM` (recurso + slotsLibres).
- **Regla dura**: no exponer `Id`, `TenantId` ni campos internos.

### PASO 5 - El Service (logica y aislamiento)

**Archivo nuevo**: `Services/*VidrieraService.cs`
- `GetVidrieraPublica(slug)`:
  1. slug -> fila con `Slug == slug && IsActive && Habilitada`.
  2. El tenant debe estar activo (si no -> null).
  3. `tenantId = vidriera.TenantId` (UNICA fuente, no del navegador).
  4. Cargar identidad desde la configuracion del negocio.
  5. Cargar servicios ACTIVOS del MISMO tenant.
  6. Resolver el **logo** con precedencia del sistema: logo propio > avatar precargado
     > null (fallback visual).
  7. Armar el ViewModel.
- `GetAvailableSlots(tenantId)`: devuelve por recurso los slots libres del dia.
  Regla de ocupacion = la MISMA del backoffice (un turno NO cancelado ocupa; el
  cancelado libera). Asi la vitrina y la agenda nunca se contradicen.
- Metodos de gestion (panel): `GetVidrieraAdmin`, `GuardarVidriera`
  (upsert por tenant, normaliza el slug, valida unicidad GLOBAL), `GetUrlPublica`.
- Conectar al ORM de forma controlada (no via un repositorio que dependa del
  claim de sesion, porque la vista publica es anonima).

### PASO 6 - El Controller publico

**Archivo nuevo**: `Controllers/*VidrieraController.cs`
- `[AllowAnonymous]` + cache `NoStore` (no servir datos stale).
- UNICA action `Index(string slug)`:
  1. `var vm = _service.GetVidrieraPublica(slug);`
  2. `if (vm == null) return HttpNotFound();`
  3. `return View(vm);`
- Sin POST, sin inicio de sesion, sin enlaces internos.

### PASO 7 - El Controller del panel (gestion)

**Archivo nuevo**: `Controllers/*VidrieraAdminController.cs`
- `[Authorize(Roles = Roles.Admin)]` (panel, NO publico).
- `Index` (GET): muestra el formulario con el estado actual o el "sin crear".
- `Guardar` (POST): valida + llama a `GuardarVidriera` + redirige (o devuelve el
  form con el error de slug duplicado).
- Auditoria de las acciones (en JKTurnos: `AuditService`, entidad `Vidriera`).

### PASO 8 - El Layout publico (independiente)

**Archivo nuevo**: `Views/Shared/_VidrieraLayout.cshtml`
- DOCTYPE completo (meta, title, CSS del design system + CSS propio).
- `<body>` standalone: un contenedor `.jk-vidriera` + `@RenderBody()`.
- **NO** renderiza sidebar, topbar, ni CSS/JS del panel.
- NO hay navegacion interna; solo muestra informacion.

### PASO 9 - La vista publica

**Archivo nuevo**: `Views/*Vidriera/Index.cshtml`
- `Layout = "~/Views/Shared/_VidrieraLayout.cshtml";`
- Secciones (cada una condicionada a su flag):
  - **Header**: logo/avatar + nombre + titulo + descripcion + contacto (chips).
  - **Mensaje/Bienvenida**: banner si `MostrarMensaje` y hay texto.
  - **Servicios**: lista si `MostrarServicios`; precio si `MostrarPrecios`.
  - **Oferta**: tarjeta si `MostrarOfertas` y hay contenido.
  - **Disponibilidad**: si `MostrarDisponibilidad`, por recurso con slots.

### PASO 10 - El panel de gestion (vista)

**Archivo nuevo**: `Views/*VidrieraAdmin/Index.cshtml`
- Formulario propio que POSTea a `Guardar` (anti-forgery).
- Slug editable + hint + mensaje de ya-en-uso.
- Checkbox Habilitada (publicar/despublicar).
- Campos de contenido (titulo/descripcion/mensaje/oferta).
- Flags de visibilidad.
- Badge de estado (publicada/despublicada/sin vidriera) + enlace "Ver publica".

### PASO 11 - CSS propio (estilo vitrina/monitor)

**Archivo nuevo**: `Content/Pages/*jk-vidriera.css`
- Seccion por bloque (header, mensaje, servicios, oferta, disponibilidad).
- TONO VITRINA/MONITOR, con el design system (tokens del proyecto, no valores
  hardcodeados):
  - Contenedor `height:100vh` + `overflow:hidden` (marco fijo: entra en 17" sin scroll).
  - Layout dashboard de 2 columnas para aprovechar el ancho.
  - Listas con `overflow-y:auto` interno (nunca rompen el marco).
  - Colores publicitarios del sistema (gradiente de marca, acento, ambar para la
    oferta, teal para la disponibilidad).
  - Blobs/halos decorativos de identidad.
  - Responsive a columna simple por debajo de un breakpoint.

### PASO 12 - Navegacion del panel

**Archivo modificado**: `Views/Shared/_Sidebar.cshtml` (o el layout del backoffice)
- Item "Vidriera" (con icono) en el grupo de Configuracion, con resaltado activo.

---

## 6. COSAS A TENER CUIDADO (trampas frecuentes)

- **Aislamiento**: verificar que `/vidriera/slugA` NUNCA muestre datos de otro
  negocio, aun manipulando querystring/parametros. El service filtra TODO por el
  tenant del slug.
- **Read-only**: la vista publica no debe tener POST (solo el panel).
- **Cero scroll en el monitor**: probar a 1024x768 con un headless browser y
  comparar `scrollHeight` vs `clientHeight`. Las listas internas deben scrollear,
  no la pagina.
- **Dos sistemas de datos, prohibido**: la vitrina consulta los servicios/turnos/
  precios reales; no copia datos.
- **Unicidad de slug**: el slug es la llave publica. Validar la duplication GLOBAL
  (otro negocio no puede usar el mismo) y normalizarlo (minusculas, sin espacios,
  sin tildes).
- **El logo**: decidir una precedencia unica (en este sistema: logo propio >
  avatar precargado > fallback) y usarla en TODO el sistema para que sea coherente.
- **Contenido opcional no bloquea**: un negocio sin precios/oferta/mensaje igual
  publica; el contenido principal son los servicios.
- **Vista publica anonima = sin claims**: el Service publico NO puede depender de
  la sesion (usa el ORM directo), a diferencia del resto de los servicios del panel.

---

## 7. CRITERIOS DE ACEPTACION (como saber que esta terminado)

1. `/vidriera/{slug}` con negocio activo + habilitada -> 200, muestra identidad y servicios.
2. Slug inexistente -> 404.
3. `Habilitada=false` -> 404 (no se expone).
4. Negocio inactivo -> 404.
5. Aislamiento: un slug nunca muestra datos de otro negocio.
6. Read-only: sin POST, sin enlaces internos, funciona sin login.
7. Entra en 17" (1024x768) sin scroll de pagina.
8. Bandera logo/avatar: logo propio > avatar > fallback.
9. El panel gestiona slug/content/flags y publicar/despublicar (despublicada -> 404).
10. Build 0 errores + vistas Razor compilan + el resto del sistema intacto.

---

## 8. RESUMEN DE ARCHIVOS (checklist)

**Nuevos**
- [ ] `Models/Entities/VidrieraPublica.cs` (entidad)
- [ ] `Models/ViewModels/VidrieraPublicaViewModel.cs`
- [ ] `Models/ViewModels/VidrieraServicioVM.cs`
- [ ] `Models/ViewModels/VidrieraDisponibilidadVM.cs`
- [ ] `Models/ViewModels/VidrieraAdminViewModel.cs`
- [ ] `Services/VidrieraService.cs`
- [ ] `Controllers/VidrieraController.cs` (publico)
- [ ] `Controllers/VidrieraAdminController.cs` (panel)
- [ ] `Views/Shared/_VidrieraLayout.cshtml`
- [ ] `Views/Vidriera/Index.cshtml` (publica)
- [ ] `Views/VidrieraAdmin/Index.cshtml` (panel)
- [ ] `Content/Pages/jk-vidriera.css`

**Modificados**
- [ ] `Data/JKContext.cs` (+DbSet + indices UNIQUE)
- [ ] `App_Start/RouteConfig.cs` (+ruta "Vidriera")
- [ ] `Ownership/*.csproj` (registrar los nuevos .cs/vistas/css)
- [ ] `Views/Shared/_Sidebar.cshtml` (item "Vidriera")
- [ ] `Services/AuditService.cs` (auditoria de la entidad, si aplica)

---

## 9. VERIFICACION RUNTIME (procedimiento baseline)

1. Compilar con validacion de vistas (p. ej. `MSBuild /p:MvcBuildViews=true`).
2. Levantar el servidor en un puerto local.
3. Crear/activar un negocio con servicios y una fila de vidriera publicada.
4. Probar la URL publica (200), un slug inexistente (404) y una despublicada (404).
5. Probar el panel logueado: crear, editar, duplicar slug (error), despublicar,
   republisar, y verificar que la auditoria quede.
6. Cabecera headless a 1024x768: medir que `docScrollH == clientH` (sin scroll).
7. Verificar bandera de logo/avatar en los 3 casos.

---

## 10. POSTA 6 - REFRESH PERIODICO (metodologia)

> Objetivo: que la vidriera se actualice SOLA para reflejar cambios de
> disponibilidad/servicios sin que nadie recargue la pagina. Es la base del uso
> como monitor o cartelera.

### 10.1 Decisiones de diseno

- Escala por fases: empezar con **la solucion mas simple** y escalar solo si hace
  falta.
  - **v1 - Meta refresh / fetch ligero del partial** (recomendado).
  - **v2 - SignalR/WebSockets** (solo si se necesita actualizacion instantanea
    de varios monitores).
- Intervalo sugerido: refrescar cada ~30 s (max), constante centralizada.
- El refresh debe traer **solo los datos que cambian** (disponibilidad, y de paso
  servicios/identidad), SIN recargar toda la pagina (evita parpadeo y pierde
  la posicion de scroll en la vitrina).

### 10.2 Archivos / capas

**Nuevos**
- `Views/*Vidriera/_Disponibilidad.cshtml` (partial): muestra el bloque de
  disponibilidad por recurso + slots; reutilizable por todo el refresh.
- (Opcional) controlador/endpoint que devuelva ese partial por AJAX.

**Modificados**
- `Views/*Vidriera/Index.cshtml`: agregar la **meta refresh** en el `<head>`
  (o el `fetch` que reemplaza el inner del contenedor por el partial).
- `Content/Pages/jk-vidriera.js` (nuevo): timer `setInterval` + `fetch` +
  `DOMParser` para reemplazar solo el bloque de datos (sin recargar scroll ni
  los listeners). Patron identico al "refresco suave" del backoffice
  (`refreshAgendaGrid` con `data-...-content-url`).
- `Controllers/*VidrieraController.cs`: action GET corta que devuelve el partial
  (legible para el fetch), con cache controlada.

### 10.3 Trampas

- **No cachear** el partial (headers NoStore): un monitor stale muestra
  disponibilidad vieja.
- El refresh NO debe reiniciar el estado visual (scroll, foco) ni parpadear.
- Mantener el aislamiento por slug en el endpoint que refresca, igual que la
  vista principal.
- Si la pagina no esta enfocada (pestaña de fondo), el navegador puede pausar
  los timers: no asumir que el intervalo corre siempre.

---

## 11. POSTA 7 - MODO MONITOR AUTOMATICO (metodologia)

> Objetivo: convertir la vidriera en una presentacion AUTOMATICA pensada para
> pantallas/TV, sin interaccion humana. Va mas alla del rediseno visual base
> (que ya entra en 17"): suma presentacion por bloques/ciclos.

### 11.1 Decisiones de diseno

- **Modo monitor** = un estado de la MISMA vista publica, activable por
  parametro/flag (segun el contrato: "Activar/desactivar el modo monitor").
- Presentacion por **ciclos/bloques**: en lugar de mostrar todo de una vez, rotar
  secciones (identidad -> servicios -> disponibilidad -> oferta) en pantalla
  completa, con intervalo configurable.
- Base sobre el rediseno monitor de 2 columnas (PASO 11 del manual):
  - En una pantalla alcanza con la vista estatica completa; el modo monitor
    agrega la ROTACION cuando el contenido no entra o se quiere enfatizar.
- **Sin interaccion**: todo automatico (timers), sin botones ni hover que
  bloqueen.
- Enfatizar lo que mas vende en cada bloque (ofertas/mensaje en un pasaje propio).

### 11.2 Archivos / capas

**Nuevos**
- `Views/*Vidriera/_Monitor*.cshtml` (parciales por bloque: identidad, servicios,
  disponibilidad, oferta).
- `Content/Pages/jk-monitor.js`: gestiona el ciclo de presentacion (avanzar de
  bloque, pausar en hover opcional, indicador de progreso).

**Modificados**
- `Views/*Vidriera/Index.cshtml` + `_VidrieraLayout.cshtml`: detectar el modo
  monitor (parametro/flag) y renderizar el layout de presentacion en pantalla
  completa (ocultar scroll, marcos).
- `Core/Pages/jk-vidriera.css`: estilos de bloque a pantalla completa y
  transiciones.
- `Controllers/*VidrieraController.cs` + `Models/ViewModels/*VidrieraPublicaViewModel`:
  exponer el flag de modo monitor para que la vista decida.

### 11.3 Trampas

- El modo monitor debe **combinarse con el refresh periodico (Posta 6)** para
  que el ciclo siempre muestre datos actuales.
- Considerar la **presentacion automatica** (rotacion) separada de la
  interactividad: no agregar controles que un monitor no puede usar.
- Que los intervalos sean configurables (fijados como constantes en v1).
- Conservation del aislamiento por slug en cada bloque.

---

## 12. RELACION ENTRE POSTAS (para no duplicar)

| Posta | Que aporta | Depende de |
|---|---|---|
| 1-5 (base) | Vista publica + panel + rediseno monitor estatico | - |
| 6 | La vista se actualiza sola (refresh ~30 s) | base |
| 7 | Presentacion automatica por bloques en pantalla completa | base + 6 |
| 8 | Seguridad/cache formal + vista "no disponible" | todas |

> La Posta 7 se apoya en el rediseno monitor (que ya hace que la vista entre en
> una 17" sin scroll) y suma la rotacion/presentacion; no rehace el layout.