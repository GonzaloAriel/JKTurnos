using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    // D-048 / M-13 (Vidriera, matriz semanal agrupada por SERVICIO). Disponibilidad
    // publica para los proximos 7 dias (hoy + 6). Cada fila es un SERVICIO que
    // agrega la disponibilidad de TODOS sus recursos asociados (un slot esta libre
    // si algun recurso del servicio lo tiene libre; se unifican sin repetir horas).
    // Nunca expone Id, TenantId ni datos internos de Turno/Bloqueo.
    public class VidrieraDisponibilidadSemanalVM
    {
        public string ServicioNombre { get; set; }

        // 7 dias ordenados (hoy + proximos 6).
        public List<VidrieraDisponibilidadDiaVM> Dias { get; set; } = new List<VidrieraDisponibilidadDiaVM>();

        // Conveniencia para la vista: true si ningun dia de la semana tiene actividad.
        public bool SinDisponibilidad
        {
            get
            {
                foreach (var d in Dias)
                    if (d.CantidadSlots > 0)
                        return false;
                return true;
            }
        }
    }
}
