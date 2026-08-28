namespace JKTurnos.Models.Entities
{
    public class EstadoTurno : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        public string Nombre { get; set; }

        /// <summary>
        /// D-040: clave estable del catalogo (PENDIENTE/EN_CURSO/FINALIZADO/CANCELADO).
        /// La semantica del sistema depende de este Codigo, nunca del Nombre.
        /// </summary>
        public string Codigo { get; set; }

        public string Color { get; set; }

        /// <summary>
        /// Icono Bootstrap Icons que representa el estado.
        /// Forma geometrica unica para distinguir del color del recurso:
        /// - bi-clock        = Pendiente
        /// - bi-play-fill    = En curso
        /// - bi-check2       = Finalizado
        /// - bi-x            = Cancelado
        /// </summary>
        public string Icono { get; set; }
    }
}
