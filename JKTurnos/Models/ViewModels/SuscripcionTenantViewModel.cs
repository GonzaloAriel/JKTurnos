using System;

namespace JKTurnos.Models.ViewModels
{
    /// <summary>
    /// AG-5: estado de suscripcion del tenant para mostrar (solo lectura).
    /// El server entrega todo calculado: la vista no aplica reglas de negocio.
    /// </summary>
    public class SuscripcionTenantViewModel
    {
        // Identificacion del tenant (grilla del Admin Global, AG-7)
        public int TenantId { get; set; }

        public string Nombre { get; set; }

        public string Subdominio { get; set; }

        // Constante de PlanesSuscripcion: ACTIVO/POR VENCER/EN GRACIA/VENCIDO/SIN PLAN/SUSPENDIDO
        public string Estado { get; set; }

        public bool SuspendidoManual { get; set; }

        public bool IsActive { get; set; }

        public string PlanCodigo { get; set; }

        public string PlanNombre { get; set; }

        public DateTime? FechaInicioPeriodo { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        // Negativo cuando ya vencio; null cuando no hay plan.
        public int? DiasRestantes { get; set; }

        // Texto listo para mostrar ("N dias restantes", "Vencido hace N dias", etc.).
        public string TiempoRestanteTexto { get; set; }
    }
}
