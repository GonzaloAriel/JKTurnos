using System;

namespace JKTurnos.Models.Entities
{
    // D-037 / AG-0 punto 5: pago de SUSCRIPCION de plataforma. Es distinto de
    // Pago (D-028, pago de turno). Registrar = confirmado: sin estados pendientes.
    // Registro financiero: SIN edicion ni baja en v1 (una correccion = nuevo pago
    // ajustativo). Entidad de PLATAFORMA: no implementa ITenantEntity y se accede
    // exclusivamente via GlobalRepository (D-022). Sin navegaciones: TenantId y
    // UsuarioId autocontenidos (patron Pago.UsuarioId) para que el registro
    // sobreviva a bajas logicas sin crear FKs.
    public class PagoSuscripcion : BaseEntity
    {
        public int TenantId { get; set; }

        // Codigo del plan pagado (PlanesSuscripcion). Se denormaliza en el pago:
        // el historial conserva el plan de CADA periodo aunque el tenant cambie despues.
        public string PlanCodigo { get; set; }

        public decimal Monto { get; set; }

        // Fecha del pago en hora local (misma convencion que Turnos).
        public DateTime Fecha { get; set; }

        // Referencia / comprobante / nota en un solo campo libre (AG-0, max 500).
        public string Observacion { get; set; }

        // AdminGlobal que registra el pago (auditoria autocontenida).
        public int UsuarioId { get; set; }
    }
}
