using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    // D-046 / M-01: viewmodel de BloqueoRecurso. Separa el dia (FechaSeleccionada para
    // la UI) de la franja horaria opcional y del tipo; sobre el mismo se reconstruye la
    // entidad en el servicio/controller. Facilita el manejo de TimeSpan en los forms.
    public class BloqueoRecursoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecciona el recurso.")]
        public int RecursoId { get; set; }

        [Required(ErrorMessage = "Selecciona el tipo de bloqueo.")]
        public TipoBloqueo Tipo { get; set; }

        // Dia base (Unico) o inicio del rango (RangoFechas) o dia de la semana de
        // referencia (RecurrenteSemanal).
        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        // Extremo del rango (solo RangoFechas).
        public DateTime? FechaFin { get; set; }

        // Franja horaria opcional dentro del dia.
        public TimeSpan? HoraInicio { get; set; }

        public TimeSpan? HoraFin { get; set; }

        [StringLength(500, ErrorMessage = "La descripcion no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; }

        public bool TodoElDia { get; set; }

        // Dropdown de recursos para el form.
        public IEnumerable<SelectListItem> Recursos { get; set; }

        // Aviso amable de colision con turnos existentes (no se persiste).
        public List<Turno> TurnosColisionados { get; set; } = new List<Turno>();
    }
}
