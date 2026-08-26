using System;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.AdminGlobal)]
    public class PagosSuscripcionController : BaseController
    {
        private readonly PagoSuscripcionService _service;
        private readonly TenantService _tenantService;

        public PagosSuscripcionController()
        {
            _service = new PagoSuscripcionService();
            _tenantService = new TenantService();
        }

        public ActionResult Registrar(int? tenantId)
        {
            ViewBag.Tenants = new SelectList(_tenantService.GetAll(), "Id", "Nombre", tenantId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(int tenantId, string planCodigo, decimal monto, DateTime fecha, string observacion)
        {
            try
            {
                _service.Registrar(tenantId, planCodigo, monto, fecha, observacion, UserId);
                TempData["Success"] = "Pago registrado correctamente.";
                return RedirectToAction("Index", "AdminGlobalDashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Tenants = new SelectList(_tenantService.GetAll(), "Id", "Nombre", tenantId);
                return View();
            }
        }
    }
}
