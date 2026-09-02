using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    // D-042 / Posta 3: viewmodel PUBLICO de la Vidriera (M-13). Solo datos
    // publicos; nunca expone Id, TenantId ni campos internos de Servicio/Turno.
    public class VidrieraPublicaViewModel
    {
        public string Slug { get; set; }

        // Identidad (desde ConfiguracionTenant, no se duplica).
        public string NombreNegocio { get; set; }
        public string LogoUrl { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }

        // Config de exhibicion (flags D-042: seccion presente SOLO si su flag).
        public bool MostrarServicios { get; set; }
        public bool MostrarPrecios { get; set; }
        public bool MostrarDisponibilidad { get; set; }
        public bool MostrarOfertas { get; set; }
        public bool MostrarMensaje { get; set; }

        // Contenido manual de la vidriera (D-042). Solo se exponen si su flag.
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string MensajePrincipal { get; set; }
        public string OfertaTitulo { get; set; }
        public string OfertaDescripcion { get; set; }

        // Contenido principal: servicios activos del tenant.
        public List<VidrieraServicioVM> Servicios { get; set; } = new List<VidrieraServicioVM>();

        // Posta 4: proximos slots libres de hoy, por recurso (si MostrarDisponibilidad).
        public List<VidrieraDisponibilidadVM> Disponibilidad { get; set; } = new List<VidrieraDisponibilidadVM>();
    }
}
