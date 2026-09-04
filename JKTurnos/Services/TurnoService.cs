using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    public class TurnoService
    {
        private readonly BaseRepository<Turno> _repo;
        private readonly BaseRepository<Servicio> _servicioRepo;
        private readonly BaseRepository<EstadoTurno> _estadoRepo;
        private readonly BaseRepository<Cliente> _clienteRepo;
        private readonly TurnoEstadoMotor _turnoEstadoMotor;

        public TurnoService()
        {
            _repo = new BaseRepository<Turno>();
            _servicioRepo = new BaseRepository<Servicio>();
            _estadoRepo = new BaseRepository<EstadoTurno>();
            _clienteRepo = new BaseRepository<Cliente>();
            _turnoEstadoMotor = new TurnoEstadoMotor();
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

        public List<Turno> GetFiltrados(string busqueda = null, DateTime? fechaInicio = null, DateTime? fechaFin = null, List<string> estados = null)
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

            // Filtro por estados operativos (D-045): si viene la lista, solo se
            // muestran los turnos cuyo Codigo este incluido.
            if (estados != null && estados.Count > 0)
            {
                var codigos = estados
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Distinct()
                    .ToList();

                if (codigos.Count > 0)
                {
                    query = query.Where(t =>
                        t.EstadoTurno != null &&
                        codigos.Contains(t.EstadoTurno.Codigo));
                }
            }

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

        // M-03: historial completo del cliente (todos sus turnos, mas reciente
        // primero). Se usa en la ficha de detalle / CRM del cliente.
        public List<Turno> GetTurnosPorCliente(int clienteId)
        {
            return _repo.GetAll()
                        .Include(t => t.Cliente)
                        .Include(t => t.Servicio)
                        .Include(t => t.Recurso)
                        .Include(t => t.EstadoTurno)
                        .Where(t => t.ClienteId == clienteId)
                        .OrderByDescending(t => t.FechaHoraInicio)
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
            // D-042: el turno Cancelado libera su slot (queda disponible para otro
            // turno); no bloquea la disponibilidad. Solo los turnos operativos
            // (Pendiente/En curso/Finalizado) ocupan el horario.
            var query = _repo.GetAll()
                .Where(x => x.RecursoId == recursoId)
                .Where(x => x.FechaHoraInicio < fin && x.FechaHoraFin > inicio)
                .Where(x => x.EstadoTurno == null || x.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado);

            if (turnoExcluirId.HasValue)
                query = query.Where(x => x.Id != turnoExcluirId.Value);

            if (query.Any())
                return false;

            // D-046 / M-01 (Concepto A): un BloqueoRecurso es una segunda fuente de
            // ocupacion. No existe exclusion de bloqueo al crear/editar un turno.
            return !new BloqueoRecursoService().EstaBloqueado(recursoId, inicio, fin);
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
                    .FirstOrDefault(x => x.Codigo == EstadosTurnoBase.Pendiente);

                if (pendiente != null)
                {
                    turno.EstadoTurnoId = pendiente.Id;
                }
                else
                {
                    // Fallback: sembrar estados genericos inline para no bloquear
                    var tenantId = new TenantProvider().GetTenantId();
                    turno.EstadoTurnoId = SeedEstadosBase(tenantId);
                }
            }

            ValidarReferenciasTenant(turno);

            _repo.Add(turno);

            new NotificacionSistemaService().RegistrarTurnoCreado(turno.Id);

            new AuditService().Registrar(
                AuditService.A_Crear,
                AuditService.E_Turno,
                "Creo el turno de " + NombreCliente(turno.ClienteId) +
                " (" + NombreServicio(turno.ServicioId) + ") " +
                turno.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                turno.Id);
        }

        public void Update(Turno turno)
        {
            var existente = _repo.GetById(turno.Id);

            if (existente == null)
                throw new ArgumentException("Turno no encontrado.");

            turno.FechaHoraFin = CalcularFin(turno.FechaHoraInicio, turno.ServicioId);

            if (!ValidarDisponibilidad(turno.RecursoId, turno.FechaHoraInicio, turno.FechaHoraFin, turno.Id))
                throw new InvalidOperationException(
                    "El recurso no esta disponible en el horario seleccionado.");

            ValidarReferenciasTenant(turno);

            var cambios = CompararTurnos(existente, turno);

            _repo.Update(turno);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Turno,
                "Edito el turno de " + NombreCliente(turno.ClienteId) + " " +
                turno.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                turno.Id,
                cambios);
        }

        // M-07: mover un turno (drag & drop del timeblock de la agenda o "Mover
        // turno" desde el drawer). Recibe el destino calculado por la UI;
        // recalcula el fin con la duracion del servicio y valida disponibilidad
        // excluyendo el propio turno. Solo se mueven turnos operativos
        // (Pendiente/En curso); Finalizado y Cancelado son terminales (D-040).
        // No se admite mover al pasado (misma regla que Crear).
        public void Mover(int turnoId, int recursoId, DateTime nuevaFechaHoraInicio)
        {
            var turno = GetById(turnoId);

            if (turno == null)
                throw new ArgumentException("Turno no encontrado.");

            var estadoCodigo = turno.EstadoTurno != null ? turno.EstadoTurno.Codigo : null;
            if (estadoCodigo != EstadosTurnoBase.Pendiente && estadoCodigo != EstadosTurnoBase.EnCurso)
                throw new InvalidOperationException(
                    "Solo se pueden mover los turnos Pendientes o En curso.");

            if (turno.RecursoId == recursoId && turno.FechaHoraInicio == nuevaFechaHoraInicio)
                return;

            if (nuevaFechaHoraInicio < DateTime.Now)
                throw new InvalidOperationException(
                    "La fecha y hora del turno no puede ser anterior a la fecha y hora actual.");

            var nuevoFin = CalcularFin(nuevaFechaHoraInicio, turno.ServicioId);

            if (!ValidarDisponibilidad(recursoId, nuevaFechaHoraInicio, nuevoFin, turno.Id))
                throw new InvalidOperationException(
                    "El recurso no esta disponible en el horario seleccionado.");

            var desde = turno.FechaHoraInicio;

            turno.RecursoId = recursoId;
            turno.FechaHoraInicio = nuevaFechaHoraInicio;
            turno.FechaHoraFin = nuevoFin;

            ValidarReferenciasTenant(turno);

            _repo.Update(turno);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Turno,
                "Movio el turno de " + NombreCliente(turno.ClienteId) +
                " de " + desde.ToString("dd/MM/yyyy HH:mm") +
                " a " + nuevaFechaHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                turno.Id);
        }

        public void CambiarEstado(int turnoId, int estadoTurnoId)
        {
            _turnoEstadoMotor.CambiarEstado(turnoId, estadoTurnoId);
        }

        public void Cancelar(int turnoId)
        {
            _turnoEstadoMotor.Cancelar(turnoId);
        }

        // P6: la matriz completa (guardia de origen incluida) quedo centralizada en
        // TurnoEstadoMotor; estos metodos publicos de TurnoService delegan en el.
        public void Iniciar(int turnoId)
        {
            _turnoEstadoMotor.Iniciar(turnoId);
        }

        // P6: ver Iniciar. Delegacion al motor unico de transiciones.
        public void Finalizar(int turnoId)
        {
            _turnoEstadoMotor.Finalizar(turnoId);
        }

        public void Delete(int id)
        {
            var turno = _repo.GetById(id);

            var detalle = turno != null
                ? "Elimino el turno de " + NombreCliente(turno.ClienteId) +
                  " (" + NombreServicio(turno.ServicioId) + ") " +
                  turno.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm")
                : "Elimino el turno #" + id;

            _repo.Delete(id);

            new AuditService().Registrar(
                AuditService.A_Eliminar,
                AuditService.E_Turno,
                detalle,
                id);
        }

        /// <summary>
        /// Semilla de estados genericos inline como fallback (D-024/D-040).
        /// Siembra exactamente el catalogo fijo con su Codigo estable.
        /// Retorna el Id del estado "Pendiente".
        /// </summary>
        private int SeedEstadosBase(int tenantId)
        {
            var pendienteCatalog = EstadosTurnoBase.Todos
                .First(e => e.Codigo == EstadosTurnoBase.Pendiente);

            int pendienteId = 0;
            foreach (var e in EstadosTurnoBase.Todos)
            {
                _estadoRepo.Add(new EstadoTurno
                {
                    TenantId = tenantId,
                    Nombre = e.Nombre,
                    Codigo = e.Codigo,
                    Color = e.Color,
                    Icono = e.Icono,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });

                if (e.Codigo == EstadosTurnoBase.Pendiente)
                {
                    using (var context = new JKContext())
                    {
                        pendienteId = context.EstadosTurno
                            .Where(x => x.TenantId == tenantId && x.Codigo == EstadosTurnoBase.Pendiente)
                            .OrderByDescending(x => x.Id)
                            .Select(x => x.Id)
                            .FirstOrDefault();
                    }
                }
            }

            return pendienteId;
        }

        /// <summary>
        /// C1 (seguridad): valida que Cliente/Servicio/Recurso/EstadoTurno referenciados
        /// por el turno pertenezcan al tenant del usuario. Evita la inyeccion de FK
        /// referencial cross-tenant (turno apuntando a entidades de OTRO tenant), que
        /// antes filtraba datos ajenos via Include(...). Se consulta por TenantId
        /// (sin exigir IsActive) para no romper la edicion de turnos cuyas referencias
        /// fueron dadas de baja (soft delete).
        /// </summary>
        private void ValidarReferenciasTenant(Turno turno)
        {
            using (var context = new JKContext())
            {
                var tenantId = new TenantProvider().GetTenantId();

                if (turno.RecursoId <= 0 ||
                    !context.Recursos.Any(r => r.Id == turno.RecursoId && r.TenantId == tenantId))
                    throw new InvalidOperationException("El recurso seleccionado no es valido.");

                if (turno.ClienteId <= 0 ||
                    !context.Clientes.Any(c => c.Id == turno.ClienteId && c.TenantId == tenantId))
                    throw new InvalidOperationException("El cliente seleccionado no es valido.");

                if (turno.ServicioId.HasValue &&
                    !context.Servicios.Any(s => s.Id == turno.ServicioId.Value && s.TenantId == tenantId))
                    throw new InvalidOperationException("El servicio seleccionado no es valido.");

                if (turno.EstadoTurnoId <= 0 ||
                    !context.EstadosTurno.Any(e => e.Id == turno.EstadoTurnoId && e.TenantId == tenantId))
                    throw new InvalidOperationException("El estado de turno seleccionado no es valido.");
            }
        }

        private string NombreCliente(int clienteId)
        {
            var cliente = _clienteRepo.GetById(clienteId);
            return cliente != null ? cliente.Nombre : "#" + clienteId;
        }

        private string NombreServicio(int? servicioId)
        {
            if (!servicioId.HasValue)
                return "sin servicio";

            var servicio = _servicioRepo.GetById(servicioId.Value);
            return servicio != null ? servicio.Nombre : "#" + servicioId;
        }

        private static string CompararTurnos(Turno a, Turno b)
        {
            return AuditService.FormatearCambios(
                AuditService.Campo("Cliente", a.ClienteId.ToString(), b.ClienteId.ToString()),
                AuditService.Campo("Servicio", a.ServicioId?.ToString(), b.ServicioId?.ToString()),
                AuditService.Campo("Recurso", a.RecursoId.ToString(), b.RecursoId.ToString()),
                AuditService.Campo("Estado", a.EstadoTurnoId.ToString(), b.EstadoTurnoId.ToString()),
                AuditService.Campo("Inicio", a.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm"), b.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm")),
                AuditService.Campo("Observaciones", a.Observaciones, b.Observaciones));
        }
    }
}
