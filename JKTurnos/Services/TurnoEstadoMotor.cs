using System;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// P6: motor UNICO de transiciones de estado del Turno (D-040, contrato P2 seccion 3).
    /// Centraliza la matriz canonica de transiciones y toda la logica comun
    /// (guardia de estado origen, resolucion por Codigo estable, notificacion y auditoria).
    /// La semantica depende de Codigo, nunca de Nombre.
    ///
    /// Matriz canonica (regla unica):
    ///   Pendiente -&gt; En curso   SI
    ///   Pendiente -&gt; Cancelado  SI
    ///   Pendiente -&gt; Finalizado NO
    ///   En curso  -&gt; Finalizado SI
    ///   En curso  -&gt; Cancelado  SI
    ///   En curso  -&gt; Pendiente  NO
    ///   Finalizado -&gt; cualquier NO (terminal)
    ///   Cancelado  -&gt; cualquier NO (terminal)
    ///
    /// TurnoService delega aqui; este es el unico lugar que sabe transicionar estados.
    /// </summary>
    public class TurnoEstadoMotor
    {
        private readonly BaseRepository<Turno> _turnoRepo;
        private readonly BaseRepository<EstadoTurno> _estadoRepo;
        private readonly BaseRepository<Cliente> _clienteRepo;

        public TurnoEstadoMotor()
        {
            _turnoRepo = new BaseRepository<Turno>();
            _estadoRepo = new BaseRepository<EstadoTurno>();
            _clienteRepo = new BaseRepository<Cliente>();
        }

        /// <summary>
        /// Regla canonica de la matriz: retorna si es permitida la transicion
        /// origen -&gt; destino (ambos por Codigo estable).
        /// </summary>
        public bool EsTransicionPermitida(string codigoOrigen, string codigoDestino)
        {
            if (codigoOrigen == EstadosTurnoBase.Pendiente)
                return codigoDestino == EstadosTurnoBase.EnCurso ||
                       codigoDestino == EstadosTurnoBase.Cancelado;

            if (codigoOrigen == EstadosTurnoBase.EnCurso)
                return codigoDestino == EstadosTurnoBase.Finalizado ||
                       codigoDestino == EstadosTurnoBase.Cancelado;

            // Finalizado y Cancelado son terminales: sin transiciones de salida.
            return false;
        }

        /// <summary>Transicion operatoria: Pendiente -&gt; En curso.</summary>
        public void Iniciar(int turnoId)
        {
            Transicionar(turnoId, EstadosTurnoBase.EnCurso,
                "Solo se puede iniciar un turno cuyo estado sea Pendiente.",
                "Inicio la atencion del turno de ");
        }

        /// <summary>Transicion operatoria: En curso -&gt; Finalizado.</summary>
        public void Finalizar(int turnoId)
        {
            Transicionar(turnoId, EstadosTurnoBase.Finalizado,
                "Solo se puede finalizar un turno cuyo estado sea En curso.",
                "Finalizo la atencion del turno de ");
        }

        /// <summary>
        /// Salida terminal: Pendiente/En curso -&gt; Cancelado.
        /// P6 agrega la guardia de estado origen (solo Pendiente o En curso).
        /// </summary>
        public void Cancelar(int turnoId)
        {
            Transicionar(turnoId, EstadosTurnoBase.Cancelado,
                "Solo se puede cancelar un turno Pendiente o En curso.",
                "Cancelo el turno de ");
        }

        /// <summary>
        /// Transicion generica por Id de estado destino, validada contra la matriz canonica.
        /// Reemplaza y centraliza el `CambiarEstado` generico (sin call sites previos).
        /// </summary>
        public void CambiarEstado(int turnoId, int estadoTurnoId)
        {
            var turno = ObtenerTurno(turnoId);
            var origen = EstadoDe(turno.EstadoTurnoId);
            var destino = EstadoDe(estadoTurnoId);

            if (origen == null)
                throw new InvalidOperationException(
                    "Estado de origen no encontrado. Ejecute la semilla primero.");
            if (destino == null)
                throw new ArgumentException("Estado de destino no encontrado.");

            if (!EsTransicionPermitida(origen.Codigo, destino.Codigo))
                throw new InvalidOperationException(
                    "La transicion de estado no es permitida: " +
                    origen.Nombre + " -> " + destino.Nombre + ".");

            Aplicar(turno, destino, "Cambio el estado del turno de ");
        }

        // ------------------------------------------------------------------

        private void Transicionar(int turnoId, string codigoDestino,
            string mensajeError, string verbo)
        {
            var turno = ObtenerTurno(turnoId);
            var origen = EstadoDe(turno.EstadoTurnoId);

            if (origen == null)
                throw new InvalidOperationException(
                    "Estado de origen no encontrado. Ejecute la semilla primero.");

            if (!EsTransicionPermitida(origen.Codigo, codigoDestino))
                throw new InvalidOperationException(mensajeError);

            var destino = EstadoPorCodigo(codigoDestino);
            if (destino == null)
                throw new InvalidOperationException(
                    "Estado " + codigoDestino + " no encontrado. Ejecute la semilla primero.");

            Aplicar(turno, destino, verbo);
        }

        private void Aplicar(Turno turno, EstadoTurno destino, string verbo)
        {
            var estadoAnterior = _estadoRepo.GetById(turno.EstadoTurnoId)?.Nombre
                ?? turno.EstadoTurnoId.ToString();
            var estadoNuevo = destino.Nombre;

            turno.EstadoTurnoId = destino.Id;
            _turnoRepo.Update(turno);

            new NotificacionSistemaService().RegistrarCambioEstado(turno.Id, estadoNuevo);

            new AuditService().Registrar(
                AuditService.A_CambiarEstado,
                AuditService.E_Turno,
                verbo + NombreCliente(turno.ClienteId) + " " +
                turno.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                turno.Id,
                AuditService.FormatearCambios(
                    AuditService.Campo("Estado", estadoAnterior, estadoNuevo)));
        }

        private Turno ObtenerTurno(int turnoId)
        {
            var turno = _turnoRepo.GetById(turnoId);
            if (turno == null)
                throw new ArgumentException("Turno no encontrado.");
            return turno;
        }

        private EstadoTurno EstadoDe(int estadoId)
        {
            return _estadoRepo.GetById(estadoId);
        }

        private EstadoTurno EstadoPorCodigo(string codigo)
        {
            return _estadoRepo.GetAll()
                .FirstOrDefault(x => x.Codigo == codigo);
        }

        private string NombreCliente(int clienteId)
        {
            var cliente = _clienteRepo.GetById(clienteId);
            return cliente != null ? cliente.Nombre : "#" + clienteId;
        }
    }
}
