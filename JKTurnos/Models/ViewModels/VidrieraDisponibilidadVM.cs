using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    // D-042 / Posta 4: disponibilidad publica de la Vidriera, POR RECURSO.
    // Solo expone el nombre del recurso y los horarios libres (hoy, jornada
    // 08:00-19:30). Nunca expone Id, TenantId ni datos internos de Turno.
    public class VidrieraDisponibilidadVM
    {
        public string RecursoNombre { get; set; }

        // Horas libres de 30 min de hoy, formato "HH:mm" (ascendente).
        public List<string> SlotsLibres { get; set; } = new List<string>();

        // Conveniencia para la vista: true si no hay slots libres (estado vacio).
        public bool SinDisponibilidad => SlotsLibres.Count == 0;
    }
}
