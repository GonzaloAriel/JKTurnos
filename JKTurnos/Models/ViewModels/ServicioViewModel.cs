using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    public class ServicioViewModel
    {
        public Servicio Servicio { get; set; }

        public List<int> RecursoIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem> TodosLosRecursos { get; set; }
    }
}
