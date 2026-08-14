using System;

namespace JKTurnos.Models.ViewModels
{
    // V1: Drawer de detalle de Time Block (solo lectura).
    // EstadoEconomico es un slot reservado para V3 (Docs/16): se renderiza
    // solo si tiene contenido. No existe modelo de pagos todavia.
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

        public int EstadoTurnoId { get; set; }
        public string EstadoNombre { get; set; }
        public string EstadoColor { get; set; }

        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }

        public string Observaciones { get; set; }

        // V3: estado economico (senia/pago). Null cuando no hay modelo todavia.
        public string EstadoEconomico { get; set; }
    }
}