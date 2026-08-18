namespace JKTurnos.Models.Entities
{
    /// <summary>
    /// D-023: Roles como constantes + atributos, sin tabla de permisos.
    /// Autorizacion con [Authorize(Roles = Roles.AdminGlobal)] y
    /// User.IsInRole(...) en vistas. RBAC completo queda a futuro.
    /// </summary>
    public static class Roles
    {
        public const string AdminGlobal = "AdminGlobal";
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
