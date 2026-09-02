# JKTurnos — Posta 5 (Paneles Vidriera): Guía de Retoma

> Archivo de continuidad. Documenta el estado real de la implementación de la
> **Posta 5** (M-13, D-042): panel de administración de la Vidriera pública del
> tenant. Consumí todo esto en conjunto con el contexto del proyecto antes de continuar.

---

## 1. Estado actual (resumen)

| Bloque | Estado |
|---|---|
| ViewModel `VidrieraAdminViewModel.cs` | ✅ Completo |
| `VidrieraService` (métodos admin) | ✅ Completo |
| `VidrieraAdminController` | ✅ Completo |
| `AuditService` (constante `E_Vidriera`) | ✅ Completo |
| Vista `Views/VidrieraAdmin/Index.cshtml` | ✅ Completo |
| Sidebar `_Sidebar.cshtml` (item "Vidriera") | ✅ Completo |
| Registro en `JKTurnos.csproj` | ✅ Completo |
| **Build** (`MvcBuildViews=true`) | ✅ **0 errores** |
| **Verificación runtime** (HTTP autenticado) | ✅ **Completo** |
| Docs (D-045 + cierre de posta) | ✅ **Completo** |

> **Posta 5 CERRADA y CONSOLIDADA (2026-09-02).** Implementación + verificación
> runtime + documentación (D-045 y cierre en `AI/DISENO_VIDRIERA_M13_POSTA3.md`
> secciones 14-15) terminadas. Completadas además (2026-09-02): extensión de la
> Vista Pública (mensaje/oferta/servicios con flags) y el **REDISENO MONITOR** de
> la vidriera (dos columnas, entrada en 17" sin scroll, más publicitaria y con
> logo/avatar con precedencia LogoUrl > AvatarUrl > fallback) — todo en la seccion 16
> del design doc y consolidado en `01_ESTADO.md` (módulo M-13). Este archivo queda
> como historial/referencia; las siguientes postas de M-13 son 6 (refresh),
> 7 (modo monitor) y 8 (seguridad).

---

## 2. Rap (Datos clave del proyecto)

- **Root:** `C:\Users\Gonzo\Desktop\JKTurnos\JKTurnos\JKTurnos\JKTurnos`
- **DB:** `Server=.\SQLEXPRESS;Database=JKTurnosDB` (Trusted_Connection)
- **EF6 spanish pluralization:** el nombre real de tablas es `VidrieraPublicas`, `Turnoes`, `Recursoes`, `EstadoTurnoes` (no `VidrieraPublica`/`Turnos`).
- **Auth:** OWIN application cookie (`Startup.cs`). Login POST → `Account/Login` con `LoginVM` (`Email`, `Password`). El panel requiere rol `Admin`.
- **AuditService:** `A_Crear`/`A_Editar` + constantes de entidad. `Campo(string nombre, string antes, string despues)` — **todos los valores van como string** (bool → `.ToString()`).
- **Método runtime establecido:** MSBuild (`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` con `/p:MvcBuildViews=true`) → `Start-Process iisexpress /port:53813` → `Invoke-WebRequest` → limpiar → `Get-Process iisexpress | Stop-Process -Force`.

---

## 3. Confirmación de indentidad (lo que voy a usar)

- **Login:** `GET /Account/Login` entrega cookie + `__RequestVerificationToken`.
  - POST a `/Account/Login` con `Email`, `Password`, `__RequestVerificationToken` y la cookie.
  - Campos del `LoginVM`: `Email`, `Password` (ver `Controllers/AccountController.cs`).
- **Panel admin:** `GET /VidrieraAdmin` (Index) y `POST /VidrieraAdmin/Guardar`.
- **Publica:** `GET /vidriera/{slug}`.

---

## 4. Próximo paso — Verificación runtime (HTTP autenticado)

Necesito loguear un usuario **Admin** y probar el flujo. Realizo login vía PowerShell con sesión de cookies + token de anti-forgery.

### 4.1 Arrancar IIS Express

```powershell
# (opcional) reiniciar limpio
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1
Start-Process "C:\Program Files\IIS Express\iisexpress.exe" `
  -ArgumentList "/path:`"C:\Users\Gonzo\Desktop\JKTurnos\JKTurnos\JKTurnos\JKTurnos`"","/port:53813"
Start-Sleep -Seconds 6
(Invoke-WebRequest -Uri "http://localhost:53813/" -UseBasicParsing -TimeoutSec 10).StatusCode
# Esperado: 200 (home redirige a login si no está autenticado)
```

### 4.2 Login autenticado (sesión de cookies + anti-forgery)

Usar un `WebRequestSession` de PowerShell y extraer el `__RequestVerificationToken` de la página de login:

```powershell
$sess = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$login = Invoke-WebRequest -Uri "http://localhost:53813/Account/Login" -WebSession $sess -UseBasicParsing
$token = [regex]::Match($login.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value

$creds = @{
  Email = "<admin-email>"
  Password = "<admin-password>"
  __RequestVerificationToken = $token
}
$r = Invoke-WebRequest -Uri "http://localhost:53813/Account/Login" `
  -Method Post -Body $creds -WebSession $sess -UseBasicParsing -MaximumRedirection 0
# Esperado: 302 redirect (login OK). La cookie AUTH queda en $sess.
```

> **Aviso:** si no se conoce un usuario Admin existente, crear/cargar uno en DB
> (hash de password vía la misma lógica de `AuthService`) o usar un usuario que
> ya exista en el entorno de prueba. Recordar que el proyecto permite datos de
> prueba (no producción).

### 4.3 Probar el panel admin (GET)

```powershell
$admin = Invoke-WebRequest -Uri "http://localhost:53813/VidrieraAdmin" -WebSession $sess -UseBasicParsing
$admin.StatusCode   # Esperado: 200
$admin.Content -match 'Todavia no creaste tu vidriera'   # si tenant sin vidriera → true
```

### 4.4 Probar creación (POST Guardar)

Extraer el `__RequestVerificationToken` del GET anterior y mandar los campos del `VidrieraAdminViewModel`:

```powershell
$gToken = [regex]::Match($admin.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
$form = @{
  __RequestVerificationToken = $gToken
  Slug = "mi-negocio"
  Habilitada = "true"
  Titulo = "Bienvenido"
  Descripcion = "Descr"
  MensajePrincipal = "Hola"
  OfertaTitulo = "2x1"
  OfertaDescripcion = "Promo"
  MostrarServicios = "true"
  MostrarPrecios = "true"
  MostrarDisponibilidad = "true"
  MostrarOfertas = "true"
  MostrarMensaje = "true"
}
$p = Invoke-WebRequest -Uri "http://localhost:53813/VidrieraAdmin/Guardar" `
  -Method Post -Body $form -WebSession $sess -UseBasicParsing -MaximumRedirection 0
# Esperado: 302 (redirect a Index). Luego re-GET y verificar que muestra badge "Publicada" + url /vidriera/mi-negocio.
```

### 4.5 Verificar publicación (público)

```powershell
# Abrirla en una sesión NO autenticada
(Invoke-WebRequest -Uri "http://localhost:53813/vidriera/mi-negocio" -UseBasicParsing -MaximumRedirection 3).StatusCode
# Esperado: 200
```

### 4.6 Validación de slug duplicado (negativo)

Repetir Guardar con otro **tenant** que ya tenga el slug `mi-negocio`, o intentar
un slug igual al de otra vidriera existente → se debe mostrar el error "Ese slug
ya esta en uso. Elegi otro." (queda en el `ModelState` y el POST devuelve la vista
Index con `ValidationSummary`, no redirect).

### 4.7 Despublicar

Guardar con `Habilitada = "false"` → re-GET de `/vidriera/{slug}` debe responder **404**.

### 4.8 Limpieza

```powershell
# Borrar filas de prueba de VidrieraPublicas (y turnos/recursos si se crearon) vía sqlcmd
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force
```

---

## 5. Posta 5 — Cierre de documentación (COMPLETO)

Ejecutado el 2026-09-02:

1. **`AI/02_DECISIONES.md`**: registrar la decisión **D-045** con
   - índice (una fila: id, título, estado) y
   - detalle (alcance del panel admin: slug editable + validación de unicidad global, campos de contenido, flags, publish/unpublish, preview).
2. **`AI/DISENO_VIDRIERA_M13_POSTA3.md`**: agregar/cerrar la sección de la Posta 5
   (marcar "IMPLEMENTADA y VERIFICADA"), dejando pendientes las Postas 6-8 (si aplican).
3. **Actualizar todos** (todo list) → marcar verificación runtime y docs como completos.
4. Si corresponde, citar en `AI/CONTRATO_VIDRIERA_M13.md` o `AI/PLAN_MEJORAS.md` (M-01/M-02).

---

## 6. Lista de todos actual para retomar

| # | Item | Estado |
|---|---|---|
| 1 | ViewModel VidrieraAdminViewModel.cs | done |
| 2 | VidrieraService admin methods | done |
| 3 | VidrieraAdminController | done |
| 4 | AuditService E_Vidriera | done |
| 5 | Views/VidrieraAdmin/Index.cshtml | done |
| 6 | _Sidebar "Vidriera" nav | done |
| 7 | csproj + build + runtime verify | done |
| 8 | docs D-045 + cierre posta | done |

---

## 7. Archivos relevantes (path completo bajo el root)

- `Views/VidrieraAdmin/Index.cshtml` — formulario (nuevo)
- `Views/Shared/_Sidebar.cshtml` — item "Vidriera" en Configuración
- `Controllers/VidrieraAdminController.cs` — Index GET + Guardar POST
- `Services/VidrieraService.cs` — GetVidrieraAdmin / GuardarVidriera / NormalizarSlug / GetUrlPublica / MapAdmin (Posta 4 availability ya ahí)
- `Services/AuditService.cs` — `E_Vidriera` + `Campo`/`FormatearCambios`
- `Models/ViewModels/VidrieraAdminViewModel.cs`
- `Models/ViewModels/VidrieraDisponibilidadVM.cs`, `VidrieraPublicaViewModel.cs` (Posta 4)
- `Models/Entities/VidrieraPublica.cs`
- `Controllers/AccountController.cs` (Login, LoginVM)
- `Startup.cs` (OWIN application cookie)
- `AI/DISENO_VIDRIERA_M13_POSTA3.md`, `AI/02_DECISIONES.md`, `AI/CONTRATO_VIDRIERA_M13.md`, `AI/PLAN_MEJORAS.md`
- `JKTurnos.csproj` (3 entradas nuevas ya registradas)
