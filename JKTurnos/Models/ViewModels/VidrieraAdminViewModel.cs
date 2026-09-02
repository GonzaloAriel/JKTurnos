using System.ComponentModel.DataAnnotations;

namespace JKTurnos.Models.ViewModels
{
    // D-042 / Posta 5: viewmodel del PANEL de administracion de la Vidriera del
    // tenant (backoffice). Encapsula todos los campos editables de VidrieraPublica.
    // El slug se normaliza (minusculas, sin espacios) y su unicidad se valida en el
    // service; aqui solo refleja la entrada del usuario.
    public class VidrieraAdminViewModel
    {
        // Identificador publico unico (URL). Normalizado al guardar.
        [Required(ErrorMessage = "El slug es obligatorio.")]
        [StringLength(100, ErrorMessage = "El slug no puede superar los 100 caracteres.")]
        public string Slug { get; set; }

        public bool Habilitada { get; set; }

        [StringLength(150, ErrorMessage = "El titulo no puede superar los 150 caracteres.")]
        public string Titulo { get; set; }

        [StringLength(500, ErrorMessage = "La descripcion no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; }

        [StringLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
        public string MensajePrincipal { get; set; }

        [StringLength(150, ErrorMessage = "El titulo de la oferta no puede superar los 150 caracteres.")]
        public string OfertaTitulo { get; set; }

        [StringLength(500, ErrorMessage = "La descripcion de la oferta no puede superar los 500 caracteres.")]
        public string OfertaDescripcion { get; set; }

        // Flags de visibilidad de secciones.
        public bool MostrarServicios { get; set; }

        public bool MostrarPrecios { get; set; }

        public bool MostrarDisponibilidad { get; set; }

        public bool MostrarOfertas { get; set; }

        public bool MostrarMensaje { get; set; }

        // True si la vidriera ya tiene fila (se edita); false si es la primera vez
        // (se crea al guardar). No se persiste.
        public bool YaExiste { get; set; }

        // URL publica de la vidriera para el boton de preview (no se persiste).
        public string UrlPublica { get; set; }
    }
}
