using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Controllers;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    public class RecursosController : BaseController
    {
        private readonly RecursoService _service;
        private readonly RecursoServicioService _recursoServicioService;
        private readonly ServicioService _servicioService;

        public RecursosController()
        {
            _service = new RecursoService();
            _recursoServicioService = new RecursoServicioService();
            _servicioService = new ServicioService();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuickCreate(
            [Bind(Exclude = "Id,TenantId,CreatedAt,UpdatedAt,IsActive")] Recurso model)
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
                nombre = model.Nombre,
                color = model.Color ?? "#2563EB"
            });
        }

        public ActionResult Index(string busqueda = null, TipoRecurso? tipo = null, bool incluirEliminados = false)
        {
            // Ver eliminados es exclusivo del Admin del tenant.
            if (!IsAdmin)
                incluirEliminados = false;

            var recursos = _service.GetFiltrados(busqueda, tipo, incluirEliminados);
            ViewBag.Busqueda = busqueda;
            ViewBag.Tipo = tipo;
            ViewBag.IncluirEliminados = incluirEliminados;
            ViewBag.Tipos = Enum.GetValues(typeof(TipoRecurso))
                .Cast<TipoRecurso>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString()
                })
                .ToList();
            return View(recursos);
        }

        public ActionResult Create()
        {
            var model = new RecursoViewModel
            {
                Recurso = new Recurso(),
                TodosLosServicios = _servicioService.GetAll().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Nombre })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RecursoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TodosLosServicios = _servicioService.GetAll().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Nombre });
                return View(model);
            }

            _service.Create(model.Recurso);
            _recursoServicioService.SetServiciosParaRecurso(model.Recurso.Id, model.ServicioIds, TenantId);
            
            return RedirectToAction("Index");
        }

        [Authorize(Roles = Roles.Admin)]
        public ActionResult Edit(int id)
        {
            var recurso = _service.GetById(id);
            if (recurso == null) return HttpNotFound();

            var model = new RecursoViewModel
            {
                Recurso = recurso,
                ServicioIds = _recursoServicioService.GetServicioIdsPorRecurso(id),
                TodosLosServicios = _servicioService.GetAll().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Nombre })
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult Edit(RecursoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TodosLosServicios = _servicioService.GetAll().Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Nombre });
                return View(model);
            }

            _service.Update(model.Recurso);
            _recursoServicioService.SetServiciosParaRecurso(model.Recurso.Id, model.ServicioIds, TenantId);

            return RedirectToAction("Index");
        }
// ... (Delete methods remain the same)

        [Authorize(Roles = Roles.Admin)]
        public ActionResult Delete(int id)
        {
            var recurso = _service.GetById(id);
            if (recurso == null) return HttpNotFound();
            return View(recurso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult DeleteConfirmed(int id)
        {
            _service.Delete(id);
            return RedirectToAction("Index");
        }

        // POST: Recursos/Restaurar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult Restaurar(int id)
        {
            try
            {
                _service.Restaurar(id);
            }
            catch (ArgumentException)
            {
            }

            return RedirectToAction("Index");
        }
    }
}
