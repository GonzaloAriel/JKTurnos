using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    // Notificaciones del sistema (in-app): feed de actividad del tenant.
    // Se registran desde los Services que ya ejecutan las acciones (sin
    // TurnoHistorial, que es V2 a futuro). El feed es por tenant.
    public class NotificacionSistemaService
    {
        public const string TipoTurnoCreado = "TurnoCreado";
        public const string TipoTurnoEstado = "TurnoEstado";
        public const string TipoPagoRegistrado = "PagoRegistrado";
        public const string TipoPagoAnulado = "PagoAnulado";
        public const string TipoEmail = "Email";

        public const string EntidadTurno = "Turno";
        public const string EntidadPago = "Pago";

        private readonly BaseRepository<NotificacionSistema> _repo;
        private readonly BaseRepository<Turno> _turnoRepo;

        public NotificacionSistemaService()
        {
            _repo = new BaseRepository<NotificacionSistema>();
            _turnoRepo = new BaseRepository<Turno>();
        }

        public void Registrar(string tipo, string titulo, string mensaje, string entidadTipo = null, int? entidadId = null)
        {
            _repo.Add(new NotificacionSistema
            {
                Tipo = tipo,
                Titulo = titulo,
                Mensaje = mensaje,
                EntidadTipo = entidadTipo,
                EntidadId = entidadId,
                Fecha = DateTime.Now,
                Leida = false
            });
        }

        public List<NotificacionSistema> GetUltimas(int cantidad)
        {
            return _repo.GetAll()
                .OrderByDescending(n => n.Fecha)
                .Take(cantidad)
                .ToList();
        }

        public int GetNoLeidasCount()
        {
            return _repo.GetAll().Count(n => !n.Leida);
        }

        public void MarcarTodasLeidas()
        {
            var noLeidas = _repo.GetAll().Where(n => !n.Leida).ToList();

            foreach (var n in noLeidas)
                n.Leida = true;

            _repo.UpdateRange(noLeidas);
        }

        public void RegistrarTurnoCreado(int turnoId)
        {
            var turno = ObtenerTurno(turnoId);
            if (turno == null) return;

            Registrar(
                TipoTurnoCreado,
                "Nuevo turno",
                ConstruirMensajeTurno(turno),
                EntidadTurno,
                turno.Id);
        }

        public void RegistrarCambioEstado(int turnoId, string estadoNombre)
        {
            var turno = ObtenerTurno(turnoId);
            if (turno == null) return;

            var estado = string.IsNullOrWhiteSpace(estadoNombre) ? "actualizado" : estadoNombre.Trim();

            Registrar(
                TipoTurnoEstado,
                "Turno " + estado.ToLower(),
                ConstruirMensajeTurno(turno),
                EntidadTurno,
                turno.Id);
        }

        public void RegistrarPagoRegistrado(int turnoId, decimal monto, string tipoPago)
        {
            var turno = ObtenerTurno(turnoId);
            if (turno == null) return;

            var cliente = turno.Cliente != null ? turno.Cliente.Nombre : "sin cliente";

            Registrar(
                TipoPagoRegistrado,
                "Pago registrado",
                "Pago de $" + monto.ToString("0.##") + " (" + tipoPago + ") - " + cliente,
                EntidadTurno,
                turno.Id);
        }

        public void RegistrarPagoAnulado(int turnoId, decimal monto)
        {
            var turno = ObtenerTurno(turnoId);
            if (turno == null) return;

            var cliente = turno.Cliente != null ? turno.Cliente.Nombre : "sin cliente";

            Registrar(
                TipoPagoAnulado,
                "Pago anulado",
                "Se anulo un pago de $" + monto.ToString("0.##") + " - " + cliente,
                EntidadTurno,
                turno.Id);
        }

        public void RegistrarEmail(int turnoId, bool exito, string detalle = null)
        {
            var turno = ObtenerTurno(turnoId);
            if (turno == null) return;

            var cliente = turno.Cliente != null ? turno.Cliente.Nombre : "sin cliente";

            if (exito)
            {
                Registrar(
                    TipoEmail,
                    "Recordatorio enviado",
                    "Email enviado a " + cliente,
                    EntidadTurno,
                    turno.Id);
            }
            else
            {
                var motivo = string.IsNullOrWhiteSpace(detalle) ? "" : " - " + detalle.Trim();
                Registrar(
                    TipoEmail,
                    "Fallo el envio de email",
                    "No se pudo enviar el email a " + cliente + motivo,
                    EntidadTurno,
                    turno.Id);
            }
        }

        private Turno ObtenerTurno(int turnoId)
        {
            return _turnoRepo.GetAll()
                .Include(t => t.Cliente)
                .Include(t => t.Servicio)
                .Include(t => t.Recurso)
                .FirstOrDefault(t => t.Id == turnoId);
        }

        private static string ConstruirMensajeTurno(Turno turno)
        {
            var cliente = turno.Cliente != null ? turno.Cliente.Nombre : "sin cliente";
            var servicio = turno.Servicio != null ? turno.Servicio.Nombre : "sin servicio";
            var fecha = turno.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm");

            return cliente + " - " + servicio + " " + fecha;
        }
    }
}