using System;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.AdminGlobal)]
    public class TenantsController : BaseController
    {
        private readonly TenantService _service;

        public TenantsController()
        {
            _service = new TenantService();
        }

        // GET: Tenants (AG-7: panel de operacion del Admin Global)
        public ActionResult Index(string busqueda = null, string estado = null)
        {
            ViewBag.Busqueda = busqueda;
            ViewBag.Estado = estado;

            return View(_service.GetPanelTenants(busqueda, estado));
        }

        // GET: Tenants/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tenants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tenant model, string adminNombre = null, string adminEmail = null, string adminPassword = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.Create(model, adminNombre, adminEmail, adminPassword);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Tenants/Edit/5
        public ActionResult Edit(int id)
        {
            var tenant = _service.GetById(id);

            if (tenant == null)
                return HttpNotFound();

            return View(tenant);
        }

        // POST: Tenants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tenant model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.Update(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        // GET: Tenants/Desactivar/5
        public ActionResult Desactivar(int id)
        {
            var tenant = _service.GetByIdInactive(id);

            if (tenant == null)
                return HttpNotFound();

            return View(tenant);
        }

        // POST: Tenants/DesactivarConfirmado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DesactivarConfirmado(int id)
        {
            _service.Delete(id);

            return RedirectToAction("Index");
        }

        // GET: Tenants/Reactivar/5
        public ActionResult Reactivar(int id)
        {
            var tenant = _service.GetByIdInactive(id);

            if (tenant == null)
                return HttpNotFound();

            return View(tenant);
        }

        // POST: Tenants/ReactivarConfirmado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReactivarConfirmado(int id)
        {
            _service.Reactivar(id);

            return RedirectToAction("Index");
        }
    }
}