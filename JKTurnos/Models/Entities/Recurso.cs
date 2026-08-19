using System.ComponentModel.DataAnnotations;

namespace JKTurnos.Models.Entities
{
    public class Recurso : BaseEntity, ITenantEntity
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public TipoRecurso Tipo { get; set; }

        [StringLength(7)]
        public string Color { get; set; } // Formato Hexadecimal
    }
}
