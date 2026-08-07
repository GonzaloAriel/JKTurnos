using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace JKTurnos.Models.ViewModels
{
    public class TurnoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El servicio es obligatorio.")]
        [Display(Name = "Servicio")]
        public int ServicioId { get; set; }

        [Required(ErrorMessage = "El recurso es obligatorio.")]
        [Display(Name = "Recurso")]
        public int RecursoId { get; set; }

        [Display(Name = "Estado")]
        public int EstadoTurnoId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de inicio son obligatorias.")]
        [Display(Name = "Inicio")]
        [DataType(DataType.DateTime)]
        public DateTime FechaHoraInicio { get; set; }

        [Display(Name = "Fin")]
        [DataType(DataType.DateTime)]
        public DateTime FechaHoraFin { get; set; }

        [Display(Name = "Observaciones")]
        [DataType(DataType.MultilineText)]
        public string Observaciones { get; set; }

        // Listas para los dropdowns
        public IEnumerable<SelectListItem> Clientes { get; set; }
        public IEnumerable<SelectListItem> Servicios { get; set; }
        public IEnumerable<SelectListItem> Recursos { get; set; }
        public IEnumerable<SelectListItem> EstadosTurno { get; set; }

        public TurnoViewModel()
        {
            Clientes = new List<SelectListItem>();
            Servicios = new List<SelectListItem>();
            Recursos = new List<SelectListItem>();
            EstadosTurno = new List<SelectListItem>();
            FechaHoraInicio = DateTime.Now;
        }
    }
}
