using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    public class BusquedaViewModel
    {
        public string Termino { get; set; }
        public int TotalResultados { get; set; }
        public List<Services.ResultadoBusqueda> Clientes { get; set; }
        public List<Services.ResultadoBusqueda> Servicios { get; set; }
        public List<Services.ResultadoBusqueda> Recursos { get; set; }
        public List<Services.ResultadoBusqueda> Turnos { get; set; }

        public BusquedaViewModel()
        {
            Clientes = new List<Services.ResultadoBusqueda>();
            Servicios = new List<Services.ResultadoBusqueda>();
            Recursos = new List<Services.ResultadoBusqueda>();
            Turnos = new List<Services.ResultadoBusqueda>();
        }
    }
}
