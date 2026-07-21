# CONTEXTO DE MIGRACION - Design System v2

Fecha: 19/7/2026
Estado: Fase 2 completada. Listo para Fase 3.

---

## DECISIONES ARQUITECTONICAS CLAVE

1. **Unificar dos Design Systems paralelos** que existian sin coordinacion
   (`jk-components.css` vs `jk-ds.css`).

2. **Migracion progresiva**: ambos sistemas coexisten durante la transicion.
   El DS v2 carga DESPUES del v1, por lo que sus reglas tienen prioridad
   por cascada CSS. Ningun archivo existente se elimina hasta Fase 5.

3. **5 archivos CSS** en `Content/DesignSystem/`:
   - `jk-tokens.css` - todas las variables de diseno
   - `jk-base.css` - reset, tipografia, animaciones
   - `jk-layout.css` - AppShell, Sidebar, Topbar, Grid, responsive
   - `jk-components.css` - todos los componentes UI
   - `jk-utilities.css` - helpers de espaciado y display

4. **11 partials Razor** en `Views/Shared/Components/` como biblioteca
   de componentes reutilizables.

5. **Sidebar y Topbar** refactorizados para usar clases del DS.
   IDs preservados (`#sidebar`, `#sidebar-collapse-toggle`,
   `#mobile-menu-toggle`) para compatibilidad con `jk-layout.js`.

6. **Contenido paginated** en `Content/Pages/` (vacio por ahora).
   Los estilos especificos de cada modulo iran ahi.

---

## VALORES DE TOKENS (consensuados)

| Token | Valor | Inspiracion |
|-------|-------|-------------|
| `--jk-bg` | `#f5f5f7` | Apple |
| `--jk-accent` | `#2563eb` | Linear |
| `--jk-text-primary` | `#1d1d1f` | Apple |
| `--jk-text-secondary` | `#86868b` | Apple |
| `--jk-border` | `#e8e8ed` | Apple |
| `--jk-success` | `#34c759` | Apple |
| `--jk-warning` | `#ff9500` | Apple |
| `--jk-danger` | `#ff3b30` | Apple |
| `--jk-radius` | `10px` | Stripe |
| `--jk-radius-lg` | `12px` | Vercel |

---

## ARCHIVOS CREADOS

```
Content/
├── DesignSystem/
│   ├── jk-tokens.css        (122 lineas - 60+ variables)
│   ├── jk-base.css          (96 lineas - reset, typography, animations)
│   ├── jk-layout.css        (245 lineas - AppShell, Sidebar, Topbar, Grid)
│   ├── jk-components.css    (578 lineas - todos los componentes UI)
│   └── jk-utilities.css     (114 lineas - helpers)
│
└── Pages/                   (vacio, para estilos por modulo)

Views/Shared/Components/
├── _PageHeader.cshtml       (header reutilizable con titulo + accion)
├── _SectionHeader.cshtml    (titulo de seccion + link opcional)
├── _ActionCard.cshtml       (card de accion con icono)
├── _KpiCard.cshtml          (metrica con label + valor + tendencia)
├── _EmptyState.cshtml       (estado vacio con icono + CTA)
├── _Timeline.cshtml         (linea de tiempo con dots)
├── _Avatar.cshtml           (avatar circular con inicial)
├── _Drawer.cshtml           (panel lateral reutilizable)
├── _Toast.cshtml            (contenedor de notificaciones)
├── _FloatingButton.cshtml   (FAB solo mobile)
└── _SearchBar.cshtml        (input de busqueda con icono)
```

---

## ARCHIVOS MODIFICADOS

| Archivo | Cambio |
|---------|--------|
| `_Layout.cshtml` | Agregadas 5 hojas DS v2. Seccion `@RenderSection("PageStyles")`. Clases `.jk-app-shell`, `.jk-sidebar-overlay`, `.jk-content-zone` en HTML. |
| `_Sidebar.cshtml` | Refactor completo: clases viejas reemplazadas por `.jk-sidebar-*`, `.jk-nav-item`, `.jk-nav-group`, `.jk-nav-label`. IDs preservados. |
| `_Topbar.cshtml` | Refactor: clases viejas reemplazadas por `.jk-topbar-*`, `.jk-btn-icon`, `.jk-avatar`. |

---

## ARCHIVOS NO MODIFICADOS (importantes)

| Archivo | Estado |
|---------|--------|
| `Views/Home/Index.cshtml` | Pendiente - se refactoriza en Fase 4 |
| `Content/jk-dashboard.css` | Pendiente - se migra en Fase 4 |
| `Content/jk-components.css` (v1) | Obsoleto - se elimina en Fase 5 |
| `Content/jk-layout.css` (v1) | Obsoleto - se elimina en Fase 5 |
| `Content/jk-ds.css` | Obsoleto (nunca cargo en Layout) |
| `Content/jk-home.css` | Obsoleto (nunca cargo en Layout) |
| `Content/jk-sidebar.css` | Obsoleto (nunca cargo en Layout) |
| `Content/jk-topbar.css` | Obsoleto (nunca cargo en Layout) |
| `Scripts/jk-layout.js` | Sin cambios - funciona con IDs preservados |
| `Controllers/*`, `Models/*`, etc. | Backend intocable |

---

## MAPA DE CLASES (v1 → v2)

### Sidebar
| v1 (jk-layout.css) | v2 (DesignSystem/jk-layout.css) |
|--------------------|----------------------------------|
| `.sidebar-header` | `.jk-sidebar-header` |
| `.sidebar-brand` | `.jk-sidebar-brand` |
| `.sidebar-brand-icon` | `.jk-sidebar-brand-icon` |
| `.sidebar-brand-text` | `.jk-sidebar-name` |
| `.sidebar-subtitle` | `.jk-sidebar-subtitle` |
| `.sidebar-menu` | `.jk-sidebar-menu` |
| `.sidebar-group` | `.jk-nav-group` |
| `.sidebar-group-label` | `.jk-nav-label` |
| `.sidebar-text` | `<span>` sin clase (texto directo) |
| `.sidebar-active` | `.active` (sobre jk-nav-item) |
| `.sidebar-footer` | `.jk-sidebar-footer` |
| `.sidebar-collapse-btn` | `.jk-nav-collapse` |
| `.btn-logout` | `.jk-nav-item.jk-nav-item-logout` |
| `.jk-btn.jk-btn-ghost` (nav) | `.jk-nav-item` |
| `w-100` | se hereda del layout flex |

### Topbar
| v1 | v2 |
|----|----|
| `.topbar-left` | `.jk-topbar-left` |
| `.topbar-right` | `.jk-topbar-right` |
| `.topbar-menu-btn` | `.jk-topbar-toggle` |
| `.topbar-title` | `.jk-topbar-title` |
| `.user-profile` | `.jk-topbar-user` |
| `.avatar-circle` | `.jk-avatar` |
| `.user-name` | `.jk-topbar-username` |
| `.jk-btn.jk-btn-ghost.p-2` (bell) | `.jk-btn-icon` |

---

## FALTANTES - Componentes del catalogo no implementados aun

Estos componentes estan definidos en el catalogo arquitectonico pero
no tienen implementacion aun. Se crearan cuando se necesiten:

- `jk-tabs.css` / `_Tabs.cshtml`
- `jk-pagination.css` / `_Pagination.cshtml`
- `jk-progress.css` / `_ProgressBar.cshtml`
- `jk-stepper.css` / `_Stepper.cshtml`
- `jk-accordion.css` / `_Accordion.cshtml`
- `jk-dropdown.css` / `_Dropdown.cshtml`
- `jk-alert.css` / `_Alert.cshtml`
- `BottomNav` / `_BottomNav.cshtml`

---

## ORDEN DE TRABAJO RECOMENDADO (proximo)

```
FASE 3 - Templates (modulos existentes)
  [ ] Refactorizar Clientes/Index.cshtml (usar _PageHeader, _EmptyState, jk-table)
  [ ] Refactorizar Clientes/Create.cshtml (usar jk-form-group, jk-input, etc)
  [ ] Refactorizar Clientes/Edit.cshtml
  [ ] Refactorizar Servicios/Index.cshtml
  [ ] Refactorizar Servicios/Create.cshtml
  [ ] Refactorizar Servicios/Edit.cshtml
  [ ] Verificar CRUD completo funcionando

FASE 4 - Home / Command Center
  [ ] Refactorizar Home/Index.cshtml (usar _PageHeader, _ActionCard, _KpiCard, _Timeline)
  [ ] Migrar jk-dashboard.css -> Pages/jk-home.css
  [ ] Integrar widget Mi Dia con componentes del DS

FASE 5 - Cleanup
  [ ] Verificar que ninguna vista usa clases v1
  [ ] Eliminar jk-components.css (v1)
  [ ] Eliminar jk-layout.css (v1)
  [ ] Eliminar jk-ds.css, jk-home.css, jk-sidebar.css, jk-topbar.css
  [ ] Actualizar _Layout.cshtml (remover links v1)

MODULOS NUEVOS (futuro)
  [ ] Agenda / Calendario
  [ ] Recursos
  [ ] Configuracion
  [ ] Reportes
```

---

## REGLAS PARA EL PROXIMO DESARROLLADOR

1. **Siempre cargar primero el diseno, despues la funcionalidad.** No crear
   una pagina nueva sin antes verificar que los componentes del DS existen.

2. **Usar partials del DS.** Para PageHeader, llamar
   `@Html.Partial("~/Views/Shared/Components/_PageHeader.cshtml", ...)`.
   No escribir el HTML manualmente.

3. **No hardcodear valores.** Usar siempre variables CSS (`var(--jk-text)`).
   Si necesitas un valor nuevo, agregalo a `jk-tokens.css`.

4. **No crear CSS por pagina.** Los estilos de modulo van en
   `Content/Pages/jk-{modulo}.css`. Los estilos de componente van en
   `DesignSystem/jk-components.css`.

5. **No borrar archivos v1 aun.** Aunque parezcan no usarse, la migracion
   debe ser progresiva. Solo eliminar en Fase 5, cuando NO haya ninguna
   vista que referencie clases v1.

6. **Textos sin acentos.** Por compatibilidad de encoding.

7. **Sidebar y Topbar usan IDs para JS.** No cambiar `id="sidebar"`,
   `id="sidebar-collapse-toggle"`, `id="mobile-menu-toggle"`. El JS
   `jk-layout.js` depende de ellos.

---

## COMANDOS UTILES

```powershell
# Compilar el proyecto
dotnet build  (si es .NET Core/.NET 5+)
# o
msbuild JKTurnos.csproj  (si es .NET Framework 4.7)

# Verificar que no hay errores de compilacion antes de continuar
```

---

## PROXIMA SESION

Continuar con FASE 3: Refactorizar los CRUDs de Clientes y Servicios
para usar los componentes del Design System v2.

**************************************************************************
Avances de fase 3 se documentaran en un nuevo archivo `07_MIGRACION_DS_FASE3.md`.
