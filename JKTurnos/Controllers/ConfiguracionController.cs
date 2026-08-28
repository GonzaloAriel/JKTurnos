using System;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class ConfiguracionController : BaseController
    {
        private readonly ConfiguracionService _service;
        private readonly TenantService _tenantService;

        public ConfiguracionController()
        {
            _service = new ConfiguracionService();
            _tenantService = new TenantService();
        }

        // GET: Configuracion
        public ActionResult Index()
        {
            ViewBag.Configuracion = _service.GetConfiguracion();
            return View();
        }

        // GET: Configuracion/Suscripcion
        public ActionResult Suscripcion()
        {
            ViewBag.Suscripcion = _tenantService.GetSuscripcionTenant(TenantId);
            ViewBag.PagosSuscripcion = _tenantService.GetPagosTenant(TenantId);
            return View();
        }

        // GET: Configuracion/DatosNegocio
        public ActionResult DatosNegocio()
        {
            var config = _service.GetConfiguracion() ?? new ConfiguracionTenant();
            config.SmtpPassword = null; // nunca se re-renderiza la password almacenada
            ViewBag.Avatares = new AvatarPlataformaService().GetAll();
            return View(config);
        }

        // POST: Configuracion/DatosNegocio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatosNegocio(ConfiguracionTenant model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Avatares = new AvatarPlataformaService().GetAll();
                return View(model);
            }

            try
            {
                _service.UpdateDatosNegocio(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Configuracion/Notificaciones
        public ActionResult Notificaciones()
        {
            ViewBag.Configuracion = _service.GetConfiguracion();
            return View();
        }

        // GET: Configuracion/NotificacionesWhatsApp
        public ActionResult NotificacionesWhatsApp()
        {
            var config = _service.GetConfiguracion() ?? new ConfiguracionTenant();
            return View(config);
        }

        // POST: Configuracion/NotificacionesWhatsApp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotificacionesWhatsApp(ConfiguracionTenant model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.UpdateNotificacionWhatsApp(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Notificaciones");
        }

        // GET: Configuracion/NotificacionesEmail
        public ActionResult NotificacionesEmail()
        {
            var config = _service.GetConfiguracion() ?? new ConfiguracionTenant();
            config.SmtpPassword = null; // nunca se re-renderiza la password almacenada
            return View(config);
        }

        // POST: Configuracion/NotificacionesEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotificacionesEmail(ConfiguracionTenant model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.UpdateNotificacionEmail(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Notificaciones");
        }
    }
}