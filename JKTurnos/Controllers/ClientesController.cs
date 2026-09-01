using System;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    public class ClientesController : BaseController
    {
        private readonly ClienteService _service;
        private readonly TurnoService _turnoService;

        public ClientesController()
        {
            _service = new ClienteService();
            _turnoService = new TurnoService();
        }

        // GET: Clientes
        public ActionResult Index(string busqueda = null)
        {
            var clientes = _service.GetFiltrados(busqueda);
            ViewBag.Busqueda = busqueda;
            return View(clientes);
        }

        // GET: Clientes/Details/5 (M-03: ficha/CRM con historial de turnos)
        public ActionResult Details(int id)
        {
            var cliente = _service.GetById(id);

            if (cliente == null)
                return HttpNotFound();

            var turnos = _turnoService.GetTurnosPorCliente(id);
            var ahora = DateTime.Now;

            // Ultimo turno realizado (Finalizado) mas reciente.
            Turno ultimo = turnos
                .Where(t => t.EstadoTurno != null && t.EstadoTurno.Codigo == EstadosTurnoBase.Finalizado)
                .OrderByDescending(t => t.FechaHoraInicio)
                .FirstOrDefault();

            // Proximo turno: futuro, no cancelado, mas cercano.
            Turno proximo = turnos
                .Where(t => t.FechaHoraInicio >= ahora &&
                            (t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado))
                .OrderBy(t => t.FechaHoraInicio)
                .FirstOrDefault();

            var modelo = new ClienteFichaViewModel
            {
                Cliente = cliente,
                Turnos = turnos,
                UltimoTurno = ultimo,
                ProximoTurno = proximo,
                TotalTurnos = turnos.Count,
                TurnosFinalizados = turnos.Count(t =>
                    t.EstadoTurno != null && t.EstadoTurno.Codigo == EstadosTurnoBase.Finalizado),
                WhatsappLink = BuildWhatsappLink(cliente.Telefono)
            };

            return View(modelo);
        }

        private static string BuildWhatsappLink(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return null;

            // El enlace de WhatsApp requiere el numero solo con digitos (+ y 0-9).
            var digitos = new string(telefono.Where(c => char.IsDigit(c) || c == '+').ToArray());
            if (string.IsNullOrEmpty(digitos))
                return null;

            return "https://wa.me/" + digitos;
        }

        // GET: Clientes/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuickCreate(Cliente model)
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

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cliente model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _service.Create(model);

            return RedirectToAction("Index");
        }

        // GET: Clientes/Edit/5
        public ActionResult Edit(int id)
        {
            var cliente = _service.GetById(id);

            if (cliente == null)
                return HttpNotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Cliente model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _service.Update(model);

            return RedirectToAction("Index");
        }

        // GET: Clientes/Delete/5
        public ActionResult Delete(int id)
        {
            var cliente = _service.GetById(id);

            if (cliente == null)
                return HttpNotFound();

            return View(cliente);
        }

        // POST: Clientes/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _service.Delete(id);

            return RedirectToAction("Index");
        }
    }
}