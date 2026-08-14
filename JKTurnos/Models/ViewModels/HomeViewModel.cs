using System.Collections.Generic;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    public class HomeViewModel
    {
        public int TurnosHoy { get; set; }

        public decimal IngresosHoy { get; set; }

        public int Ocupacion { get; set; }

        public int Clientes { get; set; }

        public List<Turno> TurnosDelDia { get; set; }
    }
}