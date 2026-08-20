using System.Web.Mvc;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // Notificaciones del sistema (in-app): feed de actividad del tenant.
    // El feed se lee desde BaseController (ViewBag.TopbarNotificaciones);
    // aqui solo viven las acciones de usuario.
    [Authorize]
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
            return Json(new { success = true });
        }
    }
}