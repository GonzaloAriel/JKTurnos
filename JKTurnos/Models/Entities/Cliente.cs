using System.ComponentModel.DataAnnotations;

namespace JKTurnos.Models.Entities
{
    public class Cliente : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        [Required]
        [StringLength(120)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Telefono { get; set; }

        [StringLength(120)]
        public string Email { get; set; }

        [StringLength(250)]
        public string Direccion { get; set; }

        public string Notas { get; set; }
    }
}