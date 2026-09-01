using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    // M-02: Resultado tipificado de la busqueda global.
    public class ResultadoBusqueda
    {
        public string Tipo { get; set; }
        public int Id { get; set; }
        public string Label { get; set; }
        public string Sub { get; set; }
        public string Url { get; set; }
    }

    // M-02: Busqueda global del topbar. Consulta Clientes, Servicios, Recursos
    // y Turnos por termino con limite, devolviendo sugerencias tipificadas para
    // que el front redirija al recurso/entidad correspondiente.
    public class BusquedaService
    {
        private readonly BaseRepository<Cliente> _clienteRepo;
        private readonly BaseRepository<Servicio> _servicioRepo;
        private readonly BaseRepository<Recurso> _recursoRepo;
        private readonly BaseRepository<Turno> _turnoRepo;

        public BusquedaService()
        {
            _clienteRepo = new BaseRepository<Cliente>();
            _servicioRepo = new BaseRepository<Servicio>();
            _recursoRepo = new BaseRepository<Recurso>();
            _turnoRepo = new BaseRepository<Turno>();
        }

        public List<ResultadoBusqueda> Buscar(string termino, int limite = 8)
        {
            var resultados = new List<ResultadoBusqueda>();

            if (string.IsNullOrWhiteSpace(termino))
                return resultados;

            var t = termino.Trim();

            foreach (var c in _clienteRepo.GetAll()
                .Where(x => x.Nombre.Contains(t) || x.Telefono != null && x.Telefono.Contains(t) || x.Email != null && x.Email.Contains(t))
                .OrderBy(x => x.Nombre)
                .Take(limite)
                .ToList())
            {
                resultados.Add(new ResultadoBusqueda
                {
                    Tipo = "cliente",
                    Id = c.Id,
                    Label = c.Nombre,
                    Sub = ContactoSub(c.Telefono, c.Email)
                });
            }

            foreach (var s in _servicioRepo.GetAll()
                .Where(x => x.Nombre.Contains(t) || (x.Descripcion != null && x.Descripcion.Contains(t)))
                .OrderBy(x => x.Nombre)
                .Take(limite)
                .ToList())
            {
                resultados.Add(new ResultadoBusqueda
                {
                    Tipo = "servicio",
                    Id = s.Id,
                    Label = s.Nombre,
                    Sub = s.DuracionMinutos + " min"
                });
            }

            foreach (var r in _recursoRepo.GetAll()
                .Where(x => x.Nombre.Contains(t) || (x.Descripcion != null && x.Descripcion.Contains(t)))
                .OrderBy(x => x.Nombre)
                .Take(limite)
                .ToList())
            {
                resultados.Add(new ResultadoBusqueda
                {
                    Tipo = "recurso",
                    Id = r.Id,
                    Label = r.Nombre,
                    Sub = r.Tipo.ToString()
                });
            }

            foreach (var tu in _turnoRepo.GetAll()
                .Include(x => x.Cliente)
                .Include(x => x.Servicio)
                .Include(x => x.Recurso)
                .Where(x => (x.Cliente != null && x.Cliente.Nombre.Contains(t)) ||
                            (x.Servicio != null && x.Servicio.Nombre.Contains(t)) ||
                            (x.Recurso != null && x.Recurso.Nombre.Contains(t)))
                .OrderByDescending(x => x.FechaHoraInicio)
                .Take(limite)
                .ToList())
            {
                var cliente = tu.Cliente != null ? tu.Cliente.Nombre : "Sin cliente";
                resultados.Add(new ResultadoBusqueda
                {
                    Tipo = "turno",
                    Id = tu.Id,
                    Label = cliente + " - " + tu.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                    Sub = tu.Recurso != null ? tu.Recurso.Nombre : ""
                });
            }

            return resultados;
        }

        private static string ContactoSub(string telefono, string email)
        {
            if (!string.IsNullOrWhiteSpace(telefono))
                return telefono;

            return !string.IsNullOrWhiteSpace(email) ? email : "";
        }
    }
}
