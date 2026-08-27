using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.AdminGlobal)]
    public class PagosSuscripcionController : BaseController
    {
        private readonly PagoSuscripcionService _service;
        private readonly TenantService _tenantService;

        public PagosSuscripcionController()
        {
            _service = new PagoSuscripcionService();
            _tenantService = new TenantService();
        }

        // GET: PagosSuscripcion
        public ActionResult Index(int? tenantId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            List<PagoSuscripcion> pagos;

            if (tenantId.HasValue)
            {
                pagos = _service.GetByTenant(tenantId.Value);
            }
            else
            {
                pagos = _service.GetAll();
            }

            if (fechaDesde.HasValue)
            {
                var desde = fechaDesde.Value.Date;
                pagos = pagos.Where(p => p.Fecha.Date >= desde).ToList();
            }

            if (fechaHasta.HasValue)
            {
                var hasta = fechaHasta.Value.Date;
                pagos = pagos.Where(p => p.Fecha.Date <= hasta).ToList();
            }

            var vm = pagos.Select(p =>
            {
                var tenant = _tenantService.GetById(p.TenantId);
                PlanSuscripcion plan = null;
                try { plan = PlanesSuscripcion.Obtener(p.PlanCodigo); }
                catch { }

                return new PagoSuscripcionItemViewModel
                {
                    Id = p.Id,
                    TenantId = p.TenantId,
                    TenantNombre = tenant != null ? tenant.Nombre : "Tenant #" + p.TenantId,
                    PlanCodigo = p.PlanCodigo,
                    PlanNombre = plan != null ? plan.Nombre : p.PlanCodigo,
                    Monto = p.Monto,
                    Fecha = p.Fecha,
                    Observacion = p.Observacion
                };
            }).ToList();

            ViewBag.Tenants = new SelectList(_tenantService.GetAll(), "Id", "Nombre", tenantId);
            ViewBag.FiltroTenantId = tenantId;
            ViewBag.FiltroFechaDesde = fechaDesde.HasValue ? fechaDesde.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.FiltroFechaHasta = fechaHasta.HasValue ? fechaHasta.Value.ToString("yyyy-MM-dd") : "";

            return View(vm);
        }

        // GET: PagosSuscripcion/Registrar
        public ActionResult Registrar(int? tenantId)
        {
            var model = new RegistrarPagoViewModel
            {
                TenantId = tenantId ?? 0,
                Fecha = DateTime.Today,
                Monto = 0m,
                // C4: la clave de idempotencia se genera UNA vez por formulario y viaja
                // oculta para que el mismo form reenviado no duplique el pago.
                IdempotencyKey = Guid.NewGuid().ToString("N")
            };

            ViewBag.Tenants = new SelectList(_tenantService.GetAll(), "Id", "Nombre", tenantId);
            ViewBag.Planes = new SelectList(PlanesSuscripcion.Todos, "Codigo", "Nombre");

            return View(model);
        }

        // POST: PagosSuscripcion/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(RegistrarPagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tenants = new SelectList(_tenantService.GetAll(), "Id", "Nombre", model.TenantId);
                ViewBag.Planes = new SelectList(PlanesSuscripcion.Todos, "Codigo", "Nombre", model.PlanCodigo);
                return View(model);
            }

            try
            {
                // C4: sella la fecha a hoy (ignora el campo Fecha del form) y reutiliza
                // la clave de idempotencia que viaja oculta desde el GET, para no
                // duplicar el pago ante un reenvio (doble submit / retry).
                var fechaSellada = DateTime.Today;
                _service.Registrar(model.TenantId, model.PlanCodigo, model.Monto, fechaSellada, model.Observacion, UserId, model.IdempotencyKey);
                TempData["Success"] = "Pago registrado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Tenants = new SelectList(_tenantService.GetAll(), "Id", "Nombre", model.TenantId);
                ViewBag.Planes = new SelectList(PlanesSuscripcion.Todos, "Codigo", "Nombre", model.PlanCodigo);
                return View(model);
            }
        }
    }
}
