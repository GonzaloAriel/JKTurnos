using System;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.AdminGlobal)]
    public class AdminGlobalDashboardController : BaseController
    {
        private readonly DashboardAdminGlobalService _service;

        public AdminGlobalDashboardController()
        {
            _service = new DashboardAdminGlobalService();
        }

        public ActionResult Index()
        {
            var model = _service.GetDashboard(DateTime.Today);
            return View(model);
        }
    }
}
