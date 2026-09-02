using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Services
{
    // D-034: Seccion Dashboard (seccion aparte). Agrega y agrupa datos que ya
    // existen (Turnos, Pagos, Clientes, Servicios, Recursos) sin nuevo schema.
    // Fase 2: evolucion de ingresos (12 meses), ingresos por recurso,
    // desglose por tipo de pago y variacion mensual.
    public class DashboardService
    {
        // Jornada laboral estandar de la Agenda (D-012): 08:00 - 19:30
        private const int MinutosJornada = 690;

        private static readonly string[] MesesCortos =
            { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

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
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado)
                .ToList();

            var pagosMes = _pagoService.GetPagosEnRango(inicioMes, finMes);

            var ingresoMesAnterior = _pagoService.GetPagosEnRango(inicioMes.AddMonths(-1), inicioMes)
                .Sum(p => p.Monto);

            return new DashboardViewModel
            {
                TurnosMes = activosMes.Count,
                IngresosMes = pagosMes.Sum(p => p.Monto),
                IngresosMesAnterior = ingresoMesAnterior,
                VariacionMensual = CalcularVariacion(pagosMes.Sum(p => p.Monto), ingresoMesAnterior),
                Clientes = _clienteService.GetAll().Count,
                OcupacionHoy = CalcularOcupacionHoy(hoyDate),
                OcupacionSemana = CalcularOcupacionSemana(hoyDate),
                IngresosMesPorDia = pagosMes
                    .GroupBy(p => p.Fecha.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new BarraGrafico { Etiqueta = g.Key.Day.ToString(), Valor = g.Sum(p => p.Monto) })
                    .ToList(),
                IngresosUltimosMeses = EvolucionIngresos(inicioMes, finMes),
                ProximosTurnos = ProximosTurnos(),
                TurnosPendientes = TurnosPendientes(),
                RankingServicios = RankingServicios(activosMes),
                IngresosPorRecurso = IngresosPorRecurso(pagosMes),
                PagosPorTipo = PagosPorTipo(pagosMes),
                SaldoPendiente = CalcularSaldoPendiente(activosMes, pagosMes, out int turnosConSaldo),
                TurnosConSaldoPendiente = turnosConSaldo
            };
        }

        private static decimal CalcularVariacion(decimal actual, decimal anterior)
        {
            if (anterior <= 0)
                return 0;

            return Math.Round(((actual - anterior) / anterior) * 100, 1);
        }

        private static List<BarraGrafico> EvolucionIngresos(DateTime inicioMes, DateTime finMes)
        {
            var pagos12 = new PagoService().GetPagosEnRango(inicioMes.AddMonths(-11), finMes);

            var resultado = new List<BarraGrafico>();

            for (var i = 11; i >= 0; i--)
            {
                var mes = inicioMes.AddMonths(-i);
                var siguiente = mes.AddMonths(1);

                var total = pagos12
                    .Where(p => p.Fecha >= mes && p.Fecha < siguiente)
                    .Sum(p => p.Monto);

                resultado.Add(new BarraGrafico
                {
                    Etiqueta = MesesCortos[mes.Month - 1],
                    Valor = total
                });
            }

            return resultado;
        }

        private static List<RankingItem> IngresosPorRecurso(List<Pago> pagosMes)
        {
            return pagosMes
                .Where(p => p.Turno != null && p.Turno.Recurso != null)
                .GroupBy(p => p.Turno.Recurso.Nombre)
                .OrderByDescending(g => g.Sum(p => p.Monto))
                .Take(6)
                .Select(g => new RankingItem { Nombre = g.Key, Total = g.Sum(p => p.Monto) })
                .ToList();
        }

        private static List<PagosPorTipoItem> PagosPorTipo(List<Pago> pagosMes)
        {
            var porTipo = pagosMes.GroupBy(p => p.TipoPago);

            return Enum.GetValues(typeof(TipoPago))
                .Cast<TipoPago>()
                .Select(tipo =>
                {
                    var grupo = porTipo.FirstOrDefault(g => g.Key == tipo);
                    var items = grupo != null ? grupo.ToList() : new List<Pago>();

                    return new PagosPorTipoItem
                    {
                        Tipo = tipo == TipoPago.Senia ? "Seña" : (tipo == TipoPago.PagoParcial ? "Pago parcial" : "Pago total"),
                        Cantidad = items.Count,
                        Total = items.Sum(p => p.Monto)
                    };
                })
                .ToList();
        }

        private int CalcularOcupacionHoy(DateTime hoyDate)
        {
            var activos = _turnoService.GetTurnosDelDia(hoyDate)
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado)
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
                    (t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado));

                resultado.Add(new BarraGrafico { Etiqueta = dayNames[i], Valor = cantidad });
            }

            return resultado;
        }

        private List<DashboardTurnoItem> ProximosTurnos()
        {
            var ahora = DateTime.Now;

            return _turnoService.GetTurnosEnRango(DateTime.Today, DateTime.Today.AddDays(14))
                .Where(t => t.FechaHoraFin > ahora)
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado)
                .OrderBy(t => t.FechaHoraInicio)
                .Take(6)
                .Select(MapTurnoItem)
                .ToList();
        }

        private List<DashboardTurnoItem> TurnosPendientes()
        {
            var ahora = DateTime.Now;

            return _turnoService.GetTurnosEnRango(DateTime.Today, DateTime.Today.AddDays(14))
                .Where(t => t.FechaHoraFin > ahora && t.EstadoTurno != null && t.EstadoTurno.Codigo == EstadosTurnoBase.Pendiente)
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
            var situacionClave = SituacionTemporal.Calcular(t);
            var situacion = situacionClave != null ? SituacionTemporal.De(situacionClave) : null;

            return new DashboardTurnoItem
            {
                Id = t.Id,
                ClienteNombre = t.Cliente != null ? t.Cliente.Nombre : "Sin cliente",
                ServicioNombre = t.Servicio != null ? t.Servicio.Nombre : "Reservado",
                RecursoNombre = t.Recurso != null ? t.Recurso.Nombre : "Sin recurso",
                EstadoNombre = t.EstadoTurno != null ? t.EstadoTurno.Nombre : "",
                EstadoIcono = t.EstadoTurno != null ? t.EstadoTurno.Icono : "bi-circle-fill",
                SituacionClave = situacionClave,
                SituacionLabel = situacion != null ? situacion.Label : null,
                SituacionColor = situacion != null ? situacion.Color : null,
                SituacionIcono = situacion != null ? situacion.Icono : null,
                FechaHoraInicio = t.FechaHoraInicio,
                FechaHoraFin = t.FechaHoraFin
            };
        }
    }
}