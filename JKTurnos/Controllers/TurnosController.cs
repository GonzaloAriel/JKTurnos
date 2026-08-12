using System;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;
using JKTurnos.Data;

namespace JKTurnos.Controllers
{
    [Authorize]
    public class TurnosController : BaseController
    {
        private readonly TurnoService _turnoService;
        private readonly ClienteService _clienteService;
        private readonly ServicioService _servicioService;
        private readonly RecursoService _recursoService;
        private readonly RecursoServicioService _recursoServicioService;
        private readonly BaseRepository<EstadoTurno> _estadoRepo;

        public TurnosController()
        {
            _turnoService = new TurnoService();
            _clienteService = new ClienteService();
            _servicioService = new ServicioService();
            _recursoService = new RecursoService();
            _recursoServicioService = new RecursoServicioService();
            _estadoRepo = new BaseRepository<EstadoTurno>();
        }

        public JsonResult GetRecursosPorServicio(int servicioId)
        {
            var recursoIds = _recursoServicioService.GetRecursoIdsPorServicio(servicioId);
            var recursos = _recursoService.GetAll()
                .Where(r => recursoIds.Contains(r.Id))
                .Select(r => new { r.Id, r.Nombre });
            return Json(recursos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetServiciosPorRecurso(int recursoId)
        {
            var servicioIds = _recursoServicioService.GetServicioIdsPorRecurso(recursoId);
            var servicios = _servicioService.GetAll()
                .Where(s => servicioIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Nombre });
            return Json(servicios, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Agenda()
        {
            var hoy = DateTime.Today;

            var model = new AgendaViewModel
            {
                Fecha = hoy,
                Recursos = _recursoService.GetAll(),
                Turnos = _turnoService.GetTurnosDelDia(hoy),
                Estados = _estadoRepo.GetAll().ToList()
            };

            var dayNames = new[] { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };
            var monthNames = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            ViewBag.CurrentDateDisplay = dayNames[(int)hoy.DayOfWeek] + " " + hoy.Day + " " + monthNames[hoy.Month - 1] + ", " + hoy.Year;

            return View(model);
        }

        // GET: Turnos
        public ActionResult Index()
        {
            var turnos = _turnoService.GetAll();
            return View(turnos);
        }

        // GET: Turnos/Create
        public ActionResult Create()
        {
            var model = new TurnoViewModel();
            PopulateDropdowns(model);
            return View(model);
        }

        // POST: Turnos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TurnoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            var turno = new Turno
            {
                ClienteId = model.ClienteId,
                ServicioId = model.ServicioId,
                RecursoId = model.RecursoId,
                EstadoTurnoId = model.EstadoTurnoId,
                FechaHoraInicio = model.FechaHoraInicio,
                FechaHoraFin = model.FechaHoraFin,
                Observaciones = model.Observaciones
            };

            _turnoService.Crear(turno);

            return RedirectToAction("Index");
        }

        // GET: Turnos/Edit/5
        public ActionResult Edit(int id)
        {
            var turno = _turnoService.GetById(id);
            if (turno == null)
                return HttpNotFound();

            var model = new TurnoViewModel
            {
                Id = turno.Id,
                ClienteId = turno.ClienteId,
                ServicioId = turno.ServicioId,
                RecursoId = turno.RecursoId,
                EstadoTurnoId = turno.EstadoTurnoId,
                FechaHoraInicio = turno.FechaHoraInicio,
                FechaHoraFin = turno.FechaHoraFin,
                Observaciones = turno.Observaciones
            };

            PopulateDropdowns(model);
            return View(model);
        }

        // POST: Turnos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TurnoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            var turno = _turnoService.GetById(model.Id);
            if (turno == null)
                return HttpNotFound();

            turno.ClienteId = model.ClienteId;
            turno.ServicioId = model.ServicioId;
            turno.RecursoId = model.RecursoId;
            turno.EstadoTurnoId = model.EstadoTurnoId;
            turno.FechaHoraInicio = model.FechaHoraInicio;
            turno.FechaHoraFin = model.FechaHoraFin;
            turno.Observaciones = model.Observaciones;

            _turnoService.Update(turno);

            return RedirectToAction("Index");
        }

        // GET: Turnos/Delete/5
        public ActionResult Delete(int id)
        {
            var turno = _turnoService.GetById(id);
            if (turno == null)
                return HttpNotFound();

            return View(turno);
        }

        // POST: Turnos/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _turnoService.Delete(id);
            return RedirectToAction("Index");
        }

        private void PopulateDropdowns(TurnoViewModel model)
        {
            model.Clientes = _clienteService.GetAll().Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Nombre // Asumiendo que tiene propiedad Nombre
            });

            model.Servicios = _servicioService.GetAll().Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Nombre // Asumiendo que tiene propiedad Nombre
            });

            model.Recursos = _recursoService.GetAll().Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Nombre // Asumiendo que tiene propiedad Nombre
            });

            model.EstadosTurno = _estadoRepo.GetAll().Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Nombre
            });
        }
    }
}
