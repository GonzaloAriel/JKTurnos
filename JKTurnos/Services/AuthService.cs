using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;
using Microsoft.Owin.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace JKTurnos.Services
{
    public class AuthService
    {
        private readonly JKContext _context;

        // C-2: Brute-force protection — lockout despues de 5 intentos fallidos en 15 min.
        private static readonly ConcurrentDictionary<string, LockoutInfo> _lockouts
            = new ConcurrentDictionary<string, LockoutInfo>(StringComparer.OrdinalIgnoreCase);

        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private class LockoutInfo
        {
            public int FailedAttempts { get; set; }
            public DateTime? LockoutEnd { get; set; }
        }

        public AuthService()
        {
            _context = new JKContext();
        }

        public User Login(string email, string password, out bool esPrimerIngreso)
        {
            esPrimerIngreso = false;

            // C-2: Verificar si la cuenta esta bloqueada.
            if (_lockouts.TryGetValue(email, out var lockout)
                && lockout.LockoutEnd.HasValue
                && DateTime.UtcNow < lockout.LockoutEnd.Value)
            {
                new AuditService().RegistrarLogin(0, null, null, null, email, false);
                return null;
            }

            // C-2: Si el lockout expiro, limpiar el registro.
            if (lockout != null && lockout.LockoutEnd.HasValue && DateTime.UtcNow >= lockout.LockoutEnd.Value)
            {
                LockoutInfo _;
                _lockouts.TryRemove(email, out _);
            }

            var user = _context.Users
                .FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                RecordFailedAttempt(email);
                new AuditService().RegistrarLogin(0, null, null, null, email, false);
                return null;
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash))
            {
                RecordFailedAttempt(email);
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

            // C-2: Login exitoso — limpiar intentos fallidos.
            LockoutInfo _clear;
            _lockouts.TryRemove(email, out _clear);

            if (user.Role != Roles.AdminGlobal)
            {
                esPrimerIngreso = !_context.Auditorias
                    .Any(a => a.UsuarioId == user.Id && a.Accion == AuditService.A_Login);
            }

            new AuditService().RegistrarLogin(user.TenantId, user.Id, user.Nombre, user.Role, user.Email, true);

            SignIn(user);

            return user;
        }

        private void RecordFailedAttempt(string email)
        {
            var info = _lockouts.AddOrUpdate(email,
                new LockoutInfo { FailedAttempts = 1 },
                (key, existing) =>
                {
                    existing.FailedAttempts++;
                    if (existing.FailedAttempts >= MaxFailedAttempts)
                        existing.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    return existing;
                });
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
