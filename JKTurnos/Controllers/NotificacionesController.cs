using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // Notificaciones del sistema (in-app): feed de actividad del tenant.
    // El feed se lee desde BaseController (ViewBag.TopbarNotificaciones);
    // aqui solo viven las acciones de usuario.
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    public class NotificacionesController : BaseController
    {
        private readonly NotificacionSistemaService _service;

        public NotificacionesController()
        {
            _service = new NotificacionSistemaService();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MarcarTodasLeidas()
        {
            _service.MarcarTodasLeidas();

            // "Marcar todas leidas" tambien oculta los recordatorios actuales
            // de la campanita (persistido por claves hasta que cambie su
            // condicion), para que el badge de la campanita se limpie.
            new RecordatorioService().OcultarActuales(incluirConfiguracion: IsAdmin);

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Descartar(string clave)
        {
            new RecordatorioService().OcultarUna(clave);
            return Json(new { success = true });
        }
    }
}