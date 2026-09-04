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
    }
}