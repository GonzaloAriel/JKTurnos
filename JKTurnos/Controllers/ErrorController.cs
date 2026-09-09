using System.Web.Mvc;

namespace JKTurnos.Controllers
{
    // Paginas de error estandar (redirects de customErrors en produccion).
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult Interno()
        {
            Response.StatusCode = 500;
            return View("Error", new HandleErrorInfo(
                new System.Exception("Error interno"),
                "Error",
                "Interno"));
        }

        [AllowAnonymous]
        public ActionResult NoEncontrado()
        {
            Response.StatusCode = 404;
            return View("Error", new HandleErrorInfo(
                new System.Exception("Recurso no encontrado"),
                "Error",
                "NoEncontrado"));
        }
    }
}