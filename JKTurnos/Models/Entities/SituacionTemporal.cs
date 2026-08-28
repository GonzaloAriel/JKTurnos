using System;
using System.Linq;

namespace JKTurnos.Models.Entities
{
    /// <summary>
    /// P7 (D-040, contrato P2 seccion 4): SITUACION TEMPORAL del turno, CALCULADA (no se persiste).
    /// Es la interpretacion del reloj sobre el estado persistido. Regla fundamental:
    /// "el reloj informa, el operador decide" - el reloj NUNCA muta el estado
    /// (nunca Pendiente-&gt;En curso automatico por hora, ni En curso-&gt;Finalizado por hora).
    ///
    /// 3 claves estables y limpias (se elimina 'horario-iniciado', redundante con 'demorado'):
    ///   "demorado"       : Pendiente y ya paso la hora de inicio (sin iniciar).
    ///   "tiempo-excedido": En curso y ya paso la hora de fin.
    ///   "a-tiempo"       : Pendiente o En curso en orden (sin alarmas).
    /// Estados terminales (Finalizado/Cancelado) = estado resolutivo: sin situacion temporal (null).
    /// </summary>
    public static class SituacionTemporal
    {
        public const string Demorado = "demorado";
        public const string TiempoExcedido = "tiempo-excedido";
        public const string ATiempo = "a-tiempo";

        /// <summary>Catálogo de las claves con su display (clave estable + Label/Color/Icono).</summary>
        public static readonly SituacionTemporalInfo[] Todas =
        {
            new SituacionTemporalInfo(Demorado,       "Demorado",        "#fd7e14", "bi-hourglass-split"),
            new SituacionTemporalInfo(TiempoExcedido, "Tiempo excedido", "#dc3545", "bi-alarm"),
            new SituacionTemporalInfo(ATiempo,        "A tiempo",        "#198754", "bi-check-circle")
        };

        /// <summary>
        /// Calcula la clave de situacion temporal segun (estado persistido, reloj).
        /// Usa turno.EstadoTurno.Codigo (semantica por Codigo estable, D-040). Retorna null
        /// si el turno es null, no trae estado, o su estado es terminal (Finalizado/Cancelado).
        /// </summary>
        public static string Calcular(Turno turno, DateTime? ahora = null)
        {
            if (turno == null)
                return null;

            var now = ahora ?? DateTime.Now;
            var codigo = turno.EstadoTurno != null ? turno.EstadoTurno.Codigo : null;

            if (codigo == EstadosTurnoBase.Pendiente)
                return now >= turno.FechaHoraInicio ? Demorado : ATiempo;

            if (codigo == EstadosTurnoBase.EnCurso)
                return now >= turno.FechaHoraFin ? TiempoExcedido : ATiempo;

            return null;
        }

        /// <summary>True si la clave es una alarma que requiere atencion del operador.</summary>
        public static bool EsAlarma(string clave)
        {
            return clave == Demorado || clave == TiempoExcedido;
        }

        /// <summary>Devuelve el item de display de una clave (o null si no existe).</summary>
        public static SituacionTemporalInfo De(string clave)
        {
            return Todas.FirstOrDefault(x => x.Clave == clave);
        }
    }

    /// <summary>Item de display de una clave de situacion temporal (Clave estable + Label/Color/Icono).</summary>
    public class SituacionTemporalInfo
    {
        public SituacionTemporalInfo(string clave, string label, string color, string icono)
        {
            Clave = clave;
            Label = label;
            Color = color;
            Icono = icono;
        }

        public string Clave { get; }
        public string Label { get; }
        public string Color { get; }
        public string Icono { get; }
    }
}
