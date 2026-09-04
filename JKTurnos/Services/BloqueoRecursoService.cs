using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    // D-046 / M-01: servicio del BloqueoRecurso (Concepto A). Contiene el CRUD y la
    // logica de solapamiento. EstaBloqueado es el punto unico que consulta
    // TurnoService.ValidarDisponibilidad como segunda fuente de ocupacion.
    public class BloqueoRecursoService
    {
        private readonly BaseRepository<BloqueoRecurso> _repo;
        private readonly BaseRepository<Recurso> _recursoRepo;

        public BloqueoRecursoService()
        {
            _repo = new BaseRepository<BloqueoRecurso>();
            _recursoRepo = new BaseRepository<Recurso>();
        }

        public List<BloqueoRecurso> GetAll()
        {
            return _repo.GetAll()
                        .OrderByDescending(b => b.FechaInicio)
                        .ToList();
        }

        // D-046: bloques posiblemente activos dentro de [desde, hasta). Se usa para
        // pintar los grises en la agenda. La coincidencia por tipo se resuelve en la
        // vista (asociada a cada dia de la grilla) via SolapaFecha.
        public List<BloqueoRecurso> GetEnRango(DateTime desde, DateTime hastaInclusive)
        {
            var fin = hastaInclusive.Date.AddDays(1);
            var inicio = desde.Date;

            return _repo.GetAll()
                .ToList()
                .Where(b => CoincideRangoConsulta(b, inicio, fin))
                .ToList();
        }

        // ?Un bloqueo puede caer dentro de una ventana [inicio, fin)?
        private bool CoincideRangoConsulta(BloqueoRecurso b, DateTime ventanaInicio, DateTime ventanaFin)
        {
            switch (b.Tipo)
            {
                case TipoBloqueo.Unico:
                    var d = b.FechaInicio.Date;
                    return d >= ventanaInicio && d < ventanaFin;

                case TipoBloqueo.RangoFechas:
                    var rIni = b.FechaInicio.Date;
                    var rFin = (b.FechaFin ?? b.FechaInicio).Date.AddDays(1);
                    return rIni < ventanaFin && rFin > ventanaInicio;

                case TipoBloqueo.RecurrenteSemanal:
                default:
                    // Un bloqueo semanal cubre (potencialmente) todos los dias de su
                    // dia-de-semana dentro de la ventana, indefinidamente.
                    return ventanaInicio.DayOfWeek == b.FechaInicio.DayOfWeek
                        || (ventanaFin != ventanaInicio && RangoCubreDiaSemana(b, ventanaInicio, ventanaFin));
            }
        }

        private bool RangoCubreDiaSemana(BloqueoRecurso b, DateTime ventanaInicio, DateTime ventanaFin)
        {
            for (var d = ventanaInicio; d < ventanaFin; d = d.AddDays(1))
            {
                if (d.DayOfWeek == b.FechaInicio.DayOfWeek)
                    return true;
            }
            return false;
        }

        public BloqueoRecurso GetById(int id)
        {
            return _repo.GetById(id);
        }

        public List<Recurso> GetRecursos()
        {
            return _recursoRepo.GetAll()
                               .OrderBy(r => r.Nombre)
                               .ToList();
        }

        public void Create(BloqueoRecurso bloqueo)
        {
            Normalizar(bloqueo);
            _repo.Add(bloqueo);

            new AuditService().Registrar(
                AuditService.A_Crear,
                AuditService.E_Recurso,
                "Creo un bloqueo de disponibilidad para el recurso #" + bloqueo.RecursoId,
                bloqueo.Id);
        }

        public void Update(BloqueoRecurso bloqueo)
        {
            var existente = _repo.GetById(bloqueo.Id);
            if (existente == null)
                throw new ArgumentException("Bloqueo no encontrado.");

            Normalizar(bloqueo);
            _repo.Update(bloqueo);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Recurso,
                "Edito un bloqueo de disponibilidad del recurso #" + bloqueo.RecursoId,
                bloqueo.Id);
        }

        public void Delete(int id)
        {
            var bloqueo = _repo.GetById(id);
            _repo.Delete(id);

            new AuditService().Registrar(
                AuditService.A_Eliminar,
                AuditService.E_Recurso,
                bloqueo != null
                    ? "Elimino el bloqueo de disponibilidad del recurso #" + bloqueo.RecursoId
                    : "Elimino el bloqueo de disponibilidad #" + id,
                id);
        }

        // Un bloqueo es "activo" en (inicio, fin] del turno en evaluacion si coincide
        // en fecha (segun el tipo) y, cuando define franja horaria, se solapa con ella.
        // Sin franja = ocupa todo el dia.
        private bool Solapa(BloqueoRecurso b, DateTime inicio, DateTime fin)
        {
            var diaInicio = inicio.Date;
            bool coincideFecha;

            switch (b.Tipo)
            {
                case TipoBloqueo.Unico:
                    coincideFecha = b.FechaInicio.Date == diaInicio;
                    break;

                case TipoBloqueo.RangoFechas:
                    var diaFin = b.FechaFin.HasValue ? b.FechaFin.Value.Date : b.FechaInicio.Date;
                    coincideFecha = diaInicio >= b.FechaInicio.Date && diaInicio <= diaFin;
                    break;

                case TipoBloqueo.RecurrenteSemanal:
                default:
                    coincideFecha = diaInicio.DayOfWeek == b.FechaInicio.DayOfWeek;
                    break;
            }

            if (!coincideFecha)
                return false;

            // Sin franja horaria: ocupa el dia completo.
            if (!b.HoraInicio.HasValue || !b.HoraFin.HasValue)
                return true;

            // Todo el dia (mitad del dia o franja completa o invertida no valida): ocupa el dia.
            if (b.HoraInicio.Value >= b.HoraFin.Value)
                return true;

            // Solapamiento sobre la franja del dia.
            DateTime franjaStart = diaInicio.Add(b.HoraInicio.Value);
            DateTime franjaEnd = diaInicio.Add(b.HoraFin.Value);

            return inicio < franjaEnd && fin > franjaStart;
        }

        // Publico para TurnoService.ValidarDisponibilidad: true si existe un bloqueo
        // activo (segundo motivo de indisponibilidad junto a los turnos).
        public bool EstaBloqueado(int recursoId, DateTime inicio, DateTime fin)
        {
            return _repo.GetAll()
                        .Where(b => b.RecursoId == recursoId)
                        .ToList()
                        .Any(b => Solapa(b, inicio, fin));
        }

        // Devuelve los turnos del recurso que colisionan con el rango del bloqueo
        // (solo ocupacion: Pendiente / En curso / Finalizado; el Cancelado libera).
        // Se usa para el aviso amable al crear/editar un bloqueo.
        public List<Turno> TurnosColisionan(int recursoId, DateTime inicio, DateTime fin, int? bloqueoExcluirId = null)
        {
            var repoTurnos = new BaseRepository<Turno>();
            var query = repoTurnos.GetAll()
                .Where(t => t.RecursoId == recursoId)
                .Where(t => t.FechaHoraInicio < fin && t.FechaHoraFin > inicio)
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado);

            if (bloqueoExcluirId.HasValue)
                query = query.Where(t => t.Id != bloqueoExcluirId.Value);

            // Se evalua la franja efectiva: para mostrar solo los turnos del dia o
            // franja que realmente se bloquean. Por simplicidad del aviso se listan los
            // turnos operativos cuyo horario solape el rango del bloqueo en la/s fecha/s.
            return query.ToList();
        }

        private void Normalizar(BloqueoRecurso b)
        {
            if (b.Tipo != TipoBloqueo.RangoFechas)
                b.FechaFin = null;

            // Rango con fin anterior al inicio: se invierte (defensa).
            if (b.Tipo == TipoBloqueo.RangoFechas && b.FechaFin.HasValue && b.FechaFin.Value.Date < b.FechaInicio.Date)
                b.FechaFin = b.FechaInicio.Date;

            // Franja incompleta o invertida = todo el dia.
            if (!b.HoraInicio.HasValue || !b.HoraFin.HasValue)
            {
                b.HoraInicio = null;
                b.HoraFin = null;
            }
            else if (b.HoraInicio.Value >= b.HoraFin.Value)
            {
                b.HoraInicio = null;
                b.HoraFin = null;
            }
        }
    }
}
