using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JKTurnos.Models.Entities
{
    public class ConfiguracionTenant : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(150)]
        public string NombreComercial { get; set; }

        [StringLength(50)]
        public string Telefono { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(250)]
        public string Direccion { get; set; }

        [StringLength(500)]
        public string LogoUrl { get; set; }

        // D-033: plantilla del recordatorio por WhatsApp (placeholders: {nombre}, {servicio}, {fecha}, {hora}).
        [StringLength(500)]
        public string WhatsAppTemplate { get; set; }

        // D-033 Fase 2: recordatorio por EMAIL SMTP. El asunto y el cuerpo admiten
        // los mismos placeholders que WhatsApp. SmtpHabilitado enciende el envio
        // (el boton del drawer); sin datos SMTP el envio falla con mensaje claro.
        [StringLength(200)]
        public string EmailAsunto { get; set; }

        [StringLength(1000)]
        public string EmailTemplate { get; set; }

        [StringLength(150)]
        public string EmailDesde { get; set; }

        [StringLength(200)]
        public string SmtpServidor { get; set; }

        public int? SmtpPuerto { get; set; }

        [StringLength(150)]
        public string SmtpUsuario { get; set; }

        [StringLength(150)]
        public string SmtpPassword { get; set; }

        public bool SmtpHabilitado { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
    }
}