using System.Web.Mvc;
using JKTurnos.Controllers;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize]
    public class RecursosController : BaseController
    {
        private readonly RecursoService _service;

        public RecursosController()
        {
            _service = new RecursoService();
        }

        public ActionResult Index()
        {
            var recursos = _service.GetAll();
            return View(recursos);
        }

        public ActionResult Create()
        {
            return View(new Recurso());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Recurso model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _service.Create(model);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var recurso = _service.GetById(id);
            if (recurso == null) return HttpNotFound();
            return View(recurso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Recurso model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _service.Update(model);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var recurso = _service.GetById(id);
            if (recurso == null) return HttpNotFound();
            return View(recurso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _service.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
