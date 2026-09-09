using System.Collections.Generic;

namespace JKTurnos.Models.Entities
{
    // D-050 / M-01 (Concepto B, evolucion): horario de trabajo por dia con
    // MULTIPLES rangos (turno partido: "08-12 y 17-20"). Persistido como JSON
    // en ConfiguracionTenant.HorarioAtencion. Informativo y no bloqueante.
    // Formato: [{"Dia":1,"Rangos":[{"Inicio":"08:00","Fin":"12:00"}]}]
    // Dia: 1=Lun .. 7=Dom. Un dia sin entrada no se atiende.
    public class HorarioAtencionDia
    {
        public int Dia { get; set; }

        public List<RangoAtencion> Rangos { get; set; } = new List<RangoAtencion>();
    }

    public class RangoAtencion
    {
        // Horas "HH:mm" (se mantienen como string para un JSON legible y
        // compatible con inputs type=time; se validan al normalizar).
        public string Inicio { get; set; }

        public string Fin { get; set; }
    }
}