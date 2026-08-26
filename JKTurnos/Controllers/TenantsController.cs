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
        public ActionResult Index()
        {
            return View(_service.GetPanelTenants());
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

        // GET: Tenants/Delete/5
        public ActionResult Delete(int id)
        {
            var tenant = _service.GetById(id);

            if (tenant == null)
                return HttpNotFound();

            return View(tenant);
        }

        // POST: Tenants/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _service.Delete(id);

            return RedirectToAction("Index");
        }

        // POST: Tenants/Desactivar/5 (D-038: suspension manual e informativa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Desactivar(int id)
        {
            _service.Desactivar(id);

            return RedirectToAction("Index");
        }

        // POST: Tenants/Activar/5 (D-038: quita la suspension; no toca fechas ni pagos)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activar(int id)
        {
            _service.Activar(id);

            return RedirectToAction("Index");
        }
    }
}