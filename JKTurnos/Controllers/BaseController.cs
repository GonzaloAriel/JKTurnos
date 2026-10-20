using System.Security.Claims;
using System.Web.Mvc;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;
using Microsoft.Owin.Security;

namespace JKTurnos.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected int TenantId
        {
            get
            {
                return new TenantProvider()
                    .GetTenantId();
            }
        }

        protected int UserId
        {
            get
            {
                var identity =
                    User.Identity as ClaimsIdentity;

                var claim =
                    identity.FindFirst(
                        ClaimTypes.NameIdentifier);

                return int.Parse(claim.Value);
            }
        }

        protected string CurrentRole
        {
            get
            {
                var identity =
                    User.Identity as ClaimsIdentity;

                if (identity == null)
                    return null;

                var claim =
                    identity.FindFirst(
                        ClaimTypes.Role);

                return claim != null ? claim.Value : null;
            }
        }

        protected bool IsAdminGlobal
        {
            get { return CurrentRole == Roles.AdminGlobal; }
        }

        protected bool IsAdmin
        {
            get { return CurrentRole == Roles.Admin; }
        }

        // Devuelve false (y cierra la sesion) si el usuario fue desactivado,
        // el tenant fue desactivado o el rol del usuario ya no coincide con el
        // claim vigente. Evita que una cookie vieja conserve accesos revocados.
        private bool SesionVigente(ActionExecutingContext filterContext)
        {
            var identity = User.Identity as ClaimsIdentity;

            if (identity == null)
                return true; // [Authorize] ya habra redirigido al Login.

            var idClaim =
                identity.FindFirst(ClaimTypes.NameIdentifier);

            if (idClaim == null)
                return true;

            int userId;

            if (!int.TryParse(idClaim.Value, out userId))
                return true;

            User user;

            using (var context = new Data.JKContext())
            {
                user = context.Users
                    .FirstOrDefault(u => u.Id == userId);
            }

            var rolClaim =
                identity.FindFirst(ClaimTypes.Role);

            var rolActual =
                rolClaim != null ? rolClaim.Value : null;

            var sesionInvalida =
                user == null
                || !user.IsActive
                || user.Role != rolActual;

            // Validar el tenant salvo para AdminGlobal (TenantId 0).
            if (!sesionInvalida && user.TenantId != 0)
            {
                Tenant tenant;

                using (var context = new Data.JKContext())
                {
                    tenant = context.Tenants
                        .FirstOrDefault(t => t.Id == user.TenantId);
                }

                sesionInvalida =
                    tenant == null || !tenant.IsActive;
            }

            if (!sesionInvalida)
                return true;

            System.Web.HttpContext.Current
                .GetOwinContext()
                .Authentication
                .SignOut("ApplicationCookie");

            filterContext.Result = new RedirectResult("~/Account/Login");

            return false;
        }

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            // Revalidacion de sesion vigente: la cookie puede seguir siendo valida
            // aunque la cuenta fue desactivada o el rol/tenant cambio despues del
            // login. Se recarga el usuario y el tenant en cada request.
            if (!SesionVigente(filterContext))
                return;

            ViewBag.TenantId = TenantId;

            ViewBag.EsAdmin = IsAdmin;

            ViewBag.UserName =
                User.Identity.Name;

            var config =
                new Services.ConfiguracionService()
                    .GetConfiguracion();

            ViewBag.ConfiguracionLogo =
                config != null
                    ? (!string.IsNullOrWhiteSpace(config.LogoUrl)
                        ? config.LogoUrl
                        : config.AvatarUrl)
                    : null;

            ViewBag.ConfiguracionNombre =
                config != null
                    ? config.NombreComercial
                    : null;

            // Feed de actividad (campanita + Actividad reciente).
            var notifService = new Services.NotificacionSistemaService();
            ViewBag.TopbarNoLeidas = notifService.GetNoLeidasCount();
            ViewBag.TopbarNotificaciones = notifService.GetUltimas(8);

            // Recordatorios operativos de la campanita (calculados en vivo).
            var recordatorioService = new Services.RecordatorioService();
            var recordatorios = recordatorioService.GetRecordatorios(incluirConfiguracion: IsAdmin);
            ViewBag.TopbarRecordatorios = recordatorios;
            ViewBag.TopbarReminderCount = recordatorios.Count;

            base.OnActionExecuting(filterContext);
        }
    }
}