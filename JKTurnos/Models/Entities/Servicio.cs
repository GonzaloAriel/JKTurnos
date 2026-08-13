using System;
using System.ComponentModel.DataAnnotations;

namespace JKTurnos.Models.Entities
{
    public class Servicio : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La duracion debe ser mayor a 0")]
        public int DuracionMinutos { get; set; }
    }
}