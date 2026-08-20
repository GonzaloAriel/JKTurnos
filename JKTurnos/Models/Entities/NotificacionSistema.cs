using System;

namespace JKTurnos.Models.Entities
{
    // Notificaciones del sistema (in-app): feed de actividad del tenant.
    // Es distinta de las notificaciones al cliente (D-033, WhatsApp/email).
    // El feed es por tenant (una sola vista operativa); Leida es compartida.
    public class NotificacionSistema : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        // Tipo de evento: TurnoCreado, TurnoEstado, PagoRegistrado,
        // PagoAnulado, Email.
        public string Tipo { get; set; }

        public string Titulo { get; set; }

        public string Mensaje { get; set; }

        // Contexto para navegar desde el feed (null si no aplica):
        // "Turno" (abre Agenda + drawer) o "Pago" (abre listado Pagos).
        public string EntidadTipo { get; set; }

        public int? EntidadId { get; set; }

        public DateTime Fecha { get; set; }

        public bool Leida { get; set; }
    }
}