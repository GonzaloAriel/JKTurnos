using System;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    // V3: item de historial de pagos del Drawer.
    public class PagoItemViewModel
    {
        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; }

        public TipoPago TipoPago { get; set; }

        public string Observacion { get; set; }

        public string UsuarioNombre { get; set; }
    }
}