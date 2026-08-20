namespace JKTurnos.Models.ViewModels
{
    // Recordatorio operativo de la campanita (in-app). Se calcula en cada
    // request desde datos reales (no se persiste) y desaparece cuando la
    // condicion que lo origina deja de existir.
    public class RecordatorioSistema
    {
        public string Titulo { get; set; }

        public string Mensaje { get; set; }

        // Icono de Bootstrap Icons (bi-alarm, bi-hourglass-split, etc.).
        public string Icono { get; set; }

        // Variante de color del icono: jk-notif-icon-accent/warning/danger/muted.
        public string IconoClase { get; set; }

        public string Controlador { get; set; }

        public string Accion { get; set; }

        public object RouteValues { get; set; }
    }
}