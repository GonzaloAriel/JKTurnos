using System.Collections.Generic;
using System.Linq;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    public class HomeViewModel
    {
        public int TurnosHoy { get; set; }

        public decimal IngresosHoy { get; set; }

        public int Ocupacion { get; set; }

        public int Clientes { get; set; }

        public List<Turno> TurnosDelDia { get; set; }

        // M-09: checklist de primeros pasos (configuracion progresiva). La card
        // del Command Center solo aparece mientras haya pasos pendientes.
        public List<SetupStepVM> SetupSteps { get; set; }

        public int SetupPendientes
        {
            get
            {
                if (SetupSteps == null)
                    return 0;
                return SetupSteps.Count(s => !s.Completo);
            }
        }
    }

    // M-09: un paso de la puesta en marcha del negocio. Clave identifica el paso
    // (recurso/servicio/horarios/logo/datos) y Url es el destino "Crear/Configurar".
    public class SetupStepVM
    {
        public string Clave { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Url { get; set; }
        public string Icono { get; set; }
        public bool Completo { get; set; }
    }
}
