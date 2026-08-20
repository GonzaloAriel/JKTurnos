using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    // Auditoria de acciones (quien-hizo-que-cuando), visible solo para Admin.
    // Toma el usuario/tenant/IP del HttpContext cuando hay sesion; para el
    // login (pre-autenticacion) se pasa el contexto explicitamente.
    public class AuditService
    {
        public const string A_Crear = "Crear";
        public const string A_Editar = "Editar";
        public const string A_Eliminar = "Eliminar";
        public const string A_CambiarEstado = "CambiarEstado";
        public const string A_Pago = "Pago";
        public const string A_Anular = "Anular";
        public const string A_Login = "Login";
        public const string A_LoginFallido = "LoginFallido";
        public const string A_Logout = "Logout";
        public const string A_ResetPassword = "ResetPassword";

        public const string E_Turno = "Turno";
        public const string E_Cliente = "Cliente";
        public const string E_Servicio = "Servicio";
        public const string E_Recurso = "Recurso";
        public const string E_Usuario = "Usuario";
        public const string E_Pago = "Pago";
        public const string E_EstadoTurno = "EstadoTurno";
        public const string E_Configuracion = "Configuracion";
        public const string E_Autenticacion = "Autenticacion";

        // Registra una accion dentro de una sesion autenticada.
        public void Registrar(string accion, string entidad, string detalle, int? entidadId = null, string cambios = null)
        {
            var ctx = HttpContext.Current;

            int tenantId = 0;
            int? usuarioId = null;
            string usuarioNombre = null;
            string rol = null;

            if (ctx != null && ctx.User != null &&
                ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
            {
                var ident = ctx.User.Identity as ClaimsIdentity;

                if (ident != null)
                {
                    var t = ident.FindFirst("TenantId");
                    if (t != null)
                        int.TryParse(t.Value, out tenantId);

                    var u = ident.FindFirst(ClaimTypes.NameIdentifier);
                    if (u != null && int.TryParse(u.Value, out var uid))
                        usuarioId = uid;

                    usuarioNombre = ident.FindFirst(ClaimTypes.Name)?.Value;
                    rol = ident.FindFirst(ClaimTypes.Role)?.Value;
                }
            }

            Guardar(tenantId, accion, entidad, entidadId,
                usuarioId, usuarioNombre, rol, detalle, cambios, GetIp());
        }

        // Registra un intento de login (aun no hay sesion autenticada).
        public void RegistrarLogin(int tenantId, int? usuarioId, string usuarioNombre, string rol,
            string email, bool exito)
        {
            var accion = exito ? A_Login : A_LoginFallido;
            var emailTxt = string.IsNullOrWhiteSpace(email) ? "" : " - " + email.Trim();

            Guardar(tenantId, accion, E_Autenticacion, null,
                usuarioId, usuarioNombre, rol,
                (exito ? "Inicio de sesion" : "Intento de inicio de sesion fallido") + emailTxt,
                null, GetIp());
        }

        private static void Guardar(int tenantId, string accion, string entidad, int? entidadId,
            int? usuarioId, string usuarioNombre, string rol, string detalle, string cambios, string ip)
        {
            using (var ctx = new JKContext())
            {
                ctx.Auditorias.Add(new Auditoria
                {
                    TenantId = tenantId,
                    Fecha = DateTime.Now,
                    Accion = accion,
                    Entidad = entidad,
                    EntidadId = entidadId,
                    UsuarioId = usuarioId,
                    UsuarioNombre = usuarioNombre,
                    Rol = rol,
                    Detalle = detalle,
                    Cambios = cambios,
                    Ip = ip,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true
                });

                ctx.SaveChanges();
            }
        }

        private static string GetIp()
        {
            try
            {
                return HttpContext.Current != null && HttpContext.Current.Request != null
                    ? HttpContext.Current.Request.UserHostAddress
                    : null;
            }
            catch
            {
                return null;
            }
        }

        // "Campo: antes -> despues" (null si no hubo cambio real).
        public static string Campo(string nombre, string antes, string despues)
        {
            if (string.Equals(antes, despues, StringComparison.Ordinal))
                return null;

            return nombre + ": " + (antes ?? "(vacio)") + " -> " + (despues ?? "(vacio)");
        }

        // Une los cambios campo a campo en una sola linea legible.
        public static string FormatearCambios(params string[] cambios)
        {
            var lista = cambios
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();

            return lista.Count == 0 ? null : string.Join(" | ", lista);
        }
    }
}