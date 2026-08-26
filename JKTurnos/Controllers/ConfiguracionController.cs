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

            // AG-5: card de suscripcion del propio tenant (solo lectura, estado derivado en server)
            ViewBag.Suscripcion = _tenantService.GetSuscripcionTenant(TenantId);

            return View(_service.GetEstados());
        }

        // GET: Configuracion/DatosNegocio
        public ActionResult DatosNegocio()
        {
            var config = _service.GetConfiguracion() ?? new ConfiguracionTenant();
            config.SmtpPassword = null; // nunca se re-renderiza la password almacenada
            return View(config);
        }

        // POST: Configuracion/DatosNegocio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatosNegocio(ConfiguracionTenant model)
        {
            if (!ModelState.IsValid)
                return View(model);

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

        // GET: Configuracion/EstadoCrear
        public ActionResult EstadoCrear()
        {
            return View(new EstadoTurno { Color = "#6c757d" });
        }

        // POST: Configuracion/EstadoCrear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EstadoCrear(EstadoTurno model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.CrearEstado(model.Nombre, model.Color);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Configuracion/EstadoEditar/5
        public ActionResult EstadoEditar(int id)
        {
            var estado = _service.GetEstado(id);

            if (estado == null)
                return HttpNotFound();

            return View(estado);
        }

        // POST: Configuracion/EstadoEditar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EstadoEditar(EstadoTurno model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.ActualizarEstado(model.Id, model.Nombre, model.Color);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Configuracion/EstadoEliminar/5
        public ActionResult EstadoEliminar(int id)
        {
            var estado = _service.GetEstado(id);

            if (estado == null)
                return HttpNotFound();

            return View(estado);
        }

        // POST: Configuracion/EstadoEliminar/5
        [HttpPost]
        [ActionName("EstadoEliminar")]
        [ValidateAntiForgeryToken]
        public ActionResult EstadoEliminarConfirmed(int id)
        {
            _service.EliminarEstado(id);
            return RedirectToAction("Index");
        }
    }
}