using System.Web.Mvc;
using System.Web.UI;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // D-042 / Posta 3: vidriera publica, vista read-only aislada del backoffice.
    // NO hereda de BaseController (el publico anonimo no debe tener claims ni
    // navegacion interna). Unica action Index(string slug); no hay POST.
    [AllowAnonymous]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*", Location = OutputCacheLocation.None)]
    public class VidrieraController : Controller
    {
        private readonly VidrieraService _service;

        public VidrieraController()
        {
            _service = new VidrieraService();
        }

        public ActionResult Index(string slug)
        {
            var vm = _service.GetVidrieraPublica(slug);
            if (vm == null)
                return HttpNotFound();

            return View(vm);
        }
    }
}
