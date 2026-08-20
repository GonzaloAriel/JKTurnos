using System;

namespace JKTurnos.Models.Entities
{
    // Auditoria de acciones (quien-hizo-que-cuando), visible solo para Admin.
    // Es distinta de NotificacionSistema (feed operativo): aqui se guarda el
    // historial formal con autor, rol, IP y cambios campo a campo. El nombre
    // del usuario se denormaliza para que el registro sobreviva a una baja.
    public class Auditoria : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        public DateTime Fecha { get; set; }

        // Crear, Editar, Eliminar, CambiarEstado, Pago, Anular,
        // Login, LoginFallido, Logout, ResetPassword.
        public string Accion { get; set; }

        // Turno, Cliente, Servicio, Recurso, Usuario, Pago,
        // EstadoTurno, Configuracion, Autenticacion.
        public string Entidad { get; set; }

        public int? EntidadId { get; set; }

        public int? UsuarioId { get; set; }

        public string UsuarioNombre { get; set; }

        public string Rol { get; set; }

        // Texto legible de lo que se hizo.
        public string Detalle { get; set; }

        // Cambios campo a campo: "Campo: antes -> despues" (null si no aplica).
        public string Cambios { get; set; }

        public string Ip { get; set; }
    }
}