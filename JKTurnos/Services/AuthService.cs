using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace JKTurnos.Services
{
    public class AuthService
    {
        private readonly JKContext _context;

        public AuthService()
        {
            _context = new JKContext();
        }

        /// <summary>
        /// Autentica un usuario. Retorna el User si el login es valido,
        /// o null si falla. El controller debe usar el User retornado
        /// (no User.IsInRole()) porque en OWIN la cookie se escribe en
        /// la respuesta pero HttpContext.User no se actualiza hasta la
        /// proxima request.
        /// </summary>
        public User Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                new AuditService().RegistrarLogin(0, null, null, null, email, false);
                return null;
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash))
            {
                new AuditService().RegistrarLogin(user.TenantId, user.Id, user.Nombre, user.Role, user.Email, false);
                return null;
            }

            if (user.TenantId != 0)
            {
                var tenant = _context.Tenants.FirstOrDefault(t => t.Id == user.TenantId);

                if (tenant == null || !tenant.IsActive)
                {
                    new AuditService().RegistrarLogin(user.TenantId, user.Id, user.Nombre, user.Role, user.Email, false);
                    return null;
                }
            }

            new AuditService().RegistrarLogin(user.TenantId, user.Id, user.Nombre, user.Role, user.Email, true);

            SignIn(user);

            return user;
        }

        private void SignIn(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("TenantId", user.TenantId.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                "ApplicationCookie");

            HttpContext.Current
                .GetOwinContext()
                .Authentication
                .SignIn(identity);
        }
    }
}
