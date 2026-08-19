using System;
using System.Web.Mvc;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // D-034: Seccion Dashboard (seccion aparte del Command Center).
    // Muestra el estado del negocio con datos reales, sin modificar el Home.
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _service;

        public DashboardController()
        {
            _service = new DashboardService();
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            var model = _service.GetDashboard(DateTime.Today);
            return View(model);
        }
    }
}