using System;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // D-046 / M-01: modulo de gestion de BloqueoRecurso (Concepto A). Solo rol Admin
    // crea/edita/elimina. Los operadores no ven estas vistas (solo los bloques grises
    // en la agenda).
    [Authorize(Roles = Roles.Admin)]
    public class BloqueosController : BaseController
    {
        private readonly BloqueoRecursoService _service;

        public BloqueosController()
        {
            _service = new BloqueoRecursoService();
        }

        public ActionResult Index()
        {
            var bloques = _service.GetAll();
            var recursos = _service.GetRecursos().ToDictionary(r => r.Id, r => r.Nombre);
            ViewBag.NombresRecursos = recursos;
            return View(bloques);
        }

        public ActionResult Create()
        {
            var model = new BloqueoRecursoViewModel
            {
                FechaInicio = DateTime.Today,
                TodoElDia = true,
                Recursos = RecursosDropdown()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BloqueoRecursoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Recursos = RecursosDropdown();
                return View(model);
            }

            var bloqueo = ToEntity(model);
            var colisiones = _service.TurnosColisionan(bloqueo.RecursoId, RangoInicio(bloqueo), RangoFin(bloqueo));
            _service.Create(bloqueo);

            if (colisiones.Count > 0)
            {
                var nombre = _service.GetRecursos()
                    .Where(r => r.Id == bloqueo.RecursoId)
                    .Select(r => r.Nombre)
                    .FirstOrDefault();
                var recursoLabel = nombre ?? ("recurso #" + bloqueo.RecursoId);
                TempData["BloqueoWarning"] =
                    "El bloqueo se creo, pero " + recursoLabel + " tiene " + colisiones.Count +
                    " turno(s) ya agendados en ese horario y el recurso no podra brindar el servicio. " +
                    "No se cancelo ningun turno: reagendalos manualmente si es necesario.";
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var bloqueo = _service.GetById(id);
            if (bloqueo == null) return HttpNotFound();

            var model = ToViewModel(bloqueo);
            model.Recursos = RecursosDropdown();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BloqueoRecursoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Recursos = RecursosDropdown();
                return View(model);
            }

            var bloqueo = ToEntity(model);
            _service.Update(bloqueo);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var bloqueo = _service.GetById(id);
            if (bloqueo == null) return HttpNotFound();
            return View(bloqueo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _service.Delete(id);
            return RedirectToAction("Index");
        }

        private SelectListItem[] RecursosDropdown()
        {
            return _service.GetRecursos()
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToArray();
        }

        private BloqueoRecurso ToEntity(BloqueoRecursoViewModel m)
        {
            return new BloqueoRecurso
            {
                Id = m.Id,
                RecursoId = m.RecursoId,
                Tipo = m.Tipo,
                FechaInicio = m.FechaInicio.Date,
                FechaFin = m.Tipo == TipoBloqueo.RangoFechas ? (m.FechaFin.HasValue ? m.FechaFin.Value.Date : (DateTime?)null) : null,
                HoraInicio = m.TodoElDia ? (TimeSpan?)null : m.HoraInicio,
                HoraFin = m.TodoElDia ? (TimeSpan?)null : m.HoraFin,
                Descripcion = m.Descripcion
            };
        }

        private BloqueoRecursoViewModel ToViewModel(BloqueoRecurso b)
        {
            return new BloqueoRecursoViewModel
            {
                Id = b.Id,
                RecursoId = b.RecursoId,
                Tipo = b.Tipo,
                FechaInicio = b.FechaInicio.Date,
                FechaFin = b.FechaFin,
                HoraInicio = b.HoraInicio,
                HoraFin = b.HoraFin,
                TodoElDia = !b.HoraInicio.HasValue && !b.HoraFin.HasValue,
                Descripcion = b.Descripcion
            };
        }

        private DateTime RangoInicio(BloqueoRecurso b)
        {
            return b.FechaInicio.Date;
        }

        private DateTime RangoFin(BloqueoRecurso b)
        {
            if (b.Tipo == TipoBloqueo.RangoFechas && b.FechaFin.HasValue)
                return b.FechaFin.Value.Date.AddDays(1);
            return b.FechaInicio.Date.AddDays(1);
        }
    }
}
