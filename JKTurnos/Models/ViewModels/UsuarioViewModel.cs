using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace JKTurnos.Models.ViewModels
{
    /// <summary>
    /// D-023: Usuario con Password en claro (jamas se bindea PasswordHash).
    /// Tenants/Roles solo se llenan para AdminGlobal; Admin crea solo rol User
    /// en su tenant.
    /// </summary>
    public class UsuarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es valido")]
        public string Email { get; set; }

        public string Password { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Role { get; set; }

        public int? TenantId { get; set; }

        public IEnumerable<SelectListItem> Tenants { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }
    }
}