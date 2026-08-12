using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JKTurnos.Models.Entities
{
    public class RecursoServicio : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        [Required]
        public int RecursoId { get; set; }

        [ForeignKey("RecursoId")]
        public virtual Recurso Recurso { get; set; }

        [Required]
        public int ServicioId { get; set; }

        [ForeignKey("ServicioId")]
        public virtual Servicio Servicio { get; set; }
    }
}
