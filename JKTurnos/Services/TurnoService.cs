using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    public class TurnoService
    {
        private readonly BaseRepository<Turno> _repo;
        private readonly BaseRepository<Servicio> _servicioRepo;
        private readonly BaseRepository<EstadoTurno> _estadoRepo;

        public TurnoService()
        {
            _repo = new BaseRepository<Turno>();
            _servicioRepo = new BaseRepository<Servicio>();
            _estadoRepo = new BaseRepository<EstadoTurno>();
        }

        public List<Turno> GetAll()
        {
            return _repo.GetAll()
                        .Include(t => t.Cliente)
                        .Include(t => t.Servicio)
                        .Include(t => t.Recurso)
                        .Include(t => t.EstadoTurno)
                        .OrderBy(x => x.FechaHoraInicio)
                        .ToList();
        }

        public Turno GetById(int id)
        {
            return _repo.GetAll()
                        .Include(t => t.Cliente)
                        .Include(t => t.Servicio)
                        .Include(t => t.Recurso)
                        .Include(t => t.EstadoTurno)
                        .FirstOrDefault(x => x.Id == id);
        }

        public DateTime CalcularFin(DateTime inicio, int servicioId)
        {
            var servicio = _servicioRepo.GetById(servicioId);
            if (servicio == null)
                throw new ArgumentException("Servicio no encontrado.");

            return inicio.AddMinutes(servicio.DuracionMinutos);
        }

        public bool ValidarDisponibilidad(int recursoId, DateTime inicio, DateTime fin, int? turnoExcluirId = null)
        {
            var query = _repo.GetAll()
                .Where(x => x.RecursoId == recursoId)
                .Where(x => x.FechaHoraInicio < fin && x.FechaHoraFin > inicio);

            if (turnoExcluirId.HasValue)
                query = query.Where(x => x.Id != turnoExcluirId.Value);

            return !query.Any();
        }

        public void Crear(Turno turno)
        {
            if (turno.FechaHoraFin == default)
                turno.FechaHoraFin = CalcularFin(turno.FechaHoraInicio, turno.ServicioId);

            if (!ValidarDisponibilidad(turno.RecursoId, turno.FechaHoraInicio, turno.FechaHoraFin))
                throw new InvalidOperationException(
                    "El recurso no esta disponible en el horario seleccionado.");

            if (turno.EstadoTurnoId == 0)
            {
                var pendiente = _estadoRepo.GetAll()
                    .FirstOrDefault(x => x.Nombre == "Pendiente");

                if (pendiente != null)
                    turno.EstadoTurnoId = pendiente.Id;
            }

            _repo.Add(turno);
        }

        public void Update(Turno turno)
        {
            turno.FechaHoraFin = CalcularFin(turno.FechaHoraInicio, turno.ServicioId);

            if (!ValidarDisponibilidad(turno.RecursoId, turno.FechaHoraInicio, turno.FechaHoraFin, turno.Id))
                throw new InvalidOperationException(
                    "El recurso no esta disponible en el horario seleccionado.");

            _repo.Update(turno);
        }

        public void CambiarEstado(int turnoId, int estadoTurnoId)
        {
            var turno = _repo.GetById(turnoId);
            if (turno == null)
                throw new ArgumentException("Turno no encontrado.");

            turno.EstadoTurnoId = estadoTurnoId;
            _repo.Update(turno);
        }

        public void Cancelar(int turnoId)
        {
            var turno = _repo.GetById(turnoId);
            if (turno == null)
                throw new ArgumentException("Turno no encontrado.");

            var cancelado = _estadoRepo.GetAll()
                .FirstOrDefault(x => x.Nombre == "Cancelado");

            if (cancelado == null)
                throw new InvalidOperationException(
                    "Estado Cancelado no encontrado. Ejecute la semilla primero.");

            turno.EstadoTurnoId = cancelado.Id;
            _repo.Update(turno);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}
