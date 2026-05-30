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

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
    }
}