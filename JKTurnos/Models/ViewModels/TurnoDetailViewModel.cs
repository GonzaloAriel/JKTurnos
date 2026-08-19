using System;
using System.Collections.Generic;

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

        public int EstadoTurnoId { get; set; }
        public string EstadoNombre { get; set; }
        public string EstadoColor { get; set; }

        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }

        public string Observaciones { get; set; }

        // V3: estado economico (senia/pago) calculado desde los Pagos.
        public string EstadoEconomico { get; set; }

        public List<PagoItemViewModel> Pagos { get; set; }
        public decimal MontoAcumulado { get; set; }
        public bool PuedeEditarPagos { get; set; }
    }
}