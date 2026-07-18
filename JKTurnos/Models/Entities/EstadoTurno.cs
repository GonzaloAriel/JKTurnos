namespace JKTurnos.Models.Entities
{
    public class EstadoTurno : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        public string Nombre { get; set; }

        public string Color { get; set; }
    }
}