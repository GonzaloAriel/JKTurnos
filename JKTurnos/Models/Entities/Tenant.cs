using System;

namespace JKTurnos.Models.Entities
{
    public class Tenant : BaseEntity
    {
        public string Nombre { get; set; }

        public string Subdominio { get; set; }

        // D-037 / AG-0: suscripcion de plataforma como campos planos sobre Tenant
        // (sin entidad Subscription en v1). El historial contractual son los pagos.
        // Null = SIN PLAN (tenants nuevos y preexistentes).

        /// <summary>Codigo del ultimo plan pagado (PlanesSuscripcion). Null = Sin Plan.</summary>
        public string PlanCodigo { get; set; }

        public DateTime? FechaInicioPeriodo { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        // D-038: suspension SOLO manual (informativa). Distinta de IsActive (baja logica).
        public bool SuspendidoManual { get; set; }
    }
}