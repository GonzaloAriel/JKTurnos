using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Services
{
    // Recordatorios operativos de la campanita (in-app): muestran lo que
    // requiere atencion (proximo turno, turnos sin confirmar, saldo por
    // cobrar, clientes sin contacto y configuracion incompleta). Se calculan
    // en cada request desde datos reales, no se persisten y se resuelven
    // solos cuando la condicion desaparece. Los de configuracion solo
    // aplican para roles Admin.
    public class RecordatorioService
    {
        private readonly TurnoService _turnoService;
        private readonly PagoService _pagoService;
        private readonly ConfiguracionService _configService;

        public RecordatorioService()
        {
            _turnoService = new TurnoService();
            _pagoService = new PagoService();
            _configService = new ConfiguracionService();
        }

        public List<RecordatorioSistema> GetRecordatorios(bool incluirConfiguracion)
        {
            var hoy = DateTime.Today;
            var ahora = DateTime.Now;

            var turnos = _turnoService.GetTurnosEnRango(hoy, hoy.AddDays(2));

            var config = _configService.GetConfiguracion();
            var ocultos = ObtenerOcultos(config);

            var recordatorios = new List<RecordatorioSistema>();

            AgregarProximoTurno(recordatorios, turnos, ahora);
            AgregarRecordatoriosDelDia(recordatorios, turnos, hoy);
            AgregarTurnosSinConfirmar(recordatorios, turnos);
            AgregarSaldoPendiente(recordatorios, hoy);
            AgregarClientesSinContacto(recordatorios, turnos, hoy);

            if (incluirConfiguracion)
                AgregarConfiguracion(recordatorios, config);

            return recordatorios
                .Where(r => !string.IsNullOrEmpty(r.Clave) && !ocultos.Contains(r.Clave))
                .ToList();
        }

        // Oculta los recordatorios actuales con "Marcar todas leidas": guarda
        // sus claves para que no vuelvan a mostrarse hasta que cambie la
        // condicion que los origina (nuevo turno, otro dia/mes, config resuelta).
        public void OcultarActuales(bool incluirConfiguracion)
        {
            var claves = GetRecordatorios(incluirConfiguracion)
                .Select(r => r.Clave)
                .ToList();

            if (claves.Count == 0)
                return;

            var config = _configService.GetConfiguracion();
            if (config == null)
                return;

            var ocultos = ObtenerOcultos(config);
            foreach (var c in claves)
                ocultos.Add(c);

            // Cap: se descartan las claves mas viejas para que la columna no
            // crezca sin limite (los turnos y dias viejos dejan de coincidir).
            var lista = ocultos.ToList();
            if (lista.Count > 100)
                lista = lista.Skip(lista.Count - 100).ToList();

            _configService.GuardarRecordatoriosOcultos(string.Join("|", lista));
        }

        // Oculta UN recordatorio (click en la "x" del item). Se persiste su
        // clave para que no vuelva a mostrarse hasta que cambie la condicion.
        public void OcultarUna(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
                return;

            var config = _configService.GetConfiguracion();
            if (config == null)
                return;

            var ocultos = ObtenerOcultos(config);
            if (!ocultos.Add(clave.Trim()))
                return;

            var lista = ocultos.ToList();
            if (lista.Count > 100)
                lista = lista.Skip(lista.Count - 100).ToList();

            _configService.GuardarRecordatoriosOcultos(string.Join("|", lista));
        }

        private static HashSet<string> ObtenerOcultos(ConfiguracionTenant config)
        {
            var ocultos = new HashSet<string>();

            if (config == null || string.IsNullOrWhiteSpace(config.RecordatoriosOcultos))
                return ocultos;

            foreach (var k in config.RecordatoriosOcultos.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                ocultos.Add(k.Trim());

            return ocultos;
        }

        private static void AgregarProximoTurno(
            List<RecordatorioSistema> recordatorios,
            List<Turno> turnos,
            DateTime ahora)
        {
            var proximo = turnos
                .Where(t => t.FechaHoraFin > ahora &&
                            (t.EstadoTurno == null || t.EstadoTurno.Nombre != "Cancelado"))
                .OrderBy(t => t.FechaHoraInicio)
                .FirstOrDefault();

            if (proximo == null)
                return;

            var cliente = proximo.Cliente != null ? proximo.Cliente.Nombre : "Sin cliente";

            var minutos = (int)(proximo.FechaHoraInicio - ahora).TotalMinutes;

            var cuando = minutos < 1
                ? "ya"
                : minutos < 60
                    ? "en " + minutos + " min"
                    : "en " + (minutos / 60) + " h " + (minutos % 60) + " min";

            recordatorios.Add(new RecordatorioSistema
            {
                Titulo = "Proximo turno",
                Mensaje = cliente + " a las " + proximo.FechaHoraInicio.ToString("HH:mm") + " (" + cuando + ").",
                Icono = "bi-alarm",
                IconoClase = "jk-notif-icon-accent",
                Clave = "proximoTurno:" + proximo.Id,
                Controlador = "Turnos",
                Accion = "Agenda",
                RouteValues = new { openTurnoId = proximo.Id }
            });
        }

        private static void AgregarRecordatoriosDelDia(
            List<RecordatorioSistema> recordatorios,
            List<Turno> turnos,
            DateTime hoy)
        {
            var ahora = DateTime.Now;

            // Turnos de HOY que todavia no arrancaron y cuyo cliente tiene
            // telefono: sugieren enviar el recordatorio por WhatsApp del dia.
            var delDia = turnos
                .Where(t => t.FechaHoraInicio.Date == hoy &&
                            t.FechaHoraFin > ahora &&
                            (t.EstadoTurno == null || t.EstadoTurno.Nombre != "Cancelado") &&
                            t.Cliente != null &&
                            !string.IsNullOrWhiteSpace(t.Cliente.Telefono))
                .OrderBy(t => t.FechaHoraInicio)
                .Take(3)
                .ToList();

            foreach (var t in delDia)
            {
                var servicio = t.Servicio != null ? t.Servicio.Nombre : "un turno";

                recordatorios.Add(new RecordatorioSistema
                {
                    Titulo = "Recordatorio por WhatsApp",
                    Mensaje = "Hoy " + t.Cliente.Nombre + " tiene " + servicio + " a las " + t.FechaHoraInicio.ToString("HH:mm") + ". Enviale un recordatorio por whatsapp.",
                    Icono = "bi-whatsapp",
                    IconoClase = "jk-notif-icon-success",
                    Clave = "diaWhatsapp:" + t.Id,
                    Controlador = "Turnos",
                    Accion = "Agenda",
                    RouteValues = new { openTurnoId = t.Id }
                });
            }
        }

        private static void AgregarTurnosSinConfirmar(
            List<RecordatorioSistema> recordatorios,
            List<Turno> turnos)
        {
            var hoy = DateTime.Today;

            var sinConfirmar = turnos.Count(t =>
                (t.FechaHoraInicio.Date == hoy || t.FechaHoraInicio.Date == hoy.AddDays(1)) &&
                t.EstadoTurno != null &&
                t.EstadoTurno.Nombre == "Pendiente");

            if (sinConfirmar <= 0)
                return;

            var turnosTxt = sinConfirmar == 1 ? "turno sin confirmar" : "turnos sin confirmar";

            recordatorios.Add(new RecordatorioSistema
            {
                Titulo = "Turnos sin confirmar",
                Mensaje = "Tenes " + sinConfirmar + " " + turnosTxt + " para hoy o manana.",
                Icono = "bi-hourglass-split",
                IconoClase = "jk-notif-icon-warning",
                Clave = "sinConfirmar:" + hoy.AddDays(1).ToString("yyyyMMdd"),
                Controlador = "Turnos",
                Accion = "Agenda",
                RouteValues = new { }
            });
        }

        private void AgregarSaldoPendiente(
            List<RecordatorioSistema> recordatorios,
            DateTime hoy)
        {
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1);

            var activosMes = _turnoService.GetTurnosEnRango(inicioMes, finMes)
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Nombre != "Cancelado")
                .ToList();

            var acumuladoPorTurno = _pagoService.GetPagosEnRango(inicioMes, finMes)
                .GroupBy(p => p.TurnoId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Monto));

            decimal saldo = 0;
            var turnosConSaldo = 0;

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

            if (turnosConSaldo <= 0)
                return;

            var turnosTxt = turnosConSaldo == 1 ? "turno" : "turnos";

            recordatorios.Add(new RecordatorioSistema
            {
                Titulo = "Saldo por cobrar",
                Mensaje = "Hay $" + saldo.ToString("0.##") + " por cobrar (" + turnosConSaldo + " " + turnosTxt + ").",
                Icono = "bi-cash-stack",
                IconoClase = "jk-notif-icon-warning",
                Clave = "saldo:" + inicioMes.ToString("yyyyMM"),
                Controlador = "Pagos",
                Accion = "Index",
                RouteValues = new { }
            });
        }

        private static void AgregarClientesSinContacto(
            List<RecordatorioSistema> recordatorios,
            List<Turno> turnos,
            DateTime hoy)
        {
            var sinContacto = turnos.Count(t =>
                t.FechaHoraInicio.Date == hoy &&
                (t.EstadoTurno == null || t.EstadoTurno.Nombre != "Cancelado") &&
                t.Cliente != null &&
                string.IsNullOrWhiteSpace(t.Cliente.Telefono) &&
                string.IsNullOrWhiteSpace(t.Cliente.Email));

            if (sinContacto <= 0)
                return;

            var turnosTxt = sinContacto == 1 ? "turno tiene" : "turnos tienen";

            recordatorios.Add(new RecordatorioSistema
            {
                Titulo = "Clientes sin contacto",
                Mensaje = sinContacto + " " + turnosTxt + " cliente sin telefono ni email. No podras enviarles recordatorios.",
                Icono = "bi-person-x",
                IconoClase = "jk-notif-icon-danger",
                Clave = "sinContacto:" + hoy.ToString("yyyyMMdd"),
                Controlador = "Turnos",
                Accion = "Agenda",
                RouteValues = new { }
            });
        }

        private void AgregarConfiguracion(List<RecordatorioSistema> recordatorios, ConfiguracionTenant config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.NombreComercial))
            {
                recordatorios.Add(new RecordatorioSistema
                {
                    Titulo = "Datos del negocio incompletos",
                    Mensaje = "Completa nombre, telefono y direccion de tu negocio.",
                    Icono = "bi-shop",
                    IconoClase = "jk-notif-icon-muted",
                    Clave = "configNegocio",
                    Controlador = "Configuracion",
                    Accion = "DatosNegocio",
                    RouteValues = new { }
                });
            }

            if (config != null && !config.SmtpHabilitado)
            {
                recordatorios.Add(new RecordatorioSistema
                {
                    Titulo = "Recordatorio por email apagado",
                    Mensaje = "Activa el envio de recordatorios por email desde Configuracion.",
                    Icono = "bi-envelope",
                    IconoClase = "jk-notif-icon-muted",
                    Clave = "configEmail",
                    Controlador = "Configuracion",
                    Accion = "NotificacionesEmail",
                    RouteValues = new { }
                });
            }

            if (config != null && string.IsNullOrWhiteSpace(config.WhatsAppTemplate))
            {
                recordatorios.Add(new RecordatorioSistema
                {
                    Titulo = "Recordatorio por WhatsApp sin configurar",
                    Mensaje = "Defini la plantilla de recordatorio por WhatsApp.",
                    Icono = "bi-chat-dots",
                    IconoClase = "jk-notif-icon-muted",
                    Clave = "configWhatsApp",
                    Controlador = "Configuracion",
                    Accion = "NotificacionesWhatsApp",
                    RouteValues = new { }
                });
            }
        }
    }
}