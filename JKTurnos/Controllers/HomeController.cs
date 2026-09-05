using System;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    public class HomeController : BaseController
    {
        private readonly HomeService _homeService;

        public HomeController()
        {
            _homeService = new HomeService();
        }

        public ActionResult Index()
        {
            var model = _homeService.GetHomeData(DateTime.Today);
            ViewBag.CurrentTenant = TenantId;
            ViewBag.CurrentUser = UserId;

            // M-09: los pasos de configuracion del negocio (horarios, logo, datos)
            // son solo de Admin; User solo ve recurso/servicio (que puede crear).
            if (!IsAdmin && model.SetupSteps != null)
            {
                var soloOperativos = new[] { "recurso", "servicio" };
                model.SetupSteps = model.SetupSteps
                    .Where(s => soloOperativos.Contains(s.Clave))
                    .ToList();
            }

            return View(model);
        }
    }
}