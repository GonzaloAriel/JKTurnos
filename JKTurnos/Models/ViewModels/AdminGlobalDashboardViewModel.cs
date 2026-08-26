using System;
using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    public class AdminGlobalDashboardViewModel
    {
        public int TotalTenants { get; set; }
        public int TotalActivos { get; set; }
        public int TotalEnGracia { get; set; }
        public int TotalSuspendidos { get; set; }
        public int TotalPagosHoy { get; set; }
        public List<TenantAlertDTO> TenantsEnRiesgo { get; set; }
        public List<PagoSuscripcionDTO> UltimosPagos { get; set; }
    }

    public class TenantAlertDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Estado { get; set; }
    }

    public class PagoSuscripcionDTO
    {
        public string Tenant { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
    }
}
