using System.Security.Claims;
using System.Web.Mvc;
using JKTurnos.Infrastructure;

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

        protected override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            ViewBag.TenantId = TenantId;

            ViewBag.UserName =
                User.Identity.Name;

            base.OnActionExecuting(filterContext);
        }
    }
}