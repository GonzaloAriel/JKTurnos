using System.ComponentModel.DataAnnotations;

namespace JKTurnos.Models.ViewModels
{
    public class AvatarPlataformaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        public string Nombre { get; set; }

        [Display(Name = "URL de la imagen")]
        [Required(ErrorMessage = "La URL es obligatoria.")]
        [MaxLength(500, ErrorMessage = "La URL no puede superar 500 caracteres.")]
        public string Url { get; set; }
    }
}
