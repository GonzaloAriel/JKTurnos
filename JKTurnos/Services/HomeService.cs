using System;
using System.Collections.Generic;
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
        private readonly RecursoService _recursoService;
        private readonly ServicioService _servicioService;

        public HomeService()
        {
            _turnoService = new TurnoService();
            _clienteService = new ClienteService();
            _pagoService = new PagoService();
            _configService = new ConfiguracionService();
            _recursoService = new RecursoService();
            _servicioService = new ServicioService();
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
                TurnosDelDia = turnosDelDia,
                SetupSteps = BuildSetupChecklist(_configService.GetConfiguracion())
            };
        }

        // M-09: checklist de primeros pasos para la configuracion progresiva.
        // No obliga: la card solo se muestra mientras haya pendientes y cada paso
        // es un link directo a su "Crear/Configurar". Se resuelve en el momento
        // con datos reales (recurso/servicio existentes + config de negocio).
        private List<SetupStepVM> BuildSetupChecklist(ConfiguracionTenant config)
        {
            var tieneRecurso = _recursoService.GetAll().Count > 0;
            var tieneServicio = _servicioService.GetAll().Count > 0;

            var tieneHorarios = config != null
                && config.HoraApertura.HasValue
                && config.HoraCierre.HasValue
                && !string.IsNullOrWhiteSpace(config.DiasLaborales);

            var tieneLogo = config != null
                && (!string.IsNullOrWhiteSpace(config.LogoUrl)
                    || !string.IsNullOrWhiteSpace(config.AvatarUrl));

            var tieneDatos = config != null
                && !string.IsNullOrWhiteSpace(config.NombreComercial);

            return new List<SetupStepVM>
            {
                new SetupStepVM
                {
                    Clave = "recurso",
                    Titulo = "Crea tu primer recurso",
                    Descripcion = "Un espacio, sala o profesional que recibe reservas.",
                    Url = "/Recursos/Create",
                    Icono = "person-badge",
                    Completo = tieneRecurso
                },
                new SetupStepVM
                {
                    Clave = "servicio",
                    Titulo = "Agrega un servicio",
                    Descripcion = "Define que ofreces y cuanto dura cada uno.",
                    Url = "/Servicios/Create",
                    Icono = "briefcase",
                    Completo = tieneServicio
                },
                new SetupStepVM
                {
                    Clave = "horarios",
                    Titulo = "Configura tus horarios",
                    Descripcion = "Define apertura, cierre y dias laborables.",
                    Url = "/Configuracion/DatosNegocio",
                    Icono = "clock",
                    Completo = tieneHorarios
                },
                new SetupStepVM
                {
                    Clave = "logo",
                    Titulo = "Elegi tu logo o avatar",
                    Descripcion = "Identifica tu marca en el menu y la vidriera.",
                    Url = "/Configuracion/DatosNegocio",
                    Icono = "image",
                    Completo = tieneLogo
                },
                new SetupStepVM
                {
                    Clave = "datos",
                    Titulo = "Completa los datos del negocio",
                    Descripcion = "Nombre, telefono, email y direccion de contacto.",
                    Url = "/Configuracion/DatosNegocio",
                    Icono = "shop",
                    Completo = tieneDatos
                }
            };
        }
    }
}