using System;
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

            return View(model);
        }
    }
}