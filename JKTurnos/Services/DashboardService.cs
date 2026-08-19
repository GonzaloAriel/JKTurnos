using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Services
{
    // D-034: Seccion Dashboard (seccion aparte). Agrega y agrupa datos que ya
    // existen (Turnos, Pagos, Clientes, Servicios, Recursos) sin nuevo schema.
    public class DashboardService
    {
        // Jornada laboral estandar de la Agenda (D-012): 08:00 - 19:30
        private const int MinutosJornada = 690;

        private readonly TurnoService _turnoService;
        private readonly PagoService _pagoService;
        private readonly ClienteService _clienteService;

        public DashboardService()
        {
            _turnoService = new TurnoService();
            _pagoService = new PagoService();
            _clienteService = new ClienteService();
        }

        public DashboardViewModel GetDashboard(DateTime hoy)
        {
            var hoyDate = hoy.Date;
            var inicioMes = new DateTime(hoyDate.Year, hoyDate.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            var turnosMes = _turnoService.GetTurnosEnRango(inicioMes, finMes);
            var activosMes = turnosMes
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Nombre != "Cancelado")
                .ToList();

            var pagosMes = _pagoService.GetPagosEnRango(inicioMes, finMes);

            return new DashboardViewModel
            {
                TurnosMes = activosMes.Count,
                IngresosMes = pagosMes.Sum(p => p.Monto),
                Clientes = _clienteService.GetAll().Count,
                OcupacionHoy = CalcularOcupacionHoy(hoyDate),
                OcupacionSemana = CalcularOcupacionSemana(hoyDate),
                IngresosMesPorDia = pagosMes
                    .GroupBy(p => p.Fecha.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new BarraGrafico { Etiqueta = g.Key.Day.ToString(), Valor = g.Sum(p => p.Monto) })
                    .ToList(),
                ProximosTurnos = ProximosTurnos(),
                TurnosPendientes = TurnosPendientes(),
                RankingServicios = RankingServicios(activosMes),
                CobradoPorRecurso = CobradoPorRecurso(pagosMes),
                SaldoPendiente = CalcularSaldoPendiente(activosMes, pagosMes, out int turnosConSaldo),
                TurnosConSaldoPendiente = turnosConSaldo
            };
        }

        private int CalcularOcupacionHoy(DateTime hoyDate)
        {
            var activos = _turnoService.GetTurnosDelDia(hoyDate)
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Nombre != "Cancelado")
                .ToList();

            var minutosOcupados = activos.Sum(t => (t.FechaHoraFin - t.FechaHoraInicio).TotalMinutes);

            if (minutosOcupados <= 0)
                return 0;

            return (int)Math.Min(100, (minutosOcupados / (double)MinutosJornada) * 100);
        }

        private List<BarraGrafico> CalcularOcupacionSemana(DateTime hoyDate)
        {
            int diff = ((int)DayOfWeek.Monday - (int)hoyDate.DayOfWeek + 7) % 7;
            var lunes = hoyDate.AddDays(diff);

            var turnosSemana = _turnoService.GetTurnosEnRango(lunes, lunes.AddDays(7));
            var dayNames = new[] { "Lun", "Mar", "Mie", "Jue", "Vie", "Sab", "Dom" };
            var resultado = new List<BarraGrafico>();

            for (int i = 0; i < 7; i++)
            {
                var dia = lunes.AddDays(i);
                var cantidad = turnosSemana.Count(t =>
                    t.FechaHoraInicio.Date == dia.Date &&
                    (t.EstadoTurno == null || t.EstadoTurno.Nombre != "Cancelado"));

                resultado.Add(new BarraGrafico { Etiqueta = dayNames[i], Valor = cantidad });
            }

            return resultado;
        }

        private List<DashboardTurnoItem> ProximosTurnos()
        {
            var ahora = DateTime.Now;

            return _turnoService.GetTurnosEnRango(DateTime.Today, DateTime.Today.AddDays(14))
                .Where(t => t.FechaHoraFin > ahora)
                .OrderBy(t => t.FechaHoraInicio)
                .Take(6)
                .Select(MapTurnoItem)
                .ToList();
        }

        private List<DashboardTurnoItem> TurnosPendientes()
        {
            var ahora = DateTime.Now;

            return _turnoService.GetTurnosEnRango(DateTime.Today, DateTime.Today.AddDays(14))
                .Where(t => t.FechaHoraFin > ahora && t.EstadoTurno != null && t.EstadoTurno.Nombre == "Pendiente")
                .OrderBy(t => t.FechaHoraInicio)
                .Take(6)
                .Select(MapTurnoItem)
                .ToList();
        }

        private List<RankingItem> RankingServicios(List<Models.Entities.Turno> activosMes)
        {
            return activosMes
                .Where(t => t.Servicio != null)
                .GroupBy(t => t.Servicio.Nombre)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new RankingItem { Nombre = g.Key, Cantidad = g.Count() })
                .ToList();
        }

        private List<RankingItem> CobradoPorRecurso(List<Models.Entities.Pago> pagosMes)
        {
            return pagosMes
                .Where(p => p.Turno != null && p.Turno.Recurso != null)
                .GroupBy(p => p.Turno.Recurso.Nombre)
                .OrderByDescending(g => g.Sum(p => p.Monto))
                .Take(5)
                .Select(g => new RankingItem { Nombre = g.Key, Total = g.Sum(p => p.Monto) })
                .ToList();
        }

        private decimal CalcularSaldoPendiente(
            List<Models.Entities.Turno> activosMes,
            List<Models.Entities.Pago> pagosMes,
            out int turnosConSaldo)
        {
            var acumuladoPorTurno = pagosMes
                .GroupBy(p => p.TurnoId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Monto));

            decimal saldo = 0;
            turnosConSaldo = 0;

            foreach (var t in activosMes.Where(t => t.Servicio != null))
            {
                var acumulado = acumuladoPorTurno.ContainsKey(t.Id) ? acumuladoPorTurno[t.Id] : 0m;
                var pendiente = Math.Max(0, t.Servicio.Precio - acumulado);

                if (pendiente > 0)
                {
                    saldo += pendiente;
                    turnosConSaldo++;
                }
            }

            return saldo;
        }

        private static DashboardTurnoItem MapTurnoItem(Models.Entities.Turno t)
        {
            return new DashboardTurnoItem
            {
                Id = t.Id,
                ClienteNombre = t.Cliente != null ? t.Cliente.Nombre : "Sin cliente",
                ServicioNombre = t.Servicio != null ? t.Servicio.Nombre : "Reservado",
                RecursoNombre = t.Recurso != null ? t.Recurso.Nombre : "Sin recurso",
                EstadoNombre = t.EstadoTurno != null ? t.EstadoTurno.Nombre : "",
                EstadoColor = t.EstadoTurno != null ? t.EstadoTurno.Color : "#aeaeb2",
                FechaHoraInicio = t.FechaHoraInicio,
                FechaHoraFin = t.FechaHoraFin
            };
        }
    }
}