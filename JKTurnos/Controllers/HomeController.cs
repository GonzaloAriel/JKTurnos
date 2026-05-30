using System.Web.Mvc;

namespace JKTurnos.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.CurrentTenant = TenantId;
            ViewBag.CurrentUser = UserId;

            return View();
        }
    }
}