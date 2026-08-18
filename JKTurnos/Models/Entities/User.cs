namespace JKTurnos.Models.Entities
{
    public class User : BaseEntity, ITenantEntity
    {
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Nombre { get; set; }

        public string Role { get; set; }

        public int TenantId { get; set; }
    }
}