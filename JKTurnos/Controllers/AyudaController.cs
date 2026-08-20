using System.Web.Mvc;

namespace JKTurnos.Controllers
{
    // Seccion Ayuda (FAQ): centro de ayuda estatico, sin datos de BD.
    // Acceso a todos los usuarios autenticados (BaseController ya es [Authorize]).
    public class AyudaController : BaseController
    {
        // GET: Ayuda
        public ActionResult Index()
        {
            return View();
        }
    }
}