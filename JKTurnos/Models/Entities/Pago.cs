using System;

namespace JKTurnos.Models.Entities
{
    // D-028: Pago registrado sobre un Turno (Dominio Financiero, DER 2.1).
    // El estado economico del turno se calcula desde estos registros,
    // no se persiste en Turno.
    public class Pago : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        public int TurnoId { get; set; }

        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; }

        public TipoPago TipoPago { get; set; }

        public string Observacion { get; set; }

        // Responsable del registro (auditoria autocontenida; TurnoHistorial a V2).
        public int UsuarioId { get; set; }

        public virtual Turno Turno { get; set; }
    }
}