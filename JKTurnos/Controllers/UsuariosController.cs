using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.AdminGlobal + "," + Roles.Admin)]
    public class UsuariosController : BaseController
    {
        private readonly UserService _service;
        private readonly TenantService _tenantService;

        public UsuariosController()
        {
            _service = new UserService();
            _tenantService = new TenantService();
        }

        private void PopulateDropdowns(UsuarioViewModel model)
        {
            if (IsAdminGlobal)
            {
                model.Tenants = _tenantService.GetAll()
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Nombre });

                model.Roles = new[] { Roles.Admin, Roles.User, Roles.AdminGlobal }
                    .Select(r => new SelectListItem { Value = r, Text = r });
            }
            else
            {
                model.Tenants = new List<SelectListItem>();

                model.Roles = new[] { Roles.User }
                    .Select(r => new SelectListItem { Value = r, Text = r });
            }
        }

        // GET: Usuarios
        public ActionResult Index(int? tenantId = null)
        {
            var usuarios = _service.GetAll(tenantId);
            ViewBag.TenantFiltro = tenantId;
            ViewBag.CurrentUserId = UserId;

            if (IsAdminGlobal)
            {
                var tenants = _tenantService.GetAll();
                ViewBag.Tenants = tenants
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Nombre });
                ViewBag.TenantNombres = tenants.ToDictionary(t => t.Id, t => t.Nombre);
            }

            return View(usuarios);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            var model = new UsuarioViewModel();
            PopulateDropdowns(model);
            return View(model);
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            var user = new User
            {
                Nombre = model.Nombre,
                Email = model.Email,
                Role = model.Role,
                TenantId = model.TenantId ?? 0
            };

            try
            {
                _service.Create(user, model.Password);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                PopulateDropdowns(model);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Usuarios/Edit/5
        public ActionResult Edit(int id)
        {
            var user = _service.GetById(id);

            if (user == null)
                return HttpNotFound();

            var model = new UsuarioViewModel
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                Role = user.Role,
                TenantId = user.TenantId
            };

            PopulateDropdowns(model);
            return View(model);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            var user = new User
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Email = model.Email,
                Role = model.Role,
                TenantId = model.TenantId ?? 0
            };

            try
            {
                _service.Update(user, model.Password);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                PopulateDropdowns(model);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int id)
        {
            var user = _service.GetById(id);

            if (user == null)
                return HttpNotFound();

            // Un Admin Global no puede ser eliminado (ni siquiera por otro Admin Global).
            if (user.Role == Roles.AdminGlobal)
            {
                TempData["Error"] = "El rol AdminGlobal no puede eliminarse para preservar la plataforma.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // POST: Usuarios/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = _service.GetById(id);
            if (user != null && user.Role == Roles.AdminGlobal)
            {
                ModelState.AddModelError("", "El rol AdminGlobal no puede eliminarse para preservar la plataforma.");
                return View("Delete", user);
            }

            try
            {
                _service.Delete(id);
            }
            catch (Exception ex)
            {
                var existingUser = _service.GetById(id);

                if (existingUser == null)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", ex.Message);
                return View("Delete", existingUser);
            }

            return RedirectToAction("Index");
        }

        // GET: Usuarios/ResetPassword/5
        public ActionResult ResetPassword(int id)
        {
            var user = _service.GetById(id);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // POST: Usuarios/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id, string password, string confirmar)
        {
            var user = _service.GetById(id);

            if (user == null)
                return HttpNotFound();

            if (id == UserId)
                ModelState.AddModelError("", "Usa 'Cambiar mi contrasena' para modificar tu propia contrasena.");

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "El password es obligatorio.");
            else if (password != confirmar)
                ModelState.AddModelError("confirmar", "Los passwords no coinciden.");

            if (!ModelState.IsValid)
                return View(user);

            try
            {
                _service.ResetPassword(id, password);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(user);
            }

            return RedirectToAction("Index");
        }

        // GET: Usuarios/MiPassword
        public ActionResult MiPassword()
        {
            return View();
        }

        // POST: Usuarios/MiPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MiPassword(string passwordActual, string nuevoPassword, string confirmar)
        {
            if (string.IsNullOrWhiteSpace(passwordActual))
                ModelState.AddModelError("passwordActual", "La contrasena actual es obligatoria.");

            if (string.IsNullOrWhiteSpace(nuevoPassword))
                ModelState.AddModelError("nuevoPassword", "El nuevo password es obligatorio.");
            else if (nuevoPassword != confirmar)
                ModelState.AddModelError("confirmar", "Los passwords no coinciden.");

            if (!ModelState.IsValid)
                return View();

            try
            {
                _service.CambiarMiPassword(UserId, passwordActual, nuevoPassword);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}