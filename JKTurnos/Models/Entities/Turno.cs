using System;

namespace JKTurnos.Models.Entities
{
    public class Turno : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        public int ClienteId { get; set; }

        public int? ServicioId { get; set; }

        public int RecursoId { get; set; }

        public int EstadoTurnoId { get; set; }

        public DateTime FechaHoraInicio { get; set; }

        public DateTime FechaHoraFin { get; set; }

        public string Observaciones { get; set; }

        public virtual Cliente Cliente { get; set; }

        public virtual Servicio Servicio { get; set; }

        public virtual EstadoTurno EstadoTurno { get; set; }

        public virtual Recurso Recurso { get; set; }
    }
}