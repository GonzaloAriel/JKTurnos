using System.Web.Mvc;
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

            var success = _authService.Login(
                model.Email,
                model.Password);

            if (!success)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                return View(model);
            }

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