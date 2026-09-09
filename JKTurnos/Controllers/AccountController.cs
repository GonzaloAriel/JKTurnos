using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;
using Microsoft.Owin.Security;
using System.Web;

namespace JKTurnos.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController()
        {
            _authService = new AuthService();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _authService.Login(
                model.Email,
                model.Password,
                out var esPrimerIngreso);

            if (user == null)
            {
                // C-2: Se devuelve el mismo mensaje para no revelar si el email existe.
                ModelState.AddModelError("", "Credenciales incorrectas o cuenta desactivada. Tras 5 intentos fallidos se bloquea el ingreso por 15 minutos.");
                return View(model);
            }

            if (user.Role == Roles.AdminGlobal)
                return RedirectToAction("Index", "AdminGlobalDashboard");

            // M-15: bienvenida solo en el primer ingreso del usuario tenant.
            if (esPrimerIngreso)
                TempData["PrimerIngreso"] = true;

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            new Services.AuditService().Registrar(
                Services.AuditService.A_Logout,
                Services.AuditService.E_Autenticacion,
                "Cierre de sesion");

            HttpContext
                .GetOwinContext()
                .Authentication
                .SignOut("ApplicationCookie");

            return RedirectToAction("Login");
        }
    }
}
