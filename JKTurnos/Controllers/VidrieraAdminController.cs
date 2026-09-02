using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // D-042 / Posta 5: panel de administracion de la Vidriera del tenant.
    // Backoffice protegido (Admin). Gestiona la fila VidrieraPublica del tenant:
    // slug, publicacion (Habilitada), contenido manual y flags de visibilidad.
    [Authorize(Roles = Roles.Admin)]
    public class VidrieraAdminController : BaseController
    {
        private readonly VidrieraService _service;

        public VidrieraAdminController()
        {
            _service = new VidrieraService();
        }

        // GET: VidrieraAdmin
        // Muestra el formulario de creacion/edicion de la vidriera del tenant.
        public ActionResult Index()
        {
            var vm = _service.GetVidrieraAdmin(TenantId);

            if (vm == null)
            {
                // Primera vez: el tenant aun no tiene vidriera. Se crea al guardar.
                vm = new VidrieraAdminViewModel
                {
                    YaExiste = false,
                    MostrarServicios = true,
                    MostrarPrecios = true,
                    MostrarDisponibilidad = true,
                    MostrarOfertas = true,
                    MostrarMensaje = true
                };
            }

            return View(vm);
        }

        // POST: VidrieraAdmin/Guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(VidrieraAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var error = _service.GuardarVidriera(TenantId, model);

            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View("Index", model);
            }

            // Recargar para refrescar YaExiste y UrlPublica.
            return RedirectToAction("Index");
        }
    }
}
