using System;

namespace JKTurnos.Models.Entities
{
    public class Servicio : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int DuracionMinutos { get; set; }
    }
}