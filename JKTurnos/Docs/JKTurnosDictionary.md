# JKTurnos Dictionary

## Diccionario Tecnico del Proyecto

Version: 1.0

Fecha: 23/7/2026

---

# 1. Filosofia del proyecto

JKTurnos se construye aplicando seis principios fundamentales en cada capa del codigo.

## 1.1 Componentes reutilizables

Todo fragmento de interfaz que se repite debe convertirse en un componente.

Los componentes viven en `Views/Shared/Components/` como partials Razor.

Un componente resuelve un problema visual o funcional especifico.

No se crean componentes por adelantado; se crean cuando existe una necesidad real detectada por repeticion.

## 1.2 Design System

Los estilos visuales no se escriben por pagina.

Todos los valores de diseno (colores, tipografia, espaciado, sombras) se definen como variables CSS en `jk-tokens.css`.

Los componentes UI se construyen con clases del DS (`.jk-card`, `.jk-btn`, `.jk-input`, etc.).

Nunca se hardcodean colores ni medidas en las vistas.

## 1.3 Composicion de vistas

Una vista no es un archivo HTML monolitico.

Se compone mediante:

- Un `@model` que define los datos
- Bloques `@helper` que construyen partes de la interfaz
- Asignacion de esos bloques a `ViewBag`
- Un `@Html.Partial` final que renderiza un componente contenedor

La vista del modulo decide el contenido; el componente decide la estructura.

## 1.4 Responsabilidad unica

Cada archivo tiene una responsabilidad clara:

- Controller: coordina, no contiene logica de negocio
- Service: contiene todas las reglas de negocio
- Repository: accede a datos, filtra por TenantId
- View: solo muestra informacion
- Partial: representa un componente reutilizable
- Helper: construye un bloque de interfaz dentro de la vista

## 1.5 DRY (Don't Repeat Yourself)

No se duplica HTML.

Si el mismo patron aparece en dos vistas, se extrae a un componente.

Si el mismo estilo aparece en dos paginas, se agrega al Design System.

## 1.6 Thin Views

Las vistas contienen la minima logica posible.

La logica de presentacion (que mostrar, cuando mostrar) se resuelve en el Controller o en helpers.

Las vistas no llaman Services ni Repositories directamente.

---

# 2. Flujo completo de una vista

El recorrido completo desde la URL hasta el HTML final.

## Ejemplo: Clientes/Index

### 2.1 Controller

`Controllers/ClientesController.cs`:

```csharp
public ActionResult Index()
{
    var clientes = _service.GetAll();
    return View(clientes);
}
```

El Controller obtiene los datos desde el Service y los pasa como Model a la View.

No construye HTML, no toca ViewBag para datos de presentacion.

### 2.2 Model

El Model es `List<Cliente>`.

Es una lista de entidades del negocio.

La vista itera sobre ella para generar filas o cards.

### 2.3 Index.cshtml

`Views/Clientes/Index.cshtml`:

```razor
@model List<JKTurnos.Models.Entities.Cliente>

@helper ListContent() {
    // HTML de la tabla (desktop) y cards (mobile)
    // Por cada fila: @Html.Partial("_TableActions.cshtml", ...)
}

@helper EmptyContent() {
    @Html.Partial("~/Views/Shared/Components/_EmptyState.cshtml", ...)
}

@{
    ViewBag.Title = "Clientes";
    ViewBag.Subtitle = "Administra tus clientes";
    ViewBag.PrimaryAction = Url.Action("Create");
    ViewBag.PageContent = Model.Any() ? ListContent() : EmptyContent();
}

@Html.Partial("~/Views/Shared/Components/_CrudPage.cshtml")
```

La vista realiza tres acciones:

1. Define helpers que construyen el HTML especifico del modulo
2. Asigna valores a `ViewBag` que seran leidos por los componentes
3. Llama a un partial que renderiza toda la pagina

### 2.4 Helpers

Los helpers (`@helper Nombre() { ... }`) son funciones Razor que devuelven HTML.

Se usan para encapsular bloques de interfaz dentro de la misma vista.

No son reutilizables entre modulos, por eso viven en la vista y no en un archivo separado.

En el ejemplo:

- `ListContent()` construye la tabla desktop + cards mobile con el listado
- `EmptyContent()` construye el estado vacio cuando no hay datos

### 2.5 ViewBag

`ViewBag` es el mecanismo de comunicacion entre la vista y los componentes.

La vista asigna valores:

```csharp
ViewBag.PageContent = ...;  // Slot principal
ViewBag.Title = "Clientes";
ViewBag.Subtitle = "Administra tus clientes";
ViewBag.PrimaryAction = Url.Action("Create");
ViewBag.PrimaryActionLabel = "Nuevo Cliente";
ViewBag.SearchPlaceholder = "Buscar clientes...";
ViewBag.SearchId = "search-clientes";
```

El componente lee estos valores y los usa para renderizarse.

### 2.6 Partial Views

`@Html.Partial("~/Views/Shared/Components/_CrudPage.cshtml")` renderiza el componente.

El componente `_CrudPage.cshtml` a su vez renderiza otros componentes:

1. `_PageHeader.cshtml` (lee ViewBag.Title, ViewBag.Subtitle, ViewBag.PrimaryAction)
2. `_CrudToolbar.cshtml` (lee ViewBag.SearchPlaceholder, ViewBag.SearchId, que internamente llama a `_SearchBar.cshtml`)
3. `ViewBag.PageContent` (el slot con el contenido especifico del modulo)

### 2.7 HTML Final

El resultado es HTML completo compuesto por:

```
Layout
  Sidebar
  Topbar
  Content Area
    PageHeader (titulo + accion primaria)
    CrudToolbar (search bar)
    PageContent (tabla o empty state)
      TableActions (por fila: editar/eliminar)
```

---

# 3. Conceptos del proyecto

## 3.1 Controller

### Que es

Clase en `Controllers/` que hereda de `BaseController`.

Responde a URLs y coordina el flujo entre el usuario y el sistema.

### Para que sirve

Recibir requests, validar ModelState, llamar Services, retornar Views o redirects.

### Cuando se usa

Siempre que exista una URL que el usuario pueda visitar.

### Cuando NO se usa

Nunca contiene logica de negocio.

Nunca accede directamente a la base de datos.

### Ejemplo

`Controllers/ClientesController.cs`:

```csharp
public class ClientesController : BaseController
{
    private readonly ClienteService _service;

    public ClientesController()
    {
        _service = new ClienteService();
    }

    public ActionResult Index()
    {
        var clientes = _service.GetAll();
        return View(clientes);
    }
}
```

---

## 3.2 Model

### Que es

Clase en `Models/Entities/` que representa una tabla de la base de datos.

Hereda de `BaseEntity` e implementa `ITenantEntity`.

### Para que sirve

Define la estructura de los datos que el sistema persiste y manipula.

### Cuando se usa

Como parametro en Controllers.

Como `@model` en Vistas.

Como tipo generico en `BaseRepository<T>`.

### Cuando NO se usa

No contiene logica de negocio.

No contiene logica de presentacion.

### Ejemplo

`Models/Entities/Cliente.cs`:

```csharp
public class Cliente : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    public string Nombre { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public string Notas { get; set; }
}
```

---

## 3.3 View

### Que es

Archivo `.cshtml` en `Views/{Controller}/` que renderiza el HTML de una accion.

### Para que sirve

Transformar datos del Model en interfaz de usuario.

### Cuando se usa

Como respuesta a una accion del Controller que retorna `View()`.

### Cuando NO se usa

No contiene logica de negocio.

No llama Services ni Repositories.

### Ejemplo

`Views/Clientes/Index.cshtml`:

```razor
@model List<Cliente>
@helper ListContent() { /* HTML del listado */ }
@helper EmptyContent() { /* HTML del estado vacio */ }
@{
    ViewBag.Title = "Clientes";
    ViewBag.PageContent = Model.Any() ? ListContent() : EmptyContent();
}
@Html.Partial("~/Views/Shared/Components/_CrudPage.cshtml")
```

---

## 3.4 Partial View

### Que es

Archivo `.cshtml` en `Views/Shared/Components/` que comienza con `_`.

Representa un componente reutilizable del Design System.

### Para que sirve

Evitar duplicar HTML.

Centralizar la interfaz de componentes que se usan en multiples modulos.

### Cuando se usa

Cuando un patron de interfaz aparece en dos o mas modulos.

### Cuando NO se usa

Cuando el contenido es especifico de un solo modulo (en ese caso va en la vista o en un helper).

No se usa para maquetacion general (esa va en el Layout).

### Ejemplo

`Views/Shared/Components/_SearchBar.cshtml`:

```razor
@{
    string placeholder = ViewBag.Placeholder ?? "Buscar...";
    string id = ViewBag.Id ?? "jk-search";
}
<div class="jk-search">
    <i class="bi bi-search"></i>
    <input type="text" id="@id" placeholder="@placeholder" />
</div>
```

---

## 3.5 Helper

### Que es

Funcion definida con `@helper Nombre() { }` dentro de una vista Razor.

### Para que sirve

Encapsular bloques de HTML especificos del modulo dentro de la misma vista.

### Cuando se usa

Para construir el contenido que se asigna a `ViewBag.PageContent` o `ViewBag.FormContent`.

### Cuando NO se usa

No es reutilizable entre modulos (si se necesita reutilizar, debe ser un Partial).

No reemplaza a los componentes del Design System.

### Ejemplo

En `Views/Clientes/Index.cshtml`:

```razor
@helper ListContent() {
    <div class="jk-table-wrap">
        <table class="jk-table">
            <thead>
                <tr>
                    <th>Nombre</th>
                    <th>Telefono</th>
                    <th>Email</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in Model)
                {
                    <tr>
                        <td>@item.Nombre</td>
                        <td>@item.Telefono</td>
                        <td>@item.Email</td>
                        <td>
                            @Html.Partial("_TableActions.cshtml",
                                new ViewDataDictionary {
                                    { "EditUrl", Url.Action("Edit", new { id = item.Id }) },
                                    { "DeleteUrl", Url.Action("Delete", new { id = item.Id }) },
                                    { "ItemName", item.Nombre }
                                })
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
```

---

## 3.6 ViewBag

### Que es

Objeto dinamico de ASP.NET MVC que permite pasar datos desde el Controller o la Vista hacia los componentes.

### Para que sirve

Comunicar valores entre la vista del modulo y los partials del Design System.

### Cuando se usa

Para enviar titulos, subtitulos, URLs de acciones, textos de botones, y el contenido de los slots (`PageContent`, `FormContent`).

### Cuando NO se usa

No se usa para pasar datos de negocio (esos van en el `@model`).

No se usa para pasar datos entre Controller y Vista si ya existe un Model.

### Ejemplo tipico

```csharp
@{
    ViewBag.Title = "Clientes";
    ViewBag.Subtitle = "Administra tus clientes";
    ViewBag.PrimaryAction = Url.Action("Create");
    ViewBag.PrimaryActionLabel = "Nuevo Cliente";
    ViewBag.PageContent = ListContent();
}
```

---

## 3.7 ViewData

### Que es

Diccionario (`ViewDataDictionary`) que acompaña a `ViewBag`.

`ViewBag` es un wrapper dinamico sobre `ViewData`.

### Para que sirve

Pasar datos a los partials cuando se invocan con `@Html.Partial`.

### Cuando se usa

Cuando se llama a un componente pasandole parametros especificos.

### Cuando NO se usa

No se usa directamente en la vista del modulo (se prefiere `ViewBag` por su sintaxis dinamica).

### Ejemplo

```razor
@Html.Partial("_TableActions.cshtml",
    new ViewDataDictionary {
        { "EditUrl", Url.Action("Edit", new { id = item.Id }) },
        { "DeleteUrl", Url.Action("Delete", new { id = item.Id }) },
        { "ItemName", item.Nombre },
        { "Compact", false }
    })
```

---

## 3.8 ViewDataDictionary

### Que es

Clase que implementa `IDictionary<string, object>`.

Se usa para construir el diccionario de datos que recibe un partial.

### Para que sirve

Permite pasar multiples parametros tipados a un componente sin necesidad de crear una clase ViewModel especifica.

### Cuando se usa

En la invocacion de `@Html.Partial()` cuando el componente necesita parametros.

### Cuando NO se usa

No se usa como alternativa a un ViewModel cuando hay logica compleja.

### Ejemplo

```razor
new ViewDataDictionary {
    { "SubmitText", "Guardar" },
    { "CancelUrl", Url.Action("Index") }
}
```

---

## 3.9 Url.Action

### Que es

Metodo de ASP.NET MVC que genera una URL relativa a partir de una accion y controlador.

### Para que sirve

Evitar URLs hardcodeadas en las vistas.

### Cuando se usa

Para generar enlaces a acciones del sistema.

### Cuando NO se usa

No se usa para URLs externas.

### Ejemplo

```razor
@Url.Action("Edit", new { id = item.Id })
// Resultado: /Clientes/Edit/5

@Url.Action("Create")
// Resultado: /Clientes/Create

@Url.Action("Index")
// Resultado: /Clientes
```

---

## 3.10 Html.Partial

### Que es

Metodo que renderiza un partial view y retorna el HTML resultante.

### Para que sirve

Insertar un componente del Design System dentro de una vista.

### Cuando se usa

Siempre que se necesita un componente reutilizable.

La ruta debe ser completa: `"~/Views/Shared/Components/_Componente.cshtml"`.

### Cuando NO se usa

No se usa para incluir vistas de otros modulos.

No se usa para incluir el Layout (eso se configura en `_ViewStart.cshtml`).

### Ejemplo

```razor
@Html.Partial("~/Views/Shared/Components/_PageHeader.cshtml")

@Html.Partial("~/Views/Shared/Components/_FormActions.cshtml",
    new ViewDataDictionary {
        { "SubmitText", "Guardar" },
        { "CancelUrl", Url.Action("Index") }
    })
```

---

## 3.11 Layout

### Que es

Archivo `Views/Shared/_Layout.cshtml` que define la estructura HTML comun de todas las paginas.

### Para que sirve

Define el AppShell: Sidebar, Topbar, Content Area, Overlay, capas de Drawer y Toast.

### Cuando se usa

Se aplica automaticamente a todas las vistas mediante `_ViewStart.cshtml`.

### Cuando NO se usa

No se modifica por modulo (es global).

### Estructura actual

```
jk-app-shell
  #sidebar (jk-sidebar)
  #sidebar-overlay
  #main-content
    #topbar (jk-topbar)
    main.content-area (jk-content-zone)
      @RenderBody()
```

El Layout carga:

- 5 archivos CSS del Design System v2
- 3 archivos CSS del Design System v1 (para migracion progresiva)
- Google Fonts (Inter)
- Bootstrap Icons
- jQuery, Bootstrap JS, jk-layout.js

---

## 3.12 Design System

### Que es

Conjunto de archivos CSS en `Content/DesignSystem/` que definen la identidad visual completa del sistema.

### Para que sirve

Mantener consistencia visual en todas las pantallas.

Eliminar la necesidad de escribir estilos por modulo.

### Cuando se usa

Toda clase CSS en las vistas debe pertenecer al DS.

Nunca se usan valores hardcodeados.

### Archivos

| Archivo | Contenido |
|---------|-----------|
| `jk-tokens.css` | Variables CSS: colores, tipografia, espaciado, sombras, z-index |
| `jk-base.css` | Reset, tipografia, animaciones, skeleton loading |
| `jk-layout.css` | AppShell, Sidebar responsive, Topbar, Content grid |
| `jk-components.css` | Todos los componentes UI: botones, cards, inputs, tablas, etc. |
| `jk-utilities.css` | Helpers: display, flex, spacing, width, responsive, accesibilidad |

### Valores principales

Actualizado 2026-08-25 (D-035/D-036): paleta calida con superficies menta palida. Titulos usan `--jk-font-display` (Nunito); cuerpo `--jk-font-sans` (Inter). Tokens decorativos con consumidores reales: `--jk-coral #E88A6F`, `--jk-butter #E6C868`, `--jk-teal #5FA8AD` (+ `-light`) y `--jk-gradient-tricolor`.

| Token | Valor |
|-------|-------|
| `--jk-bg` | `#F6F1E8` |
| `--jk-surface` | `#F1FAFA` |
| `--jk-sidebar-bg` | `#D7F0EF` |
| `--jk-accent` | `#5F7A5D` |
| `--jk-text-primary` | `#4A4038` |
| `--jk-text-secondary` | `#7B6F65` |
| `--jk-border` | `#E4D8C8` |
| `--jk-success` | `#759572` |
| `--jk-warning` | `#D9A441` |
| `--jk-danger` | `#BE5B50` |
| `--jk-radius` | `14px` |

Paleta VINTAGE del selector de recursos (`_ColorSelector.cshtml`): Azul `#7295B5`, Verde `#9CAF88`, Rojo `#C97B6D`, Naranja `#E8A87C`, Amarillo `#E6C868`, Violeta `#A594C5`, Rosa `#D9A5B3`, Turquesa `#6FB3B8`, Gris `#A89F91`, Marron `#B08968`, Indigo `#8784B8`, Cian `#92C7AF`.

---

## 3.13 Componente

### Que es

Partial view en `Views/Shared/Components/_Nombre.cshtml` que encapsula un patron de interfaz reutilizable.

### Para que sirve

Estandarizar la interfaz y evitar duplicacion de HTML.

### Cuando se usa

Cuando el mismo patron aparece en dos o mas modulos.

### Cuando NO se usa

No se crea un componente antes de que exista una necesidad real.

No se crean componentes que encapsulan logica especifica de un modulo.

### Componentes existentes (16)

| Componente | Funcion |
|------------|---------|
| `_PageHeader.cshtml` | Titulo + subtitulo + accion primaria |
| `_SectionHeader.cshtml` | Titulo de seccion + link opcional |
| `_CrudPage.cshtml` | Wrapper completo para pagina de listado |
| `_CrudToolbar.cshtml` | Barra de busqueda para listados |
| `_SearchBar.cshtml` | Input de busqueda con icono |
| `_TableActions.cshtml` | Acciones editar/eliminar por fila |
| `_FormLayout.cshtml` | Wrapper para pagina de formulario |
| `_FormActions.cshtml` | Botones submit/cancelar |
| `_EmptyState.cshtml` | Estado vacio con icono + mensaje + accion |
| `_ActionCard.cshtml` | Card de accion rapida con icono |
| `_KpiCard.cshtml` | Indicador con label, valor y tendencia |
| `_Timeline.cshtml` | Linea de tiempo con eventos |
| `_Avatar.cshtml` | Avatar circular con inicial |
| `_Drawer.cshtml` | Panel lateral reutilizable |
| `_Toast.cshtml` | Contenedor de notificaciones |
| `_FloatingButton.cshtml` | FAB para mobile |

---

## 3.14 CRUD Page

### Que es

Componente `_CrudPage.cshtml` que orquesta la estructura completa de una pagina de listado.

### Para que sirve

Estandarizar todas las paginas Index del sistema.

Componer PageHeader + CrudToolbar + Content slot en un solo llamado.

### Cuando se usa

En toda vista Index de modulos CRUD.

### Cuando NO se usa

No se usa en paginas que no son listados (Home, Configuracion, etc.).

### Funcionamiento

```razor
@* _CrudPage.cshtml *@
@{
    var pageContent = ViewBag.PageContent;
}

@Html.Partial("_PageHeader.cshtml")
@Html.Partial("_CrudToolbar.cshtml")

@if (pageContent != null)
{
    <div class="jk-mt-24">
        @pageContent
    </div>
}
```

### Datos que recibe via ViewBag

| Propiedad | Tipo | Uso |
|-----------|------|-----|
| `ViewBag.Title` | string | Titulo de la pagina |
| `ViewBag.Subtitle` | string | Subtitulo |
| `ViewBag.PrimaryAction` | string | URL de la accion principal |
| `ViewBag.PrimaryActionLabel` | string | Texto del boton principal |
| `ViewBag.SearchPlaceholder` | string | Placeholder del buscador |
| `ViewBag.SearchId` | string | ID del input de busqueda |
| `ViewBag.PageContent` | IHtmlString | Contenido del slot principal |

---

## 3.15 Slot

### Que es

Mecanismo por el cual una vista inyecta contenido especifico dentro de un componente generico.

### Para que sirve

Permitir que un componente contenedor (`_CrudPage`, `_FormLayout`) renderice HTML variable sin conocer su contenido.

### Cuando se usa

Cuando un componente define una estructura pero el contenido depende del modulo.

### Implementacion

El componente declara una variable que lee de ViewBag:

```razor
@{
    var pageContent = ViewBag.PageContent;
}
@if (pageContent != null)
{
    @pageContent
}
```

La vista asigna el contenido:

```razor
@{
    ViewBag.PageContent = Model.Any() ? ListContent() : EmptyContent();
}
```

---

## 3.16 PageContent

### Que es

Slot principal del componente `_CrudPage.cshtml`.

Contiene el HTML especifico del listado (tabla desktop + cards mobile) o el estado vacio.

### Para que sirve

Inyectar el contenido variable dentro de la estructura fija del CRUD Page.

### Cuando se usa

En toda vista Index que utiliza `_CrudPage`.

### Asignacion tipica

```razor
@{
    ViewBag.PageContent = Model.Any() ? ListContent() : EmptyContent();
}
```

---

## 3.17 EmptyState

### Que es

Componente `_EmptyState.cshtml` que muestra un mensaje amigable cuando no hay datos.

### Para que sirve

Evitar pantallas vacias.

Siempre invitar al usuario a realizar una accion.

### Datos que recibe via ViewDataDictionary

| Propiedad | Ejemplo |
|-----------|---------|
| `Icon` | `"people"` |
| `Title` | `"No tienes clientes"` |
| `Description` | `"Crea tu primer cliente para comenzar."` |
| `ActionText` | `"Crear Cliente"` |
| `ActionUrl` | `Url.Action("Create")` |

### Ejemplo de uso

```razor
@Html.Partial("~/Views/Shared/Components/_EmptyState.cshtml",
    new ViewDataDictionary {
        { "Icon", "people" },
        { "Title", "No tienes clientes" },
        { "Description", "Crea tu primer cliente para comenzar." },
        { "ActionText", "Crear Cliente" },
        { "ActionUrl", Url.Action("Create") }
    })
```

---

## 3.18 TableActions

### Que es

Componente `_TableActions.cshtml` que renderiza los botones de Editar y Eliminar para cada fila de un listado.

### Para que sirve

Estandarizar las acciones por registro en todos los modulos.

### Datos que recibe via ViewDataDictionary

| Propiedad | Tipo | Descripcion |
|-----------|------|-------------|
| `EditUrl` | string | URL de la accion Edit |
| `DeleteUrl` | string | URL de la accion Delete |
| `ItemName` | string | Nombre del elemento (para aria-label) |
| `Compact` | bool | Si es true, oculta el texto (solo iconos) |

### Modos

- `Compact = false`: muestra icono + texto "Editar" / "Eliminar" (desktop)
- `Compact = true`: muestra solo iconos (mobile)

---

## 3.19 FormLayout

### Que es

Componente `_FormLayout.cshtml` que orquesta la estructura completa de una pagina de formulario.

### Para que sirve

Estandarizar todas las paginas Create y Edit del sistema.

Componer PageHeader + card con form + anti-forgery + form actions.

### Cuando se usa

En toda vista Create y Edit de modulos CRUD.

### Funcionamiento

```razor
@Html.Partial("_PageHeader.cshtml")

<div class="jk-mt-24">
    <div class="row">
        <div class="col-md-8 col-lg-6">
            <div class="jk-card">
                @using (Html.BeginForm())
                {
                    @Html.AntiForgeryToken()

                    if (formContent != null)
                    {
                        @formContent
                    }

                    @Html.Partial("_FormActions.cshtml",
                        new ViewDataDictionary {
                            { "SubmitText", submitText },
                            { "CancelUrl", cancelUrl }
                        })
                }
            </div>
        </div>
    </div>
</div>
```

### Datos que recibe via ViewBag

| Propiedad | Descripcion |
|-----------|-------------|
| `ViewBag.Title` | Titulo de la pagina |
| `ViewBag.Subtitle` | Subtitulo |
| `ViewBag.SubmitText` | Texto del boton de envio |
| `ViewBag.CancelUrl` | URL de cancelar (volver al Index) |
| `ViewBag.FormContent` | Slot con los campos del formulario |

---

## 3.20 FormActions

### Que es

Componente `_FormActions.cshtml` que renderiza los botones de submit y cancelar.

### Para que sirve

Estandarizar la barra de acciones en todos los formularios.

### Datos que recibe via ViewDataDictionary

| Propiedad | Descripcion |
|-----------|-------------|
| `SubmitText` | Texto del boton de guardar |
| `CancelUrl` | URL del boton de cancelar |

---

# 4. Flujo de los componentes

## 4.1 Clientes/Index

```
Clientes/Index.cshtml
    |
    | define @helper ListContent()    (construye la tabla/cards + TableActions por fila)
    | define @helper EmptyContent()   (construye EmptyState con llamada a crear)
    | asigna ViewBag.PageContent segun corresponda
    |
    v
_CrudPage.cshtml
    |
    |--- _PageHeader.cshtml
    |       recibe: ViewBag.Title, ViewBag.Subtitle, ViewBag.PrimaryAction, ViewBag.PrimaryActionLabel
    |       renderiza: h1 + subtitle + boton primario
    |
    |--- _CrudToolbar.cshtml
    |       |
    |       |--- _SearchBar.cshtml
    |       |       recibe: ViewBag.SearchPlaceholder, ViewBag.SearchId
    |       |       renderiza: input con icono de busqueda
    |
    |--- PageContent (slot)
            |
            |-- [si hay datos] ListContent()
            |       |
            |       |-- Tabla (desktop) con filas
            |       |       por cada fila: _TableActions.cshtml
            |       |           recibe: EditUrl, DeleteUrl, ItemName, Compact=false
            |       |
            |       |-- Cards (mobile) con datos
            |               por cada card: _TableActions.cshtml
            |                   recibe: EditUrl, DeleteUrl, ItemName, Compact=true
            |
            |-- [si no hay datos] EmptyContent()
                    |
                    |-- _EmptyState.cshtml
                            recibe: Icon, Title, Description, ActionText, ActionUrl
                            renderiza: icono + mensaje + boton de accion
```

## 4.2 Clientes/Create

```
Clientes/Create.cshtml
    |
    | define @helper FormContent()    (construye los campos del formulario)
    | asigna ViewBag.FormContent
    |
    v
_FormLayout.cshtml
    |
    |--- _PageHeader.cshtml
    |       recibe: ViewBag.Title, ViewBag.Subtitle
    |
    |--- Card con formulario
    |       |
    |       |--- @Html.BeginForm() + @Html.AntiForgeryToken()
    |       |
    |       |--- FormContent (slot)
    |       |       campos del formulario especificos del modulo
    |       |
    |       |--- _FormActions.cshtml
    |               recibe: ViewBag.SubmitText, ViewBag.CancelUrl
    |               renderiza: boton submit + boton cancelar
```

## 4.3 Clientes/Edit

Flujo identico a Create, con la diferencia de que el helper `FormContent()` incluye `@Html.HiddenFor` para:

- `Id`
- `CreatedAt`
- `TenantId`
- `IsActive`

Estos campos ocultos son necesarios para que EF no intente actualizar `CreatedAt` con `DateTime.MinValue`, lo que causaria un error de rango en SQL Server.

---

# 5. Reglas de construccion de vistas

## 5.1 Responsabilidades

- El Controller nunca construye HTML
- Las vistas reciben un `@model` con los datos de negocio
- Los Helpers (`@helper`) construyen bloques reutilizables dentro de la vista
- Los Partials (`_Nombre.cshtml`) representan componentes reutilizables del Design System
- Los componentes reciben informacion mediante `ViewBag` o `ViewDataDictionary`
- El HTML especifico del modulo queda en la vista del modulo
- El HTML reutilizable pasa al Design System (`Views/Shared/Components/`)

## 5.2 Estructura de una vista Index

```
@model List<Entidad>

@helper ListContent() { }
@helper EmptyContent() { }

@{
    ViewBag.Title = "...";
    ViewBag.Subtitle = "...";
    ViewBag.PrimaryAction = Url.Action("Create");
    ViewBag.PrimaryActionLabel = "...";
    ViewBag.SearchPlaceholder = "...";
    ViewBag.SearchId = "...";
    ViewBag.PageContent = Model.Any() ? ListContent() : EmptyContent();
}

@Html.Partial("~/Views/Shared/Components/_CrudPage.cshtml")
```

## 5.3 Estructura de una vista Create

```
@model Entidad

@helper FormContent() {
    <div class="jk-form-group">
        @Html.LabelFor(...)
        @Html.TextBoxFor(...)
        @Html.ValidationMessageFor(...)
    </div>
    ...
}

@{
    ViewBag.Title = "Nuevo ...";
    ViewBag.Subtitle = "...";
    ViewBag.SubmitText = "Guardar";
    ViewBag.CancelUrl = Url.Action("Index");
    ViewBag.FormContent = FormContent();
}

@Html.Partial("~/Views/Shared/Components/_FormLayout.cshtml")
```

## 5.4 Estructura de una vista Edit

```
@model Entidad

@helper FormContent() {
    @Html.HiddenFor(m => m.Id)
    @Html.HiddenFor(m => m.CreatedAt)
    @Html.HiddenFor(m => m.TenantId)
    @Html.HiddenFor(m => m.IsActive)

    <div class="jk-form-group">
        @Html.LabelFor(...)
        @Html.TextBoxFor(...)
        @Html.ValidationMessageFor(...)
    </div>
    ...
}

@{
    ViewBag.Title = "Editar ...";
    ViewBag.Subtitle = "...";
    ViewBag.SubmitText = "Actualizar";
    ViewBag.CancelUrl = Url.Action("Index");
    ViewBag.FormContent = FormContent();
}

@Html.Partial("~/Views/Shared/Components/_FormLayout.cshtml")
```

## 5.5 Regla de los HiddenFor en Edit

Toda vista Edit debe incluir:

```razor
@Html.HiddenFor(m => m.Id)
@Html.HiddenFor(m => m.CreatedAt)
@Html.HiddenFor(m => m.TenantId)
@Html.HiddenFor(m => m.IsActive)
```

Sin estos campos, al hacer POST, `CreatedAt` se envia como `DateTime.MinValue` y SQL Server rechaza el valor por estar fuera del rango de `datetime`.

## 5.6 Regla de textos sin acentos

Todos los textos visibles en las vistas deben escribirse SIN ACENTOS.

Motivo: compatibilidad de encoding.

Ejemplo:

- `"Administracion"` en vez de `"Administración"`
- `"Telefono"` en vez de `"Teléfono"`
- `"Duracion"` en vez de `"Duración"`

---

# 6. Reglas para crear nuevos modulos CRUD

## 6.1 Proceso oficial

```
1. Crear la entidad en Models/Entities/
2. Agregar DbSet en Data/JKContext.cs
3. Crear el Service en Services/
4. Crear el Controller en Controllers/
5. Crear la vista Index usando _CrudPage
6. Crear la vista Create usando _FormLayout
7. Crear la vista Edit usando _FormLayout
8. Crear la vista Delete (sigue siendo manual, sin componente)
9. Agregar enlace en _Sidebar.cshtml
```

## 6.2 Entidad

Heredar de `BaseEntity` e implementar `ITenantEntity`.

```csharp
public class MiEntidad : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
    [Required]
    public string Nombre { get; set; }
    // ... demas propiedades
}
```

## 6.3 DbSet

En `Data/JKContext.cs`:

```csharp
public DbSet<MiEntidad> MiEntidad { get; set; }
```

## 6.4 Service

En `Services/MiEntidadService.cs`:

```csharp
public class MiEntidadService
{
    private readonly BaseRepository<MiEntidad> _repo;

    public MiEntidadService()
    {
        _repo = new BaseRepository<MiEntidad>();
    }

    public List<MiEntidad> GetAll()
    {
        return _repo.GetAll().OrderBy(x => x.Nombre).ToList();
    }

    public MiEntidad GetById(int id)
    {
        return _repo.GetById(id);
    }

    public void Create(MiEntidad model)
    {
        _repo.Add(model);
    }

    public void Update(MiEntidad model)
    {
        _repo.Update(model);
    }

    public void Delete(int id)
    {
        _repo.Delete(id);
    }
}
```

## 6.5 Controller

En `Controllers/MiEntidadController.cs`:

```csharp
[Authorize]
public class MiEntidadController : BaseController
{
    private readonly MiEntidadService _service;

    public MiEntidadController()
    {
        _service = new MiEntidadService();
    }

    public ActionResult Index()
    {
        return View(_service.GetAll());
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(MiEntidad model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _service.Create(model);
        return RedirectToAction("Index");
    }

    public ActionResult Edit(int id)
    {
        var entity = _service.GetById(id);
        if (entity == null) return HttpNotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(MiEntidad model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _service.Update(model);
        return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
        var entity = _service.GetById(id);
        if (entity == null) return HttpNotFound();
        return View(entity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        _service.Delete(id);
        return RedirectToAction("Index");
    }
}
```

## 6.6 Vista Index

Seguir la estructura estandar con helpers `ListContent()` y `EmptyContent()`, asignando `ViewBag.PageContent` y llamando a `_CrudPage`.

## 6.7 Vista Create

Seguir la estructura estandar con helper `FormContent()`, asignando `ViewBag.FormContent` y llamando a `_FormLayout`.

## 6.8 Vista Edit

Igual a Create, pero incluyendo los 4 `@Html.HiddenFor` obligatorios.

## 6.9 Vista Delete

No tiene componente wrappe. Se escribe manualmente con `@Html.BeginForm("DeleteConfirmed")` + `@Html.AntiForgeryToken()` + `@Html.HiddenFor(m => m.Id)`.

## 6.10 Sidebar

Agregar enlace en `Views/Shared/_Sidebar.cshtml` dentro del grupo correspondiente:

```razor
<a href="@Url.Action("Index", "MiEntidad")"
   class="jk-nav-item @(ViewContext.RouteData.Values["controller"].ToString() == "MiEntidad" ? "active" : "")"
   data-sidebar-item>
    <i class="bi bi-icon"></i>
    <span>Nombre en Menu</span>
</a>
```

---

# 7. Convenciones oficiales del proyecto

## 7.1 Nombres de archivos

- Los partials comienzan con `_`
- Los controllers usan el sufijo `Controller`
- Los services usan el sufijo `Service`
- Los repositories son genericos (`BaseRepository<T>`)

## 7.2 Ubicacion de archivos

| Que | Donde |
|-----|-------|
| Componentes del DS | `Views/Shared/Components/_Nombre.cshtml` |
| CSS del DS | `Content/DesignSystem/jk-*.css` |
| Estilos especificos de modulo | `Content/Pages/jk-{modulo}.css` |
| Entities | `Models/Entities/` |
| ViewModels | `Models/ViewModels/` |
| Services | `Services/` |
| Repositories | `Data/` |
| Infrastructure | `Infrastructure/` |

## 7.3 No duplicar HTML

Si el mismo patron aparece en dos o mas vistas, se extrae a un componente en `Views/Shared/Components/`.

## 7.4 No crear componentes antes de tiempo

No se crea un componente hasta que exista una necesidad real detectada por duplicacion.

La excepcion son los componentes del Design System base (PageHeader, EmptyState, etc.) que se crearon como parte de la migracion del DS.

## 7.5 Reutilizar siempre el Design System

No se escriben estilos CSS nuevos sin antes verificar si existe una clase del DS que resuelva el problema.

## 7.6 Textos sin acentos

Todos los strings visibles se escriben sin acentos.

## 7.7 No hardcodear valores CSS

Siempre usar variables CSS: `var(--jk-text)`, `var(--jk-accent)`, etc.

## 7.8 Flujo de datos

```
Controller -> Model -> View -> Helpers -> ViewBag -> Partials -> HTML
```

Nunca se rompe este flujo.

## 7.9 Seguridad

- Toda pagina protegida usa `[Authorize]` (excepto Login)
- Todo formulario POST usa `[ValidateAntiForgeryToken]`
- Todo formulario incluye `@Html.AntiForgeryToken()`
- Las entidades implementan `ITenantEntity` y `BaseRepository` filtra automaticamente por TenantId

---

# 8. Glosario

### A

**ActionCard**
Componente del DS. Card con icono, titulo, descripcion y URL. Se usa para acciones rapidas en el Home.

**AntiForgeryToken**
Mecanismo de seguridad contra CSRF. Obligatorio en todo formulario POST.

**AppShell**
Estructura general del Layout compuesta por Sidebar + Topbar + Content Area + Overlay + capas de Drawer/Toast/Modal.

**Avatar**
Componente del DS. Circulo con la inicial del nombre del usuario.

### B

**BaseController**
Clase base que todos los controllers heredan. Expone `TenantId` y `UserId` desde Claims.

**BaseRepository**
Repositorio generico que implementa CRUD completo con filtrado automatico por TenantId e IsActive.

**BundleConfig**
Configuracion de ASP.NET Bundling para CSS y JS.

### C

**Claims**
Datos de identidad almacenados en la cookie: NameIdentifier, Name, Email, Role, TenantId.

**Command Center**
Filosofia del Home. No es un dashboard de estadisticas; debe responder "Que debo hacer ahora".

**Componente**
Partial view reutilizable en `Views/Shared/Components/`. Representa un patron de interfaz del DS.

**Controller**
Clase que recibe requests, coordina Services y retorna Views.

**CRUD Page**
Componente `_CrudPage.cshtml` que wrappe la estructura completa de una pagina de listado.

**CrudToolbar**
Componente `_CrudToolbar.cshtml` que renderiza la barra de busqueda en paginas Index.

### D

**Design System**
Conjunto de 5 archivos CSS en `Content/DesignSystem/` que definen la identidad visual completa.

**Drawer**
Componente del DS. Panel lateral que se abre para edicion o detalle sin navegar a otra pagina.

**DRY**
Principio: no repetir codigo. El motor detras de la creacion de componentes reutilizables.

### E

**EmptyState**
Componente del DS. Mensaje amigable con icono, descripcion y accion para cuando no hay datos.

**Entity**
Clase en `Models/Entities/` que representa una tabla de la base de datos.

### F

**FAB**
Floating Action Button. Componente del DS visible solo en mobile.

**FloatingButton**
Componente `_FloatingButton.cshtml`. Boton flotante para acciones rapidas en mobile.

**FormActions**
Componente `_FormActions.cshtml`. Botones submit + cancelar para formularios.

**FormContent**
Slot que contiene los campos del formulario, inyectado en `_FormLayout.cshtml` via `ViewBag.FormContent`.

**FormLayout**
Componente `_FormLayout.cshtml` que wrappe la estructura completa de una pagina de formulario.

### H

**Helper**
Funcion `@helper` definida dentro de una vista Razor. Encapsula HTML especifico del modulo.

**HiddenFor**
Campos ocultos obligatorios en Edit: Id, CreatedAt, TenantId, IsActive.

### I

**ITenantEntity**
Interface que exige la propiedad `TenantId`. Toda entidad del negocio la implementa.

### J

**JKContext**
DbContext principal del sistema. Contiene todos los DbSet.

### K

**KPI Card**
Componente del DS. Indicador visual con label, valor y tendencia opcional.

### L

**Layout**
`Views/Shared/_Layout.cshtml`. Define la estructura HTML comun (AppShell).

**ListContent**
Helper tipico en vistas Index. Construye la tabla desktop y cards mobile del listado.

### M

**Model**
Entidad de negocio o ViewModel que la vista recibe como `@model`.

**Multi Tenant**
Arquitectura donde cada entidad pertenece a un Tenant. Aislamiento total entre negocios.

### O

**OWIN**
Middleware de autenticacion utilizado. Reemplaza a ASP.NET Identity. Usa Cookie Authentication.

### P

**PageContent**
Slot principal de `_CrudPage.cshtml`. Contiene el listado o el estado vacio.

**PageHeader**
Componente del DS. Titulo + subtitulo + boton de accion primaria.

**Partial View**
Archivo `.cshtml` que comienza con `_`. Componente reutilizable del DS.

**PasswordHasher**
Clase en `Infrastructure/` que wrappe BCrypt para hashing y verificacion de contrasenas.

### R

**Repository**
Clase en `Data/` que abstrae el acceso a datos. `BaseRepository<T>` es el unico repository del sistema.

**RouteConfig**
Configuracion de rutas MVC. Ruta por defecto: `{controller}/{action}/{id}`.

### S

**SearchBar**
Componente `_SearchBar.cshtml`. Input de busqueda con icono de lupa.

**SectionHeader**
Componente del DS. Titulo de seccion con link opcional de "Ver todo".

**Service**
Clase en `Services/` que contiene la logica de negocio. Se instancia manualmente en los Controllers.

**Sidebar**
Menu de navegacion principal. Responsive: fija en desktop, colapsable en tablet, drawer en mobile.

**Slot**
Mecanismo para inyectar HTML variable (PageContent, FormContent) dentro de un componente contenedor.

**Soft Delete**
Eliminacion logica: en vez de borrar registros, se marca `IsActive = false`.

### T

**TableActions**
Componente `_TableActions.cshtml`. Botones de editar y eliminar por fila.

**Tenant**
Entidad que representa un negocio/cliente del SaaS. Aisla todos los datos.

**TenantId**
Claim que identifica al Tenant actual. Se obtiene via `TenantProvider` desde la cookie.

**TenantProvider**
Clase en `Infrastructure/` que obtiene el TenantId desde los Claims del usuario autenticado.

**Timeline**
Componente del DS. Linea de tiempo vertical con eventos, usado para actividad reciente.

**Toast**
Componente del DS. Contenedor de notificaciones emergentes.

**Topbar**
Barra superior con menu hamburguesa (mobile), titulo, notificaciones y perfil.

### U

**Url.Action**
Metodo que genera URLs relativas a partir de accion/controlador.

### V

**View**
Archivo `.cshtml` que renderiza la interfaz de una accion del Controller.

**ViewBag**
Objeto dinamico para pasar datos de la vista a los componentes.

**ViewData**
Diccionario subyacente de ViewBag. Se usa directamente al pasar parametros a partials via `new ViewDataDictionary`.

**ViewDataDictionary**
Clase usada para construir los parametros que recibe un partial.

**ViewModel**
Clase en `Models/ViewModels/` con datos especificos para una vista. En JKTurnos solo existe `LoginVM`.

### W

**Web.config**
Archivo de configuracion con connection string a SQL Server Express.
