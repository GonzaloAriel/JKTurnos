using System;
using System.Collections.Generic;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    public class AgendaViewModel
    {
        public DateTime Fecha { get; set; }

        public string Vista { get; set; }

        public IEnumerable<Recurso> Recursos { get; set; }

        public IEnumerable<Turno> Turnos { get; set; }

        // D-046 / M-01: bloques de indisponibilidad (grises) activos en la fecha/vista.
        public IEnumerable<BloqueoRecurso> Bloqueos { get; set; }

        public int? OpenRecursoId { get; set; }

        // Dias laborables del tenant (para filtrar columnas en vista semanal).
        public HashSet<DayOfWeek> DiasLaborales { get; set; }

        // D-050 / M-01 (evolucion): horario de atencion efectivo por dia (JSON ->
        // legacy -> default). Claves = dias con al menos un rango. La grilla lo usa
        // para marcar celdas fuera de horario como no clickeables (sin crear turno).
        public Dictionary<DayOfWeek, List<RangoAtencion>> HorarioAtencion { get; set; }
    }
}