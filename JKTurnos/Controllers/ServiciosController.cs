using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize]
    public class ServiciosController : BaseController
    {
        private readonly ServicioService _service;

        public ServiciosController()
        {
            _service = new ServicioService();
        }

        public ActionResult Index()
        {
            return View(_service.GetAll());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Servicio model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _service.Create(model);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var servicio = _service.GetById(id);

            if (servicio == null)
                return HttpNotFound();

            return View(servicio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Servicio model)
        {
            System.Diagnostics.Debug.WriteLine(model.CreatedAt);
            System.Diagnostics.Debug.WriteLine(model.TenantId);
            System.Diagnostics.Debug.WriteLine(model.IsActive);

            if (!ModelState.IsValid)
                return View(model);

            _service.Update(model);

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var servicio = _service.GetById(id);

            if (servicio == null)
                return HttpNotFound();

            return View(servicio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _service.Delete(id);

            return RedirectToAction("Index");
        }
    }
}