using System;
using System.Collections.Generic;
using JKTurnos.Models.Entities;

namespace JKTurnos.Models.ViewModels
{
    /// <summary>
    /// M-08: modelo del portal publico de reservas por subdominio. Se reconstruye en
    /// cada paso (Inicio -> FechaHora -> Datos -> Confirmacion) transportando el
    /// contexto del tenant resuelto por host y la seleccion del cliente.
    /// </summary>
    public class PortalViewModel
    {
        // ---- Tenant / negocio (branding publico) ----
        public int TenantId { get; set; }
        public string NegocioNombre { get; set; }
        public string NegocioLogo { get; set; }
        public string NegocioTelefono { get; set; }
        public string NegocioDireccion { get; set; }

        // ---- Paso 1: recursos ----
        public List<Recurso> Recursos { get; set; } = new List<Recurso>();
        public int? RecursoId { get; set; }

        // ---- Paso 2: fecha/hora ----
        public string Fecha { get; set; }
        public string FechaLegible { get; set; }
        public bool SlotsConsultados { get; set; }
        public List<string> SlotsLibres { get; set; } = new List<string>();
        public string HoraSeleccionada { get; set; }

        // ---- Paso 3: datos del cliente ----
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Observaciones { get; set; }

        // ---- Paso 4: confirmacion ----
        public bool ReservaCreada { get; set; }
        public int TurnoId { get; set; }
        public string WhatsAppLink { get; set; }
        public DateTime? FechaHoraSeleccionada { get; set; }
        public string FechaHoraLegible { get; set; }
        public string RecursoNombre { get; set; }

        // ---- Meta ----
        public string Subdominio { get; set; }
    }
}