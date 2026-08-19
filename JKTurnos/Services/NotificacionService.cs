using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// Modulo Notificaciones al Cliente (D-033).
    /// Fase 1: WhatsApp via link `wa.me` (mensaje prellenado desde la plantilla).
    /// Fase 2: email SMTP (asunto y cuerpo desde plantillas propias) con envio real.
    /// Fase 3 (a futuro): automatizacion/recordatorios programados.
    /// Placeholders soportados: {nombre}, {servicio}, {fecha}, {hora}.
    /// </summary>
    public class NotificacionService
    {
        public const string DefaultTemplate = "Hola {nombre}! Te recordamos tu turno de {servicio} el {fecha} a las {hora}. Te esperamos!";

        public const string DefaultEmailAsunto = "Recordatorio de tu turno: {servicio}";
        public const string DefaultEmailTemplate = "Hola {nombre}! Te recordamos tu turno de {servicio} el {fecha} a las {hora}. Te esperamos!";

        private readonly ConfiguracionService _configService;
        private readonly TurnoService _turnoService;

        public NotificacionService()
        {
            _configService = new ConfiguracionService();
            _turnoService = new TurnoService();
        }

        /// <summary>
        /// Genera el link `wa.me` con el mensaje prellenado para el cliente del turno.
        /// Devuelve null si el cliente no tiene telefono (no se puede notificar).
        /// </summary>
        public string GenerarLinkWhatsApp(Turno turno)
        {
            if (turno == null || turno.Cliente == null)
                return null;

            var telefono = NormalizarTelefono(turno.Cliente.Telefono);
            if (string.IsNullOrEmpty(telefono))
                return null;

            var mensaje = GenerarMensaje(turno);
            return "https://wa.me/" + telefono + "?text=" + Uri.EscapeDataString(mensaje);
        }

        public string GenerarMensaje(Turno turno)
        {
            var config = _configService.GetConfiguracion();
            var plantilla = config != null && !string.IsNullOrWhiteSpace(config.WhatsAppTemplate)
                ? config.WhatsAppTemplate
                : DefaultTemplate;

            return RellenarPlantilla(turno, plantilla);
        }

        public string GenerarAsuntoEmail(Turno turno)
        {
            var config = _configService.GetConfiguracion();
            var asunto = config != null && !string.IsNullOrWhiteSpace(config.EmailAsunto)
                ? config.EmailAsunto
                : DefaultEmailAsunto;

            return RellenarPlantilla(turno, asunto);
        }

        public string GenerarCuerpoEmail(Turno turno)
        {
            var config = _configService.GetConfiguracion();
            var cuerpo = config != null && !string.IsNullOrWhiteSpace(config.EmailTemplate)
                ? config.EmailTemplate
                : DefaultEmailTemplate;

            return RellenarPlantilla(turno, cuerpo);
        }

        /// <summary>
        /// Fase 2: envia el recordatorio por email al cliente del turno usando la
        /// configuracion SMTP del negocio. Lanza excepciones con mensajes claros:
        /// ArgumentException si faltan datos, InvalidOperationException si el SMTP
        /// no esta habilitado o configurado, o si falla el envio.
        /// </summary>
        public void EnviarEmail(int turnoId)
        {
            var turno = _turnoService.GetById(turnoId);

            if (turno == null)
                throw new ArgumentException("Turno no encontrado.");

            if (turno.Cliente == null || string.IsNullOrWhiteSpace(turno.Cliente.Email))
                throw new InvalidOperationException("El cliente no tiene email cargado.");

            var config = _configService.GetConfiguracion();

            if (config == null || !config.SmtpHabilitado)
                throw new InvalidOperationException("El envio por email no esta habilitado. Activelo en Configuracion.");

            if (string.IsNullOrWhiteSpace(config.SmtpServidor) || string.IsNullOrWhiteSpace(config.SmtpUsuario))
                throw new InvalidOperationException("Faltan los datos del servidor SMTP. Configurelos en Configuracion.");

            var desde = !string.IsNullOrWhiteSpace(config.EmailDesde)
                ? config.EmailDesde
                : config.SmtpUsuario;

            using (var mensaje = new MailMessage())
            {
                mensaje.From = new MailAddress(desde, config.NombreComercial);
                mensaje.To.Add(turno.Cliente.Email);
                mensaje.Subject = GenerarAsuntoEmail(turno);
                mensaje.Body = GenerarCuerpoEmail(turno);
                mensaje.IsBodyHtml = false;

                using (var cliente = new SmtpClient())
                {
                    cliente.Host = config.SmtpServidor.Trim();
                    cliente.Port = config.SmtpPuerto.GetValueOrDefault(587);
                    cliente.EnableSsl = true;
                    cliente.Credentials = new NetworkCredential(config.SmtpUsuario, config.SmtpPassword ?? "");
                    cliente.Send(mensaje);
                }
            }
        }

        private string RellenarPlantilla(Turno turno, string plantilla)
        {
            var nombre = turno.Cliente != null ? turno.Cliente.Nombre : "cliente";
            var servicio = turno.Servicio != null ? turno.Servicio.Nombre : "tu turno";
            var fecha = turno.FechaHoraInicio.ToString("dd/MM/yyyy");
            var hora = turno.FechaHoraInicio.ToString("HH:mm");

            var resultado = plantilla;
            resultado = ReemplazarToken(resultado, "{nombre}", nombre);
            resultado = ReemplazarToken(resultado, "{servicio}", servicio);
            resultado = ReemplazarToken(resultado, "{fecha}", fecha);
            resultado = ReemplazarToken(resultado, "{hora}", hora);
            return resultado;
        }

        private static string ReemplazarToken(string texto, string token, string valor)
        {
            return Regex.Replace(texto, Regex.Escape(token), valor, RegexOptions.IgnoreCase);
        }

        private static string NormalizarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return null;

            // wa.me usa formato internacional sin "+". Se descartan los separadores.
            var digits = Regex.Replace(telefono, @"[^\d]", "");
            return digits.Length == 0 ? null : digits;
        }
    }
}