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

        public List<Turno> GetFiltrados(string busqueda = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _repo.GetAll()
                             .Include(t => t.Cliente)
                             .Include(t => t.Servicio)
                             .Include(t => t.Recurso)
                             .Include(t => t.EstadoTurno);

            var desde = DateTime.Today;
            var hasta = DateTime.Today.AddDays(1);

            if (fechaInicio.HasValue || fechaFin.HasValue)
            {
                desde = (fechaInicio ?? fechaFin).Value.Date;
                hasta = (fechaFin ?? fechaInicio).Value.Date.AddDays(1);
            }

            query = query.Where(t => t.FechaHoraInicio >= desde
                                     && t.FechaHoraInicio < hasta);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim();

                query = query.Where(t =>
                    (t.Cliente != null && t.Cliente.Nombre.Contains(termino)) ||
                    (t.Servicio != null && t.Servicio.Nombre.Contains(termino)) ||
                    (t.Recurso != null && t.Recurso.Nombre.Contains(termino)));
            }

            return query.OrderBy(t => t.FechaHoraInicio).ToList();
        }

        public List<Turno> GetTurnosDelDia(DateTime fecha)
        {
            var inicioDelDia = fecha.Date;
            var finDelDia = inicioDelDia.AddDays(1);

            return _repo.GetAll()
                        .Include(t => t.Cliente)
                        .Include(t => t.Servicio)
                        .Include(t => t.Recurso)
                        .Include(t => t.EstadoTurno)
                        .Where(t => t.FechaHoraInicio < finDelDia && t.FechaHoraFin > inicioDelDia)
                        .OrderBy(t => t.FechaHoraInicio)
                        .ToList();
        }

        public List<Turno> GetTurnosEnRango(DateTime desde, DateTime hasta)
        {
            var inicio = desde.Date;
            var fin = hasta.Date.AddDays(1);

            return _repo.GetAll()
                        .Include(t => t.Cliente)
                        .Include(t => t.Servicio)
                        .Include(t => t.Recurso)
                        .Include(t => t.EstadoTurno)
                        .Where(t => t.FechaHoraInicio < fin && t.FechaHoraFin > inicio)
                        .OrderBy(t => t.FechaHoraInicio)
                        .ToList();
        }

        public DateTime CalcularFin(DateTime inicio, int? servicioId)
        {
            if (!servicioId.HasValue)
                return inicio.AddMinutes(30); // Duración por defecto si no hay servicio

            var servicio = _servicioRepo.GetById(servicioId.Value);
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
            if (turno.FechaHoraInicio < DateTime.Now)
                throw new InvalidOperationException(
                    "La fecha y hora del turno no puede ser anterior a la fecha y hora actual.");

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

            if (turno.EstadoTurnoId == 0)
                throw new InvalidOperationException(
                    "No hay estados de turno configurados. Contacte al administrador.");

            _repo.Add(turno);

            new NotificacionSistemaService().RegistrarTurnoCreado(turno.Id);
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

            var estadoNombre = _estadoRepo.GetById(estadoTurnoId)?.Nombre;
            new NotificacionSistemaService().RegistrarCambioEstado(turnoId, estadoNombre);
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

            new NotificacionSistemaService().RegistrarCambioEstado(turnoId, "Cancelado");
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}
