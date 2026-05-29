namespace JKTurnos.Models.Entities
{
    public class Tenant : BaseEntity
    {
        public string Nombre { get; set; }

        public string Subdominio { get; set; }
    }
}