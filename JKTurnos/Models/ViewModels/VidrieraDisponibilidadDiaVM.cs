using System;

namespace JKTurnos.Models.ViewModels
{
    // D-048: un dia dentro de la matriz semanal de disponibilidad de la Vidriera
    // (agrupada por SERVICIO). Unifica los slots libres consecutivos de 30 min en
    // rangos legibles (ej. "09-11") y expone cuantos hay para el color de la celda.
    public class VidrieraDisponibilidadDiaVM
    {
        public DateTime Fecha { get; set; }

        // Nombre corto del dia ("LUN", "MAR", ...).
        public string NombreDia { get; set; }

        // Dia del mes (1-31).
        public int DiaDelMes { get; set; }

        // True si es el dia de hoy (se resalta en la vista).
        public bool EsHoy { get; set; }

        // Cantidad total de slots libres de 30 min (para el color/estado de la celda).
        public int CantidadSlots { get; set; }

        // Rangos de disponibilidad ("09-11", "14"). Vacio si no hay actividad.
        public System.Collections.Generic.List<string> Rangos { get; set; } =
            new System.Collections.Generic.List<string>();

        public bool SinDisponibilidad => CantidadSlots == 0;
    }
}
