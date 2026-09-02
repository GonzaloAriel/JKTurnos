using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace JKTurnos.Models.ViewModels
{
    // V3: Drawer de detalle de Time Block con estado economico (D-028).
    // EstadoEconomico es el slot de V1; se puebla con el estado calculado
    // desde los Pagos del turno.
    public class TurnoDetailViewModel
    {
        public int Id { get; set; }

        public string ClienteNombre { get; set; }
        public string ClienteTelefono { get; set; }
        public string ClienteEmail { get; set; }

        public string ServicioNombre { get; set; }
        public int? ServicioDuracionMinutos { get; set; }
        public decimal? ServicioPrecio { get; set; }

        public string RecursoNombre { get; set; }
        public string RecursoColor { get; set; }

        // M-07: recursos del tenant para el dropdown de "Mover turno" del drawer
        // (preseleccionado con RecursoId actual del turno).
        public int RecursoId { get; set; }
        public IEnumerable<SelectListItem> Recursos { get; set; }

        public int EstadoTurnoId { get; set; }
        public string EstadoNombre { get; set; }
        public string EstadoColor { get; set; }
        public string EstadoIcono { get; set; }

        // D-040: acciones contextuales de estado habilitadas segun el Codigo actual
        // (P5): se muestran en el Drawer de detalle para operar sin ir al Edit.
        public bool PuedeIniciar { get; set; }
        public bool PuedeFinalizar { get; set; }
        public bool PuedeCancelar { get; set; }

        public string SituacionClave { get; set; }
        public string SituacionLabel { get; set; }
        public string SituacionColor { get; set; }
        public string SituacionIcono { get; set; }

        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }

        public string Observaciones { get; set; }

        // D-033: link wa.me con el recordatorio prellenado (null si el cliente no tiene telefono).
        public string WhatsAppLink { get; set; }

        // D-033 Fase 2: el cliente tiene email cargado (habilita el boton de envio por email).
        public bool PuedeEnviarEmail { get; set; }

        // V3: estado economico (senia/pago) calculado desde los Pagos.
        public string EstadoEconomico { get; set; }

        public List<PagoItemViewModel> Pagos { get; set; }
        public decimal MontoAcumulado { get; set; }
        public bool PuedeEditarPagos { get; set; }
    }
}
