# AUDITORIA SEGURIDAD - PRE-DEPLOY JKTurnos

FORMATO: 1.0

Estado: TODOS LOS HALLAZGOS REMEDIADOS (2026-09-06).
Alcance: auditoria de seguridad del SaaS multi-tenant (ASP.NET MVC 5, .NET Framework 4.8, EF6, OWIN Cookie Auth) ANTES del deploy a hosting.

Regla del proyecto: textos sin acentos. Stack: Controller -> Service -> Repository -> EF, multi-tenant por TenantId. Nunca romper el flujo.

## 1. Metodologia

Auditoria manual de las capas Controller / Service / Repository / Vistas / Configuracion / JS propio, con foco en las top 10 OWASP y los ataques especificos a un SaaS multi-tenant:
- Inyeccion SQL
- XSS (reflejado / almacenado / DOM)
- CSRF
- Session fixation / cookie auth
- IDOR / object-level authorization (cross-tenant)
- Exposicion de informacion (headers, errores, debug)
- Fuerza bruta / cuentas
- Token hijacking (JSON GET)
- Mass assignment / over-posting (Id, TenantId, IsActive, CreatedAt, UpdatedAt)
- Version disclosure y headers de servidor

## 2. Resultados de la auditoria previa (2026, pre-remediacion)

- SQL Injection: CERO (toda la persistencia pasa por EF parametrizado).
- CSRF en POST: OK en los formularios (ValidateAntiForgeryToken).
- Tenant isolation en repos: OK (BaseRepository filtra TenantId + IsActive).
- IDOR cross-tenant: bloqueado (los services cargan por GetById que filtra tenant).

Hallazgos ordenados por severidad:

## 3. CRITICAL (5) - REMEDIADOS

| ID | Hallazgo | Remedio | Archivo |
|----|----------|---------|---------|
| C-1 | machineKey por defecto / no reproducible | machineKey nueva con CSPRNG, validation=HMACSHA256 | Web.config |
| C-2 | Sin lockout de cuenta (fuerza bruta) | Lockout in-memory: 5 intentos fallidos -> 15 min bloqueo | Services/AuthService.cs |
| C-3 | @Html.Raw en Vidriera (XSS almacenado) | Encodado estandar @Model.OfertaTitulo | Views/Vidriera/Index.cshtml |
| C-4 | Cookie sin HttpOnly / Secure | CookieHttpOnly, CookieSecure=Always, CookieSameSite=Lax | Startup.cs |
| C-5 | Session demasiado larga | ExpireTimeSpan=8h + SlidingExpiration | Startup.cs |

Nota C-4: CookieSecure.Always exige HTTPS en el hosting o nadie loguea.

## 4. HIGH (5) - REMEDIADOS

| ID | Hallazgo | Remedio | Archivo |
|----|----------|---------|---------|
| H-1 | Politica de password debil | PasswordPolicy min 6 chars en Create/Update/Reset/Cambiar | Infrastructure/PasswordPolicy.cs, Services/UserService.cs |
| H-2 | debug=true + errores por defecto | debug=false; Web.Release fuerza off + customErrors On; ErrorController | Web.config, Controllers/ErrorController.cs |
| H-3 | innerHTML en toasts (DOM XSS) | createElement + createTextNode en 7 archivos | Turnos/Index, 3 drawers CSHTML, jk-*.js |
| H-4 | Headers de seguridad ausentes | X-Content-Type-Options, X-Frame-Options, Referrer-Policy, X-XSS-Protection | Global.asax.cs |
| H-5 | Logout por GET (CSRF de logout) | Logout [HttpPost]+[ValidateAntiForgeryToken]; sidebar a form POST | Controllers/AccountController.cs, Views/Shared/_Sidebar.cshtml |

## 5. MEDIUM (12) - REMEDIADOS

| ID | Hallazgo | Remedio | Archivo |
|----|----------|---------|---------|
| M-1 | Cookie HttpOnly | CookieHttpOnly=true | Startup.cs |
| M-2 | Cookie Secure | CookieSecure=Always | Startup.cs |
| M-3 | Cookie SameSite | CookieSameSite=Lax | Startup.cs |
| M-4 | - | (agrupado en H-4 headers) | - |
| M-5 | Referrer-Policy | strict-origin-when-cross-origin | Global.asax.cs |
| M-6 | ViewBag interpolado en strings JS (3 drawers) | Valores en data-* + lectura por getAttribute | _*QuickCreateDrawer.cshtml |
| M-7 | JsonRequestBehavior.AllowGet (token hijacking) | Buscar/BuscarTurnos a POST + antiforgery; render() seguro | Controllers/BusquedaController.cs, PagosController.cs, jk-pagos.js, jk-layout.js |
| M-8 | Mass assignment en binding de entidades | [Bind(Exclude="Id,TenantId,CreatedAt,UpdatedAt,IsActive")] | Clientes/Servicios/Recursos/Tenants Controllers |
| M-9 | EF6 sin Global Query Filters (aislamiento manual) | Documentado + BaseRepository.Update fail-closed (el aislamiento queda en repos) | Data/JKContext.cs, Data/BaseRepository.cs |
| M-10 | Update de entidad foranea podia alterar TenantId | Recarga persistida, valida TenantId, preserva TenantId/IsActive/CreatedAt; throws si difiere | Data/BaseRepository.cs |
| M-11 | UserService.GetAll devolvia todos sin filtro (otros roles) | Fail-closed: no-Admin solo ve su tenant; AdminGlobal filtra por tenantId opcional | Services/UserService.cs |
| M-12 | Headers Server / X-Powered-By / version ASP.NET expuestos | removeServerHeader + enableVersionHeader=false | Web.config |

## 6. LOW (5) - REMEDIADOS

| ID | Hallazgo | Remedio | Archivo |
|----|----------|---------|---------|
| L-1 | Timeout de session | 8h (ya en M-5/C-5) | Startup.cs |
| L-2 | Sliding expiration | true (ya en C-5) | Startup.cs |
| L-3 | AdminGlobal podia clonarse (crear/promover otro AdminGlobal) | Sep. de deberes: AdminGlobal NO crea/promueve/edita/elimina AdminGlobal; dropdown sin opcion AdminGlobal | Services/UserService.cs, Controllers/UsuariosController.cs |
| L-4 | BCrypt work factor 10 | 12 (via HashPassword(password, 12)) | Infrastructure/PasswordHasher.cs |
| L-5 | HTTP sin redirigir a HTTPS | URL Rewrite 301 HTTP->HTTPS | Web.config |

Nota L-3: el primer AdminGlobal solo se crea via seed (global@jkturnos.com). El power ya no es clonable desde la UI.

## 7. Total

| Nivel | Cantidad | Estado |
|-------|----------|--------|
| CRITICAL | 5 | TODOS REMEDIADOS |
| HIGH | 5 | TODOS REMEDIADOS |
| MEDIUM | 12 | TODOS REMEDIADOS |
| LOW | 5 | TODOS REMEDIADOS |

MSBuild Debug: 0 errores. node --check OK en los JS propios.

## 8. CHECKLIST PRE-DEPLOY (hosting) - OBLIGATORIO

Revisar en orden antes de publicar. Sin esto, la app puede no funcionar (items 1-3) o quedar debil (4-6).

- [ ] 1. HTTPS / SSL: OBLIGATORIO. Las cookies son Secure (C-4/M-2), hay HSTS condicionado a HTTPS (M-12) y URL Rewrite a HTTPS (L-5). Sin certificado SSL, las cookies de sesion no viajan y NADIE puede loguearse. Activar SSL y asegurar que la request llega como https (o usar el URL Rewrite para forzar).
- [ ] 2. Modulo URL Rewrite de IIS instalado (para L-5 y el redirect HTTP->HTTPS). Si el hosting no lo trae, la regla se ignora (no rompe la app), pero hay que forzar HTTPS por otro medio.
- [ ] 3. Modulo Request Filtering de IIS (para removeServerHeader, M-12). Si falla al arrancar, quitar esa linea de Web.config.
- [ ] 4. machineKey de PRODUCCION: inyectar por variable de entorno / config del hosting (como indica el comentario en Web.config). NO commitear la key de produccion al repositorio. La key local del repo es de dev.
- [ ] 5. BD existente: conserva los hashes de password con BCrypt cost 10 (el work factor 12, L-4, solo aplica a hashes nuevos; los viejos siguen validando). El seed 'JkTurnos2026!@#' (Migrations/Configuration.cs) solo aplica a BD nueva; NO cambia passwords de una BD ya poblada.
- [ ] 6. Test manual post-deploy de los endpoints convertidos a POST (M-7): busqueda global del topbar (autocomplete) y registro de pagos. Flujos funcionales y antiforgery.

## 9. Pruebas manuales recomendadas

1. Login con password incorrecto 5 veces -> cuenta bloqueada 15 min (C-2), mensaje generico.
2. Busqueda del topbar: autocomplete funciona (POST con antiforgery token) y no inyecta HTML (render seguro M-7).
3. Registrar un pago de turno: POST con token (M-7).
4. Como Admin intentar GET directo a /Pagos/BuscarTurnos (deberia 405) y POST sin token (deberia 400/403 antiforgery).
5. Como AdminGlobal: el dropdown de rol NO ofrece AdminGlobal; intentar crear/editar/eliminar un AdminGlobal (deberia rechazarse con mensaje; L-3).
6. Forzar email de otro tenant en el form de usuario (Admin): el servicio filtra por CurrentTenantId (M-11).
7. Forzar TenantId de otro tenant en Update de Cliente/Servicio/Recurso: el repo revalida y lanza UnauthorizedAccess (M-10).
8. Verificar headers en la respuesta: X-Content-Type-Options, X-Frame-Options: SAMEORIGIN, Referrer-Policy, X-XSS-Protection (H-4), y ausencia de Server detallado / X-Powered-By (M-12). Bajo https ademas Strict-Transport-Security.
9. Con la app en debug=false y customErrors On: provocar un error 500 y un 404 y ver las paginas del ErrorController (H-2).

## 10. Deudas / notas (no bloqueantes)

- GetRecursos / GetRecursosPorServicio / GetServiciosPorRecurso de TurnosController quedaron GET (solo devuelven Id/Nombre, bajo riesgo; convertirlos exigiria reescribir la agenda). Documentado.
- _Drawer.cshtml (onclick con @id) y _TurnoDetailDrawer.cshtml (@Url.Action) quedan con valores server-side interpolados (bajo riesgo, revisados sin Html.Raw).
- Lockout (C-2) es in-memory: en pool de IIS multi-proceso o tras recycle se resetea. No es persistente. Aceptable para este alcance.
- EF6 no soporta Global Query Filters (M-9): el aislamiento multi-tenant sigue dependiendo de que toda consulta pase por BaseRepository/GlobalRepository o filtre TenantId. La capa de Update ahora es fail-closed (M-10), la capa de consulta se mantiene por convencion de repos (auditoria previa: tenant isolation OK).
