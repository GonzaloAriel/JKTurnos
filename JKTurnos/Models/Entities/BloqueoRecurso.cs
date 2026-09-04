using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JKTurnos.Models.Entities
{
    // D-046 / M-01: tipo de recurrencia de un BloqueoRecurso (modelo hibrido).
    // RecurrenciaMensual queda a v2.
    public enum TipoBloqueo
    {
        // Un dia concreto (FechaInicio; dia completo o franja horaria opcional).
        Unico,

        // Periodo continuo FechaInicio..FechaFin (extremas inclusivas).
        RangoFechas,

        // Dia de semana fijo: se usa el DayOfWeek de FechaInicio como "dia fijo".
        RecurrenteSemanal
    }

    // D-046 / M-01: indisponibilidad de un Recurso (Concepto A). Se comporta como
    // OCCUPADO dentro de TurnoService.ValidarDisponibilidad (segunda fuente de
    // ocupacion junto a los turnos NO Cancelados). Solo el rol Admin lo gestiona;
    // los operadores solo ven el bloque gris en la agenda.
    public class BloqueoRecurso : BaseEntity, ITenantEntity
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public int RecursoId { get; set; }

        [Required]
        public TipoBloqueo Tipo { get; set; }

        // Unico: dia puntual. RangoFechas: inicio del rango. RecurrenteSemanal: se
        // usa solo el DayOfWeek de esta fecha como "dia fijo de la semana".
        [Required]
        public DateTime FechaInicio { get; set; }

        // Solo para RangoFechas (extremo inclusivo). Null en Unico / RecurrenteSemanal.
        public DateTime? FechaFin { get; set; }

        // Franja horaria OPCIONAL dentro del dia. Ambas null = todo el dia.
        public TimeSpan? HoraInicio { get; set; }

        public TimeSpan? HoraFin { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [ForeignKey("RecursoId")]
        public virtual Recurso Recurso { get; set; }
    }
}
