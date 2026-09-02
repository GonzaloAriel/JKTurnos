namespace JKTurnos.Models.ViewModels
{
    // D-042 / Posta 3: item de servicio publico de la Vidriera. No expone Id,
    // TenantId ni campos internos de Servicio (contrato seccion 5).
    public class VidrieraServicioVM
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }

        // Conveniencia para la vista: se resuelve una sola vez en el service.
        public bool MostrarPrecio { get; set; }
    }
}
