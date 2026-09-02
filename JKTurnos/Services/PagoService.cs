using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    // D-028: Dominio Financiero (DER 2.1). Reglas de pagos del turno.
    // Sin tope por precio de servicio (referencia). La senia se registra
    // una sola vez. No se bloquea por estado operativo (D-005).
    public class PagoService
    {
        private readonly BaseRepository<Pago> _repo;
        private readonly BaseRepository<Turno> _turnoRepo;

        public PagoService()
        {
            _repo = new BaseRepository<Pago>();
            _turnoRepo = new BaseRepository<Turno>();
        }

        public void RegistrarPago(int turnoId, decimal monto, TipoPago tipo, string observacion, int usuarioId)
        {
            var turno = _turnoRepo.GetAll()
                .Include(t => t.EstadoTurno)
                .FirstOrDefault(t => t.Id == turnoId);

            if (turno == null)
                throw new ArgumentException("Turno no encontrado.");

            // D-042: un turno cancelado no admite pagos (defensa en profundidad,
            // ademas del filtrado en lista/autocomplete).
            if (turno.EstadoTurno != null && turno.EstadoTurno.Codigo == EstadosTurnoBase.Cancelado)
                throw new InvalidOperationException("No se pueden registrar pagos en un turno cancelado.");

            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a 0.");

            if (tipo == TipoPago.Senia &&
                _repo.GetAll().Any(p => p.TurnoId == turnoId && p.TipoPago == TipoPago.Senia))
                throw new InvalidOperationException("La seña se registra una sola vez.");

            var pago = new Pago
            {
                TurnoId = turnoId,
                Monto = monto,
                Fecha = DateTime.Now,
                TipoPago = tipo,
                Observacion = (observacion ?? "").Trim(),
                UsuarioId = usuarioId
            };

            _repo.Add(pago);

            new NotificacionSistemaService().RegistrarPagoRegistrado(turnoId, monto, NombreTipoPago(tipo));

            new AuditService().Registrar(
                AuditService.A_Pago,
                AuditService.E_Pago,
                "Registro un pago de $" + monto.ToString("0.##") +
                " (" + NombreTipoPago(tipo) + ") para el turno #" + turnoId,
                pago.Id);
        }

        public List<Pago> GetPagos(int turnoId)
        {
            return _repo.GetAll()
                .Where(p => p.TurnoId == turnoId)
                .OrderBy(p => p.Fecha)
                .ToList();
        }

        // Listado de pagos (modulo Pagos): filtros por rango de fecha del pago
        // (D-029) y busqueda por cliente/recurso.
        public List<Pago> GetFiltrados(DateTime? fechaDesde = null, DateTime? fechaHasta = null, string busqueda = null, int? turnoId = null)
        {
            var query = _repo.GetAll()
                .Include(p => p.Turno.Cliente)
                .Include(p => p.Turno.Servicio)
                .Include(p => p.Turno.Recurso)
                .Include(p => p.Turno.EstadoTurno);

            // D-042: los pagos de turnos cancelados no se listan ni se buscan,
            // para no registrar pagos sobre un turno cancelado.
            query = query.Where(p =>
                p.Turno.EstadoTurno == null ||
                p.Turno.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado);

            if (turnoId.HasValue)
            {
                query = query.Where(p => p.TurnoId == turnoId.Value);
            }

            if (fechaDesde.HasValue)
            {
                var desde = fechaDesde.Value.Date;
                query = query.Where(p => p.Fecha >= desde);
            }

            if (fechaHasta.HasValue)
            {
                var hasta = fechaHasta.Value.Date.AddDays(1);
                query = query.Where(p => p.Fecha < hasta);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim();

                query = query.Where(p =>
                    (p.Turno.Cliente != null && p.Turno.Cliente.Nombre.Contains(termino)) ||
                    (p.Turno.Recurso != null && p.Turno.Recurso.Nombre.Contains(termino)));
            }

            return query.OrderByDescending(p => p.Fecha).ToList();
        }

        // Etiqueta legible del turno filtrado, para el titulo del modulo Pagos
        // cuando se accede desde la grilla de turnos (icono billete).
        public string GetTurnoEtiqueta(int turnoId)
        {
            var turno = _turnoRepo.GetAll()
                .Include(t => t.Cliente)
                .Include(t => t.Recurso)
                .FirstOrDefault(t => t.Id == turnoId);

            if (turno == null)
                return "Turno #" + turnoId;

            return (turno.Cliente != null ? turno.Cliente.Nombre : "Sin cliente")
                + " - " + turno.FechaHoraInicio.ToString("dd/MM/yyyy HH:mm");
        }

        // Baja logica con motivo: el registro queda conservado (D-008).
        public void Anular(int pagoId, string motivo)
        {
            var pago = _repo.GetById(pagoId);

            if (pago == null)
                throw new ArgumentException("Pago no encontrado.");

            if (!string.IsNullOrWhiteSpace(motivo))
                pago.Observacion = ((pago.Observacion ?? "") + " | Anulado: " + motivo.Trim()).Trim();

            var montoAnulado = pago.Monto;
            var turnoIdAnulado = pago.TurnoId;

            _repo.Delete(pagoId);

            new NotificacionSistemaService().RegistrarPagoAnulado(turnoIdAnulado, montoAnulado);

            var motivoTxt = string.IsNullOrWhiteSpace(motivo) ? "" : " - Motivo: " + motivo.Trim();

            new AuditService().Registrar(
                AuditService.A_Anular,
                AuditService.E_Pago,
                "Anulo un pago de $" + montoAnulado.ToString("0.##") +
                " del turno #" + turnoIdAnulado + motivoTxt,
                pagoId);
        }

        // Edicion del monto/tipo/observacion (D-031): corrige un error de carga.
        public void ActualizarPago(int pagoId, decimal monto, TipoPago tipo, string observacion)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a 0.");

            var pago = _repo.GetById(pagoId);

            if (pago == null)
                throw new ArgumentException("Pago no encontrado.");

            var cambios = AuditService.FormatearCambios(
                AuditService.Campo("Monto", pago.Monto.ToString("0.##"), monto.ToString("0.##")),
                AuditService.Campo("Tipo", NombreTipoPago(pago.TipoPago), NombreTipoPago(tipo)),
                AuditService.Campo("Observacion", pago.Observacion, (observacion ?? "").Trim()));

            pago.Monto = monto;
            pago.TipoPago = tipo;
            pago.Observacion = (observacion ?? "").Trim();

            _repo.Update(pago);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Pago,
                "Edito un pago del turno #" + pago.TurnoId,
                pagoId,
                cambios);
        }

        // Autocomplete del registro de pagos: busca turnos por cliente/recurso.
        // Devuelve top N (escalable a cualquier volumen de turnos).
        public List<Turno> BuscarTurnos(string termino, int limite = 20)
        {
            var query = _turnoRepo.GetAll()
                .Include(t => t.Cliente)
                .Include(t => t.Recurso)
                .Include(t => t.EstadoTurno);

            // D-042: los turnos cancelados no se pueden buscar para registrar
            // un pago (un turno cancelado no tiene cobros asociados).
            query = query.Where(t =>
                t.EstadoTurno == null ||
                t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado);

            if (!string.IsNullOrWhiteSpace(termino))
            {
                var t = termino.Trim();

                query = query.Where(turno =>
                    (turno.Cliente != null && turno.Cliente.Nombre.Contains(t)) ||
                    (turno.Recurso != null && turno.Recurso.Nombre.Contains(t)));
            }

            return query
                .OrderByDescending(turno => turno.FechaHoraInicio)
                .Take(limite)
                .ToList();
        }

        // D-029: dinero efectivamente cobrado en el dia (fecha del pago).
        public decimal GetIngresosDelDia(DateTime fecha)
        {
            var inicio = fecha.Date;
            var fin = inicio.AddDays(1);

            return _repo.GetAll()
                .Where(p => p.Fecha >= inicio && p.Fecha < fin)
                .Sum(p => (decimal?)p.Monto) ?? 0m;
        }

        // D-034: pagos en un rango de fechas (para el Dashboard). Incluye el turno
        // con recurso y servicio para agrupar por recurso.
        public List<Pago> GetPagosEnRango(DateTime desde, DateTime hasta)
        {
            var inicio = desde.Date;
            var fin = hasta.Date.AddDays(1);

            return _repo.GetAll()
                .Include(p => p.Turno.Recurso)
                .Include(p => p.Turno.Servicio)
                .Where(p => p.Fecha >= inicio && p.Fecha < fin)
                .OrderBy(p => p.Fecha)
                .ToList();
        }

        // Estado economico CALCULADO desde los pagos (nunca persistido, D-028).
        public string CalcularEstadoEconomico(List<Pago> pagos, decimal? precio)
        {
            if (pagos == null || pagos.Count == 0)
                return "Sin seña";

            var acumulado = pagos.Sum(p => p.Monto);

            if (!precio.HasValue)
            {
                var soloSenias = pagos.All(p => p.TipoPago == TipoPago.Senia);
                return soloSenias ? "Seña pagada" : "Pago completo";
            }

            if (acumulado >= precio.Value)
                return "Pago completo";

            var tieneSenia = pagos.Any(p => p.TipoPago == TipoPago.Senia);
            if (tieneSenia)
                return "Seña pagada";

            return "Pago parcial";
        }

        // Nombre legible del tipo de pago para el feed de actividad.
        private static string NombreTipoPago(TipoPago tipo)
        {
            switch (tipo)
            {
                case TipoPago.Senia:
                    return "Seña";
                case TipoPago.PagoParcial:
                    return "Pago parcial";
                case TipoPago.PagoTotal:
                    return "Pago total";
                default:
                    return tipo.ToString();
            }
        }

        // Resolucion de nombres del responsable (lectura de plataforma, D-022).
        public Dictionary<int, string> GetNombresUsuarios(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();

            if (ids.Count == 0)
                return new Dictionary<int, string>();

            return new GlobalRepository<User>()
                .GetAll()
                .Where(u => ids.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.Nombre ?? u.Email);
        }
    }
}