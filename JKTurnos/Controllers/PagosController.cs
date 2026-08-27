using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // Modulo Pagos: listado + filtros, registro, edicion y anulacion (baja logica con motivo).
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    public class PagosController : BaseController
    {
        private readonly PagoService _pagoService;

        public PagosController()
        {
            _pagoService = new PagoService();
        }

        public ActionResult Index(DateTime? fechaDesde = null, DateTime? fechaHasta = null, string busqueda = null)
        {
            var pagos = _pagoService.GetFiltrados(fechaDesde, fechaHasta, busqueda);
            var nombresUsuarios = _pagoService.GetNombresUsuarios(pagos.Select(p => p.UsuarioId));

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.Busqueda = busqueda;
            ViewBag.NombresUsuarios = nombresUsuarios;

            return View(pagos);
        }

        // Autocomplete del registro de pagos (D-030): busca por cliente/recurso,
        // devuelve top 20. Escala a cualquier volumen de turnos.
        public JsonResult BuscarTurnos(string termino = null)
        {
            var turnos = _pagoService.BuscarTurnos(termino, 20)
                .Select(t => new
                {
                    id = t.Id,
                    label = (t.Cliente != null ? t.Cliente.Nombre : "Sin cliente")
                        + " - " + (t.Recurso != null ? t.Recurso.Nombre : "")
                        + " " + t.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm")
                });

            return Json(turnos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarPago(PagoViewModel model)
        {
            if (model.TurnoId <= 0)
            {
                TempData["PagoError"] = "Selecciona un turno.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["PagoError"] = "El monto debe ser mayor a 0.";
                return RedirectToAction("Index");
            }

            try
            {
                _pagoService.RegistrarPago(model.TurnoId, model.Monto, model.TipoPago, model.Observacion, UserId);
                TempData["PagoOk"] = "Pago registrado.";
            }
            catch (Exception ex)
            {
                TempData["PagoError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Anular(int id, string motivo)
        {
            try
            {
                _pagoService.Anular(id, motivo);
                TempData["PagoOk"] = "Pago anulado.";
            }
            catch (Exception ex)
            {
                TempData["PagoError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPago(int id, string Monto, JKTurnos.Models.Entities.TipoPago? TipoPago, string Observacion)
        {
            decimal monto;
            bool montoValido =
                decimal.TryParse(Monto, NumberStyles.Number, CultureInfo.InvariantCulture, out monto)
                || decimal.TryParse(Monto, NumberStyles.Number, CultureInfo.CurrentCulture, out monto);

            if (!montoValido || monto <= 0)
            {
                TempData["PagoError"] = "El monto debe ser mayor a 0.";
                return RedirectToAction("Index");
            }

            try
            {
                _pagoService.ActualizarPago(id, monto, TipoPago.GetValueOrDefault(JKTurnos.Models.Entities.TipoPago.PagoParcial), Observacion);
                TempData["PagoOk"] = "Pago actualizado.";
            }
            catch (Exception ex)
            {
                TempData["PagoError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}