using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    public class RecursoViewModel
    {
        public Recurso Recurso { get; set; }

        public List<int> ServicioIds { get; set; } = new List<int>();

        public IEnumerable<SelectListItem> TodosLosServicios { get; set; }
    }
}
