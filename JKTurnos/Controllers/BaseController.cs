using System.Security.Claims;
using System.Web.Mvc;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;

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

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            ViewBag.TenantId = TenantId;

            ViewBag.UserName =
                User.Identity.Name;

            var config =
                new Services.ConfiguracionService()
                    .GetConfiguracion();

            ViewBag.ConfiguracionLogo =
                config != null
                    ? (!string.IsNullOrWhiteSpace(config.AvatarUrl)
                        ? config.AvatarUrl
                        : config.LogoUrl)
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