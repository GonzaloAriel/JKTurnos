# SESION 2026-09-05: PRIMEROS PASOS DE UI + BIENVENIDA PRIMER INGRESO

## Objetivo
Mejoras de presentacion (login/logo/favicon, FAQ por rol, animacion movil)
+ Checklist de primeros pasos en el Command Center (M-09) + Bienvenida de
primer ingreso (M-15) + numero de dia en la grilla de la Vidriera.

## Estado
- M-09 (checklist de primeros pasos en Command Center): IMPLEMENTADO
  (2026-09-05), pendiente confirmacion TEAM LEAD. Vista Home/Index reemplaza
  la seccion estatica "Acciones sugeridas".
- M-15 (bienvenida primer ingreso): IMPLEMENTADO (2026-09-05), pendiente
  confirmacion TEAM LEAD. Aparece UNA sola vez en el primer login del usuario
  tenant, sin tocar el esquema de datos.
- Vidriera (M-13, ajuste): filas de dias muestran el numero del dia del mes
  ("SAB 5"). Ajuste visual solo en vista + CSS.
- Build MSBuild Debug al cierre: 0 errores.

## Cambios

### Login / logo / favicon
- `Views/Shared/_LoginLayout.cshtml`: icono `bi bi-calendar-check` reemplazado
  por `<img class="jk-login-logo-img" src="~/Img/Loguin/Sol.png">`; favicon PNG
  agregado tras `<title>` (se conserva `favicon.ico`).
- `Content/Pages/jk-login.css`: `.jk-login-logo-img` 60x60 `object-fit:contain`,
  SIN fondo/sombra/padding (pedido del usuario).
- `JKTurnos.csproj`: `<Content Include="Img\Loguin\Sol.png" />`.
- `Views/Shared/_Layout.cshtml` y `_VidrieraLayout.cshtml`: favicon PNG
  `~/Img/Loguin/Sol.png`.

### FAQ / Ayuda por rol
- `Views/Ayuda/Index.cshtml` reescrito: 6 grupos con gating por rol via
  `User.IsInRole`/`!IsInRole`:
  - Operacion diaria (`!AdminGlobal`): Command Center, crear turno, mover turno,
    colores de bloques (color=recurso, no estado), estado operativo vs
    economico, senas/pagos, agregar servicio/recurso, vincular, busqueda global.
  - Recordatorios (`!AdminGlobal`): WhatsApp, email, plantillas, campanita.
  - Configuracion del negocio (`Admin`): Dashboard, bloqueos, logo/avatar,
    auditoria, vidriera.
  - Roles (`Admin || AdminGlobal`), Plataforma (`AdminGlobal`): suscripciones/
    pagos plataforma + Dashboard Global. Cuenta (`Admin || AdminGlobal`):
    cambiar contrasena.
- `Content/Pages/jk-ayuda.css`: `.jk-faq-group` para encabezados de grupo.

### Agenda movil: animacion de acordeon
- `Scripts/jk-agenda.js`: `initMobileAccordion()` escucha el evento `toggle` de
  `.jk-agenda-mobile-resource` (`<details>`) y anima la altura del body
  (0 -> scrollHeight al abrir, colapsa al cerrar) con `--jk-transition*`;
  fallback nativo si no dispara el evento. Validado con `node --check`.

### M-09: Checklist de primeros pasos en Command Center
- `Models/ViewModels/HomeViewModel.cs`: `SetupSteps` (List<SetupStepVM>) +
  propiedad calculada `SetupPendientes`; `SetupStepVM` (Clave, Titulo,
  Descripcion, Url, Icono, Completo). `using System.Linq`.
- `Services/HomeService.cs`: `BuildSetupChecklist(ConfiguracionTenant)` detecta
  5 pasos: recurso (`RecursoService`), servicio (`ServicioService`), horarios
  (`HoraApertura`+`HoraCierre`+`DiasLaborales`), logo (`LogoUrl`||`AvatarUrl`),
  datos (`NombreComercial`). `using System.Collections.Generic`.
- `Controllers/HomeController.cs`: `Index()` filtra para `!IsAdmin` solo claves
  `recurso`/`servicio` (Configuracion es rol Admin). `using System.Linq`.
- `Views/Home/Index.cshtml`: card "Poner en marcha tu negocio" (progreso %,
  lista de pasos con links, checks) reemplaza la seccion "Acciones sugeridas";
  visible solo si `SetupPendientes > 0` (respeta la configuracion progresiva:
  nunca obligar).
- `Content/jk-dashboard.css`: estilos `.jk-onboarding-*` con tokens `--jk-*`.

### M-15: Bienvenida al primer ingreso (usuario tenant)
- `Services/AuthService.cs`: `Login(email, password, out bool esPrimerIngreso)`.
  Se calcula ANTES de registrar el login: es primer ingreso si el usuario (rol
  != AdminGlobal) NO tiene ningun `A_Login` previo en `Auditorias`. Sin
  migracion / sin cambio de esquema.
- `Controllers/AccountController.cs`: si `esPrimerIngreso` y rol != AdminGlobal,
  `TempData["PrimerIngreso"] = true` antes del redirect a Home.
- `Views/Shared/Components/_BienvenidaPrimerIngreso.cshtml` (nuevo): modal
  centrado con backdrop, icono `bi-balloon-heart`, titulo "Bienvenido a
  JKTurnos!", mensaje entusiasta, CTA "Empezar a usar JKTurnos". Lee
  `TempData["PrimerIngreso"]` (NO Peek): al consumirlo no vuelve a aparecer.
  Cierre por X, backdrop y CTA; `requestAnimationFrame` agrega `.is-open`.
- `Views/Shared/_Layout.cshtml`: render del partial de bienvenida.
- `Content/jk-dashboard.css`: estilos `.jk-welcome-*` (backdrop fijo centrado,
  animacion suave). Corregido `--jk-space-40` (no existe) por `--jk-space-32`.

### Vidriera: numero de dia en la grilla
- `Views/Vidriera/Index.cshtml`: la celda `.day` ahora muestra
  `<span class="day-name">@dd.NombreDia</span>` + `<span class="day-num">@dd.DiaDelMes</span>`
  (el campo `DiaDelMes` ya existia en `VidrieraDisponibilidadDiaVM`).
- `Content/Pages/jk-vidriera.css`: `.day` pasa a columna (nombre arriba en
  mayusculas, numero grande debajo).

## Decisiones nuevas
- D-049: Bienvenida de primer ingreso (M-15). Deteccion por auditoria (sin
  migracion): primer ingreso = usuario tenant sin `A_Login` previo; se evalua
  antes de registrar el login. La UI se muestra una sola vez via
  `TempData["PrimerIngreso"]` (consumido al leerse en `_Layout`). Excluye
  `AdminGlobal` (va a su Dashboard Global sin bienvenida).

## Reglas de negocio aplicadas
- D-032: UTF-8 habilitado; textos respetan el estilo actual (sin acentos en
  textos visibles nuevos de UI).
- D-010/D-035/D-036: estilos con tokens `--jk-*` del DS.
- D-008: soft delete / filtro por tenant intactos (no se toco modelo).

## Pendientes
1. Confirmacion TEAM LEAD de M-09 (checklist Command Center) y M-15
   (bienvenida primer ingreso).
2. M-09: al completar todos los pasos la card desaparece sola
   (`SetupPendientes == 0`) - verificar en runtime.