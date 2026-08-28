namespace JKTurnos.Models.Entities
{
    /// <summary>
    /// D-040: catalogo FIJO de estados operativos del Turno, administrado por el
    /// sistema. No configurables por tenant. La semantica depende del Codigo
    /// estable (nunca del Nombre, que es solo display).
    /// Cada estado lleva su Codigo, Nombre de display, Color e Icono geometrico
    /// (para no confundir con el color del Recurso, D-004/D-011).
    /// </summary>
    public static class EstadosTurnoBase
    {
        public const string Pendiente = "PENDIENTE";
        public const string EnCurso = "EN_CURSO";
        public const string Finalizado = "FINALIZADO";
        public const string Cancelado = "CANCELADO";

        /// <summary>
        /// Descripcion de los 4 estados del catalogo: Codigo, Nombre (display),
        /// Color e Icono Bootstrap Icons.
        /// </summary>
        public static readonly EstadoTurnoBase[] Todos =
        {
            new EstadoTurnoBase(Pendiente, "Pendiente", "#ffc107", "bi-clock"),
            new EstadoTurnoBase(EnCurso,  "En curso",   "#0d6efd", "bi-play-fill"),
            new EstadoTurnoBase(Finalizado,"Finalizado","#198754", "bi-check2"),
            new EstadoTurnoBase(Cancelado, "Cancelado", "#dc3545", "bi-x")
        };
    }

    /// <summary>
    /// Item del catalogo de estados: valor estable referenciado por el sistema
    /// (Codigo) + datos de display (Nombre/Color/Icono).
    /// </summary>
    public class EstadoTurnoBase
    {
        public EstadoTurnoBase(string codigo, string nombre, string color, string icono)
        {
            Codigo = codigo;
            Nombre = nombre;
            Color = color;
            Icono = icono;
        }

        public string Codigo { get; }
        public string Nombre { get; }
        public string Color { get; }
        public string Icono { get; }
    }
}
