using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    // D-048: una "pantalla" de la pizarra semanal de disponibilidad de la Vidriera.
    // Como el monitor es de alto fijo y no puede tener scroll, la lista de servicios
    // se parte en pantallas de N servicios que rotan solas cada 10 segundos. Cada
    // pantalla muestra los MISMOS 7 dias (hoy + 6) para su grupo de servicios.
    public class VidrieraPantallaVM
    {
        // Servicios (filas) que caben en esta pantalla.
        public List<VidrieraDisponibilidadSemanalVM> Servicios { get; set; } =
            new List<VidrieraDisponibilidadSemanalVM>();

        public bool SinDisponibilidad
        {
            get
            {
                foreach (var s in Servicios)
                    if (!s.SinDisponibilidad)
                        return false;
                return true;
            }
        }
    }
}
