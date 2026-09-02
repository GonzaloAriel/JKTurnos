using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;
using System.Collections.Generic;

namespace JKTurnos.Controllers
{
    /// <summary>
    /// M-08: Portal publico de reservas por subdominio. Hereda de Controller (NO de
    /// BaseController) y es [AllowAnonymous]: corre sin sesion, fuera de la topologia
    /// [Authorize]. El tenant se resuelve SOLO por host (X-Forwarded-Host / Host) y es
    /// SOLO contexto de navegacion, JAMAS un mecanismo de autorizacion. Si el tenant
    /// no es valido o no tiene el portal habilitado, se muestra una vista neutra 404.
    /// </summary>
    [AllowAnonymous]
    public class PortalController : Controller
    {
        private readonly PortalReservaService _service;

        public PortalController()
        {
            _service = new PortalReservaService();
        }

        private string RequestHost
        {
            get
            {
                // Proxy inverso (IIS/Azure): el primer X-Forwarded-Host es el host original.
                var fwd = Request.Headers["X-Forwarded-Host"];
                return !string.IsNullOrWhiteSpace(fwd)
                    ? fwd
                    : Request.Url.Host;
            }
        }

        private TenantContext ResolveTenant()
        {
            var tenant = _service.ResolveTenantByHost(RequestHost);

            if (tenant == null)
                return new TenantContext { Ok = false, IsDisponible = false };

            var disponible = _service.PortalDisponible(tenant);
            var config = _service.GetConfiguracion(tenant.Id);

            return new TenantContext
            {
                Ok = true,
                IsDisponible = disponible,
                TenantId = tenant.Id,
                Subdominio = tenant.Subdominio,
                NegocioNombre = config != null ? config.NombreComercial : tenant.Nombre,
                NegocioLogo = config != null && !string.IsNullOrWhiteSpace(config.LogoUrl)
                    ? config.LogoUrl
                    : (config != null ? config.AvatarUrl : null),
                NegocioTelefono = config != null ? config.Telefono : null,
                NegocioDireccion = config != null ? config.Direccion : null
            };
        }

        // GET: /Portal (o el subdominio). Paso 1: elegir recurso.
        public ActionResult Inicio()
        {
            var ctx = ResolveTenant();

            if (!ctx.Ok || !ctx.IsDisponible)
                return View("NoDisponible", ctx);

            var model = new PortalViewModel
            {
                TenantId = ctx.TenantId,
                NegocioNombre = ctx.NegocioNombre,
                NegocioLogo = ctx.NegocioLogo,
                NegocioTelefono = ctx.NegocioTelefono,
                NegocioDireccion = ctx.NegocioDireccion,
                Subdominio = ctx.Subdominio,
                Recursos = _service.GetRecursos(ctx.TenantId)
            };

            return View(model);
        }

        // POST: elegir recurso -> paso 2 (fecha/hora)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ElegirRecurso(PortalViewModel model)
        {
            var ctx = ResolveTenant();
            if (!ctx.Ok || !ctx.IsDisponible)
                return View("NoDisponible", ctx);

            if (!model.RecursoId.HasValue)
            {
                ModelState.AddModelError("", "Elegi un recurso para continuar.");
                model.TenantId = ctx.TenantId;
                model.NegocioNombre = ctx.NegocioNombre;
                model.NegocioLogo = ctx.NegocioLogo;
                model.NegocioTelefono = ctx.NegocioTelefono;
                model.NegocioDireccion = ctx.NegocioDireccion;
                model.Recursos = _service.GetRecursos(ctx.TenantId);
                return View("Inicio", model);
            }

            model.TenantId = ctx.TenantId;
            model.Fecha = DateTime.Today.ToString("yyyy-MM-dd");
            model.FechaHoraSeleccionada = null;

            return View("FechaHora", model);
        }

        // POST: elegir fecha -> lista de slots libres para esa fecha.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Slots(PortalViewModel model)
        {
            var ctx = ResolveTenant();
            if (!ctx.Ok || !ctx.IsDisponible)
                return View("NoDisponible", ctx);

            var fecha = ParseFecha(model.Fecha);

            if (!model.RecursoId.HasValue || fecha == null)
            {
                ModelState.AddModelError("", "Elegi un recurso y una fecha valida.");
                return View("Inicio", BuildInicioModel(ctx, model));
            }

            model.TenantId = ctx.TenantId;
            model.FechaHoraSeleccionada = fecha.Value;
            model.SlotsConsultados = true;
            model.SlotsLibres = _service.GetSlotsLibres(
                ctx.TenantId, model.RecursoId.Value, fecha.Value);
            model.FechaLegible = fecha.Value.ToString("dd/MM/yyyy");

            return View("FechaHora", model);
        }

        // POST: elegir hora -> paso 3 (datos del cliente)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ElegirHora(PortalViewModel model)
        {
            var ctx = ResolveTenant();
            if (!ctx.Ok || !ctx.IsDisponible)
                return View("NoDisponible", ctx);

            var fecha = ParseFecha(model.Fecha);

            if (string.IsNullOrWhiteSpace(model.HoraSeleccionada) || fecha == null)
            {
                model.TenantId = ctx.TenantId;
                model.SlotsLibres = fecha != null
                    ? _service.GetSlotsLibres(ctx.TenantId, model.RecursoId.Value, fecha.Value)
                    : new List<string>();
                return View("FechaHora", model);
            }

            model.TenantId = ctx.TenantId;
            model.FechaHoraSeleccionada = fecha.Value.Date.Add(ParseHora(model.HoraSeleccionada));
            model.FechaHoraLegible = model.FechaHoraSeleccionada.Value.ToString("EEEE, dd/MM/yyyy 'a las' HH:mm");

            return View("Datos", model);
        }

        // POST: datos del cliente -> crear reserva + link WhatsApp (paso 4).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirmar(PortalViewModel model)
        {
            var ctx = ResolveTenant();
            if (!ctx.Ok || !ctx.IsDisponible)
                return View("NoDisponible", ctx);

            var fecha = ParseFecha(model.Fecha);

            if (!model.RecursoId.HasValue || fecha == null
                || string.IsNullOrWhiteSpace(model.HoraSeleccionada))
            {
                ModelState.AddModelError("", "Faltan datos de la reserva.");
                return View("Datos", model);
            }

            if (string.IsNullOrWhiteSpace(model.Nombre) || string.IsNullOrWhiteSpace(model.Telefono))
            {
                ModelState.AddModelError("", "Completá tu nombre y telefono para reservar.");
                return View("Datos", model);
            }

            var fechaHora = fecha.Value.Date.Add(ParseHora(model.HoraSeleccionada));

            try
            {
                var turno = _service.CrearReserva(
                    ctx.TenantId,
                    model.RecursoId.Value,
                    fechaHora,
                    model.Nombre,
                    model.Telefono,
                    model.Email,
                    model.Observaciones);

                model.ReservaCreada = true;
                model.TurnoId = turno.Id;
                model.FechaHoraSeleccionada = fechaHora;
                model.FechaHoraLegible = fechaHora.ToString("EEEE, dd/MM/yyyy 'a las' HH:mm");
                model.RecursoNombre = _service.GetRecursos(ctx.TenantId)
                    .FirstOrDefault(r => r.Id == turno.RecursoId)?.Nombre;
                model.WhatsAppLink = _service.GenerarLinkConfirmacion(ctx.TenantId, turno.Id);
                model.NegocioNombre = ctx.NegocioNombre;
                model.NegocioLogo = ctx.NegocioLogo;

                return View("Confirmacion", model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Datos", model);
            }
        }

        private PortalViewModel BuildInicioModel(TenantContext ctx, PortalViewModel model)
        {
            model.TenantId = ctx.TenantId;
            model.NegocioNombre = ctx.NegocioNombre;
            model.NegocioLogo = ctx.NegocioLogo;
            model.NegocioTelefono = ctx.NegocioTelefono;
            model.NegocioDireccion = ctx.NegocioDireccion;
            model.Subdominio = ctx.Subdominio;
            model.Recursos = _service.GetRecursos(ctx.TenantId);
            return model;
        }

        /// <summary>Parsea una fecha "yyyy-MM-dd" (valor del input type=date) a DateTime.</summary>
        private static DateTime? ParseFecha(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha))
                return null;

            var partes = fecha.Split('-');
            if (partes.Length < 3
                || !TryParseExact(partes[0], out int a)
                || !TryParseExact(partes[1], out int m)
                || !TryParseExact(partes[2], out int d))
                return null;

            try
            {
                return new DateTime(a, m, d);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Parsea una hora "HH:mm" a un intervalo de minutos desde medianoche.</summary>
        private static TimeSpan ParseHora(string hora)
        {
            var partes = (hora ?? "").Split(':');
            if (partes.Length < 2 || !TryParseExact(partes[0], out int h) || !TryParseExact(partes[1], out int m))
                return TimeSpan.Zero;

            return TimeSpan.FromMinutes(h * 60 + m);
        }

        private static bool TryParseExact(string texto, out int valor)
        {
            valor = 0;

            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return int.TryParse(texto.Trim(), out valor);
        }

        /// <summary>Contexto del tenant resuelto por host para las vistas publicas.</summary>
        public class TenantContext
        {
            public bool Ok { get; set; }
            public bool IsDisponible { get; set; }
            public int TenantId { get; set; }
            public string Subdominio { get; set; }
            public string NegocioNombre { get; set; }
            public string NegocioLogo { get; set; }
            public string NegocioTelefono { get; set; }
            public string NegocioDireccion { get; set; }
            public bool NoExiste => Ok == false;
            public bool NoHabilitado => Ok && !IsDisponible;
        }
    }
}