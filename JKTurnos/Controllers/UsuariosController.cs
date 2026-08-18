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

            return View(user);
        }

        // POST: Usuarios/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                _service.Delete(id);
            }
            catch (Exception ex)
            {
                var user = _service.GetById(id);

                if (user == null)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", ex.Message);
                return View("Delete", user);
            }

            return RedirectToAction("Index");
        }
    }
}