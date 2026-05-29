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

        public bool Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
                return false;

            if (!PasswordHasher.Verify(password, user.PasswordHash))
                return false;

            SignIn(user);

            return true;
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