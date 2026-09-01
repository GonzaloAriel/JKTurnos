using System.Collections.Generic;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    // M-03: ficha/CRM del cliente. Contiene los datos de contacto del cliente
    // y su historial de turnos. El controller deriva el ultimo/proximo turno y
    // el enlace de WhatsApp a partir de estos datos.
    public class ClienteFichaViewModel
    {
        public Cliente Cliente { get; set; }

        public List<Turno> Turnos { get; set; } = new List<Turno>();

        public Turno UltimoTurno { get; set; }

        public Turno ProximoTurno { get; set; }

        public int TotalTurnos { get; set; }

        public int TurnosFinalizados { get; set; }

        public string WhatsappLink { get; set; }
    }
}
