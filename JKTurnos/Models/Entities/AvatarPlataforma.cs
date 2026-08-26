using System.ComponentModel.DataAnnotations;

namespace JKTurnos.Models.Entities
{
    public class AvatarPlataforma : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(500)]
        public string Url { get; set; }
    }
}
