using System;

namespace JKTurnos.Models.ViewModels
{
    /// <summary>
    /// AG-8: linea del historial de pagos de suscripcion (listado Admin Global).
    /// </summary>
    public class PagoSuscripcionItemViewModel
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantNombre { get; set; }
        public string PlanCodigo { get; set; }
        public string PlanNombre { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Observacion { get; set; }
    }
}
