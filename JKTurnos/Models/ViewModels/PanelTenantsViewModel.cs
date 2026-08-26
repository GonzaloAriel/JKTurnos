using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    /// <summary>
    /// AG-7: panel de operacion de Tenants del Admin Global (shell + grilla).
    /// Contadores derivados en server; la vista solo renderiza.
    /// </summary>
    public class PanelTenantsViewModel
    {
        public List<SuscripcionTenantViewModel> Tenants { get; set; }

        public int TotalActivos { get; set; }

        public int TotalPorVencer { get; set; }

        public int TotalEnGracia { get; set; }

        public int TotalVencidos { get; set; }

        public int TotalSuspendidos { get; set; }

        public int TotalSinPlan { get; set; }
    }
}
