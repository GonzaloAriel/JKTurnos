using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;
using JKTurnos.Data;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    public class TurnosController : BaseController
    {
        private readonly TurnoService _turnoService;
        private readonly ClienteService _clienteService;
        private readonly ServicioService _servicioService;
        private readonly RecursoService _recursoService;
        private readonly RecursoServicioService _recursoServicioService;
        private readonly PagoService _pagoService;
        private readonly BaseRepository<EstadoTurno> _estadoRepo;

        public TurnosController()
        {
            _turnoService = new TurnoService();
            _clienteService = new ClienteService();
            _servicioService = new ServicioService();
            _recursoService = new RecursoService();
            _recursoServicioService = new RecursoServicioService();
            _pagoService = new PagoService();
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

        public JsonResult GetRecursos()
        {
            var recursos = _recursoService.GetAll()
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

        [HttpGet]
        public ActionResult GetDetailDrawer(int id)
        {
            var turno = _turnoService.GetById(id);
            if (turno == null)
                return HttpNotFound();

            var pagos = _pagoService.GetPagos(id);
            var precios = turno.Servicio != null ? turno.Servicio.Precio : (decimal?)null;
            var nombresUsuarios = _pagoService.GetNombresUsuarios(pagos.Select(p => p.UsuarioId));
            var notificacionService = new NotificacionService();

            var model = new TurnoDetailViewModel
            {
                Id = turno.Id,
                ClienteNombre = turno.Cliente != null ? turno.Cliente.Nombre : "Sin cliente",
                ClienteTelefono = turno.Cliente != null ? turno.Cliente.Telefono : null,
                ClienteEmail = turno.Cliente != null ? turno.Cliente.Email : null,
                ServicioNombre = turno.Servicio != null ? turno.Servicio.Nombre : null,
                ServicioDuracionMinutos = turno.Servicio != null ? turno.Servicio.DuracionMinutos : (int?)null,
                ServicioPrecio = precios,
                RecursoNombre = turno.Recurso != null ? turno.Recurso.Nombre : "Sin recurso",
                RecursoColor = turno.Recurso != null ? turno.Recurso.Color : null,
                EstadoTurnoId = turno.EstadoTurnoId,
                EstadoNombre = turno.EstadoTurno != null ? turno.EstadoTurno.Nombre : "Sin estado",
                EstadoColor = turno.EstadoTurno != null ? turno.EstadoTurno.Color : null,
                FechaHoraInicio = turno.FechaHoraInicio,
                FechaHoraFin = turno.FechaHoraFin,
                Observaciones = turno.Observaciones,
                WhatsAppLink = notificacionService.GenerarLinkWhatsApp(turno),
                PuedeEnviarEmail = turno.Cliente != null && !string.IsNullOrWhiteSpace(turno.Cliente.Email),
                EstadoEconomico = _pagoService.CalcularEstadoEconomico(pagos, precios),
                MontoAcumulado = pagos.Sum(p => p.Monto),
                Pagos = pagos.Select(p => new PagoItemViewModel
                {
                    Monto = p.Monto,
                    Fecha = p.Fecha,
                    TipoPago = p.TipoPago,
                    Observacion = p.Observacion,
                    UsuarioNombre = nombresUsuarios.ContainsKey(p.UsuarioId) ? nombresUsuarios[p.UsuarioId] : null
                }).ToList(),
                PuedeEditarPagos = CurrentRole == Roles.Admin || CurrentRole == Roles.User
            };

            return PartialView("~/Views/Shared/Components/_TurnoDetailDrawer.cshtml", model);
        }

        // V3: registra un pago/senia desde el Drawer (D-028). POST ajax, patrón QuickCreate (D-013).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarPago(PagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(m => m.Value.Errors.Count > 0)
                    .ToDictionary(m => m.Key, m => m.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault());

                return Json(new { success = false, errors = errors });
            }

            try
            {
                _pagoService.RegistrarPago(model.TurnoId, model.Monto, model.TipoPago, model.Observacion, UserId);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string> { { "", ex.Message } }
                });
            }

            return Json(new { success = true });
        }

        // D-033 Fase 2: envia el recordatorio por email al cliente desde el drawer.
        // POST ajax, patron QuickCreate (D-013). El boton aparece en el drawer solo
        // si el cliente tiene email (PuedeEnviarEmail).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnviarRecordatorioEmail(int turnoId)
        {
            try
            {
                new NotificacionService().EnviarEmail(turnoId);
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "No se pudo enviar el correo: " + ex.Message });
            }

            return Json(new { success = true, message = "Recordatorio enviado por email" });
        }

        public ActionResult Agenda(DateTime? fecha = null, string vista = null, int? openRecursoId = null, int? openTurnoId = null)
        {
            var hoy = DateTime.Today;
            var fechaSeleccionada = (fecha ?? hoy).Date;
            var vistaNormalizada = vista == "semana" ? "semana" : "dia";

            var dayNames = new[] { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };
            var monthNames = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            var model = new AgendaViewModel
            {
                Fecha = fechaSeleccionada,
                Vista = vistaNormalizada,
                Recursos = _recursoService.GetAll(),
                Estados = _estadoRepo.GetAll().ToList(),
                OpenRecursoId = openRecursoId
            };

            if (vistaNormalizada == "semana")
            {
                int diff = ((int)DayOfWeek.Monday - (int)fechaSeleccionada.DayOfWeek + 7) % 7;
                var lunes = fechaSeleccionada.AddDays(diff);
                var domingo = lunes.AddDays(6);

                model.Turnos = _turnoService.GetTurnosEnRango(lunes, domingo);

                if (lunes.Month == domingo.Month && lunes.Year == domingo.Year)
                {
                    ViewBag.CurrentDateDisplay = lunes.Day + " - " + domingo.Day + " " + monthNames[lunes.Month - 1] + ", " + lunes.Year;
                }
                else if (lunes.Year == domingo.Year)
                {
                    ViewBag.CurrentDateDisplay = lunes.Day + " " + monthNames[lunes.Month - 1] + " - " + domingo.Day + " " + monthNames[domingo.Month - 1] + ", " + domingo.Year;
                }
                else
                {
                    ViewBag.CurrentDateDisplay = lunes.Day + " " + monthNames[lunes.Month - 1] + " " + lunes.Year + " - " + domingo.Day + " " + monthNames[domingo.Month - 1] + " " + domingo.Year;
                }
            }
            else
            {
                model.Turnos = _turnoService.GetTurnosDelDia(fechaSeleccionada);
                ViewBag.CurrentDateDisplay = dayNames[(int)fechaSeleccionada.DayOfWeek] + " " + fechaSeleccionada.Day + " " + monthNames[fechaSeleccionada.Month - 1] + ", " + fechaSeleccionada.Year;
            }

            ViewBag.Vista = vistaNormalizada;
            ViewBag.FechaNavegacion = fechaSeleccionada;
            ViewBag.OpenTurnoId = openTurnoId;

            // Modelo de dropdowns para el drawer de Quick Create (D-017).
            var quickModel = new TurnoViewModel();
            PopulateDropdowns(quickModel);
            ViewBag.TurnoQuickModel = quickModel;

            return View(model);
        }

        // GET: Turnos
        public ActionResult Index(string busqueda = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var turnos = _turnoService.GetFiltrados(busqueda, fechaInicio, fechaFin);

            ViewBag.Busqueda = busqueda;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

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

            try
            {
                _turnoService.Crear(turno);

                if (turno.ServicioId.HasValue)
                    _recursoServicioService.Vincular(turno.ServicioId.Value, turno.RecursoId, TenantId);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                PopulateDropdowns(model);
                return View(model);
            }

            return RedirectToAction("Agenda", new { openRecursoId = model.RecursoId });
        }

        // D-017 QuickCreate: alta rapida de turno desde el drawer de la agenda
        // (click en celda libre). POST ajax, patron QuickCreate (D-013). La
        // fecha/hora y el recurso llegan prellenados por la celda; el fin se
        // calcula con la duracion del servicio.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuickCreate(TurnoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(m => m.Value.Errors.Count > 0)
                    .ToDictionary(m => m.Key, m => m.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault());

                return Json(new { success = false, errors = errors });
            }

            var turno = new Turno
            {
                ClienteId = model.ClienteId,
                ServicioId = model.ServicioId,
                RecursoId = model.RecursoId,
                FechaHoraInicio = model.FechaHoraInicio,
                Observaciones = model.Observaciones
            };

            try
            {
                _turnoService.Crear(turno);

                if (turno.ServicioId.HasValue)
                    _recursoServicioService.Vincular(turno.ServicioId.Value, turno.RecursoId, TenantId);
            }
            catch (InvalidOperationException ex)
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string> { { "", ex.Message } }
                });
            }

            return Json(new { success = true, id = turno.Id });
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

            try
            {
                _turnoService.Update(turno);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                PopulateDropdowns(model);
                return View(model);
            }

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
