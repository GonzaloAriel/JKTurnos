using System.ComponentModel.DataAnnotations;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    // V3: formulario inline de registro de pago dentro del Drawer (D-003).
    public class PagoViewModel
    {
        public int TurnoId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public TipoPago TipoPago { get; set; }

        [StringLength(250)]
        public string Observacion { get; set; }
    }
}