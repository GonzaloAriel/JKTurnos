using System;
using System.ComponentModel.DataAnnotations;

namespace JKTurnos.Models.ViewModels
{
    /// <summary>
    /// AG-8: formulario de registro de pago de suscripcion de plataforma.
    /// </summary>
    public class RegistrarPagoViewModel
    {
        [Display(Name = "Tenant")]
        [Required(ErrorMessage = "Debe seleccionar un tenant.")]
        public int TenantId { get; set; }

        [Display(Name = "Plan")]
        [Required(ErrorMessage = "Debe seleccionar un plan.")]
        public string PlanCodigo { get; set; }

        [Display(Name = "Monto")]
        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
        public decimal Monto { get; set; }

        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        [Display(Name = "Observacion")]
        [MaxLength(500, ErrorMessage = "La observacion no puede superar 500 caracteres.")]
        public string Observacion { get; set; }
    }
}
