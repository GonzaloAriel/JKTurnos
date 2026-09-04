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
        private readonly ConfiguracionService _configService;

        public HomeService()
        {
            _turnoService = new TurnoService();
            _clienteService = new ClienteService();
            _pagoService = new PagoService();
            _configService = new ConfiguracionService();
        }

        public HomeViewModel GetHomeData(DateTime fecha)
        {
            var turnosDelDia = _turnoService.GetTurnosDelDia(fecha);

            var activos = turnosDelDia
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado)
                .ToList();

            var minutosOcupados = activos
                .Sum(t => (t.FechaHoraFin - t.FechaHoraInicio).TotalMinutes);

            // D-012 / D-046: ocupacion = minutos ocupados / Franja Formal (default 690 min).
            int minutosJornada = _configService.GetMinutosJornada();

            var ocupacion = minutosOcupados <= 0
                ? 0
                : (int)Math.Min(100, (minutosOcupados / (double)minutosJornada) * 100);

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