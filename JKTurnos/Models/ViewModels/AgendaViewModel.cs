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

        public int? OpenRecursoId { get; set; }
    }
}