using System;
using System.Linq;
using System.Text.RegularExpressions;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// Modulo Notificaciones al Cliente (D-033), Fase 1: WhatsApp via link `wa.me`.
    /// Construye el mensaje desde la plantilla de `ConfiguracionTenant.WhatsAppTemplate`
    /// (placeholders {nombre}, {servicio}, {fecha}, {hora}) y genera el link que el
    /// negocio abre con el mensaje prellenado. El envio lo dispara el negocio; la
    /// automatizacion queda para la Fase 3.
    /// </summary>
    public class NotificacionService
    {
        public const string DefaultTemplate = "Hola {nombre}! Te recordamos tu turno de {servicio} el {fecha} a las {hora}. Te esperamos!";

        private readonly ConfiguracionService _configService;

        public NotificacionService()
        {
            _configService = new ConfiguracionService();
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

            var nombre = turno.Cliente != null ? turno.Cliente.Nombre : "cliente";
            var servicio = turno.Servicio != null ? turno.Servicio.Nombre : "tu turno";
            var fecha = turno.FechaHoraInicio.ToString("dd/MM/yyyy");
            var hora = turno.FechaHoraInicio.ToString("HH:mm");

            var mensaje = plantilla;
            mensaje = ReemplazarToken(mensaje, "{nombre}", nombre);
            mensaje = ReemplazarToken(mensaje, "{servicio}", servicio);
            mensaje = ReemplazarToken(mensaje, "{fecha}", fecha);
            mensaje = ReemplazarToken(mensaje, "{hora}", hora);
            return mensaje;
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