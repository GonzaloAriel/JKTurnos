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
                ModelState.AddModelError("", "Credenciales incorrectas o cuenta desactivada. Contacte al administrador.");
                return View(model);
            }

            if (user.Role == Roles.AdminGlobal)
                return RedirectToAction("Index", "AdminGlobalDashboard");

            // M-15: bienvenida solo en el primer ingreso del usuario tenant.
            if (esPrimerIngreso)
                TempData["PrimerIngreso"] = true;

            return RedirectToAction("Index", "Home");
        }

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
