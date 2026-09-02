using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JKTurnos.Models.Entities
{
    // D-042: configuracion de la Vidriera Publica del tenant (M-13). 1:1 con Tenant.
    // Datos manuales de la vidriera; los datos operativos (identidad, servicios,
    // precios, disponibilidad) se consultan en tiempo real de las entidades
    // existentes, no se duplican.
    public class VidrieraPublica : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        // Identificador publico unico (lower, sin espacios). NO es un ID interno.
        // Es la unica llave publica de resolucion: /vidriera/{slug}.
        [Required]
        [StringLength(100)]
        public string Slug { get; set; }

        // Interruptor de publicacion. INDEPENDIENTE del contenido: precios,
        // ofertas, mensaje y disponibilidad son opcionales y su ausencia nunca
        // impide publicar.
        public bool Habilitada { get; set; }

        // Contenido manual (opcional).
        [StringLength(150)]
        public string Titulo { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [StringLength(500)]
        public string MensajePrincipal { get; set; }

        [StringLength(150)]
        public string OfertaTitulo { get; set; }

        [StringLength(500)]
        public string OfertaDescripcion { get; set; }

        // Flags de visibilidad de secciones.
        public bool MostrarServicios { get; set; }

        public bool MostrarPrecios { get; set; }

        public bool MostrarDisponibilidad { get; set; }

        public bool MostrarOfertas { get; set; }

        public bool MostrarMensaje { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
    }
}
