using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize]
    public class ServiciosController : BaseController
    {
        private readonly ServicioService _service;
        private readonly RecursoService _recursoService;
        private readonly RecursoServicioService _recursoServicioService;

        public ServiciosController()
        {
            _service = new ServicioService();
            _recursoService = new RecursoService();
            _recursoServicioService = new RecursoServicioService();
        }

        private void PopulateRecursos(ServicioViewModel model)
        {
            var recursos = _recursoService.GetAll();
            model.TodosLosRecursos = recursos
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre });
            ViewBag.RecursoColors = recursos.ToDictionary(r => r.Id, r => r.Color ?? "#2563EB");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuickCreate(Servicio model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(m => m.Value.Errors.Count > 0)
                    .ToDictionary(m => m.Key, m => m.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault());

                return Json(new { success = false, errors = errors });
            }

            _service.Create(model);

            return Json(new
            {
                success = true,
                id = model.Id,
                nombre = model.Nombre
            });
        }

        public ActionResult Index(string busqueda = null)
        {
            var servicios = _service.GetFiltrados(busqueda);
            ViewBag.Busqueda = busqueda;
            return View(servicios);
        }

        public ActionResult Create()
        {
            var model = new ServicioViewModel { Servicio = new Servicio() };
            PopulateRecursos(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServicioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateRecursos(model);
                return View(model);
            }

            _service.Create(model.Servicio);
            _recursoServicioService.SetRecursosParaServicio(model.Servicio.Id, model.RecursoIds, TenantId);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var servicio = _service.GetById(id);

            if (servicio == null)
                return HttpNotFound();

            var model = new ServicioViewModel
            {
                Servicio = servicio,
                RecursoIds = _recursoServicioService.GetRecursoIdsPorServicio(id)
            };
            PopulateRecursos(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServicioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateRecursos(model);
                return View(model);
            }

            _service.Update(model.Servicio);
            _recursoServicioService.SetRecursosParaServicio(model.Servicio.Id, model.RecursoIds, TenantId);

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