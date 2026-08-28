using System;
using System.Linq;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Services
{
    public class HomeService
    {
        private readonly TurnoService _turnoService;
        private readonly ClienteService _clienteService;
        private readonly PagoService _pagoService;

        // Jornada laboral estandar de la Agenda (D-012): 08:00 - 19:30
        private const int MinutosJornada = 690;

        public HomeService()
        {
            _turnoService = new TurnoService();
            _clienteService = new ClienteService();
            _pagoService = new PagoService();
        }

        public HomeViewModel GetHomeData(DateTime fecha)
        {
            var turnosDelDia = _turnoService.GetTurnosDelDia(fecha);

            var activos = turnosDelDia
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado)
                .ToList();

            var minutosOcupados = activos
                .Sum(t => (t.FechaHoraFin - t.FechaHoraInicio).TotalMinutes);

            var ocupacion = minutosOcupados <= 0
                ? 0
                : (int)Math.Min(100, (minutosOcupados / (double)MinutosJornada) * 100);

            return new HomeViewModel
            {
                TurnosHoy = activos.Count,
                // D-029: ingresos reales = suma de pagos cobrados en el dia.
                IngresosHoy = _pagoService.GetIngresosDelDia(fecha),
                Ocupacion = ocupacion,
                Clientes = _clienteService.GetAll().Count,
                TurnosDelDia = turnosDelDia
            };
        }
    }
}