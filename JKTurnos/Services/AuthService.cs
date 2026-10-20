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

        private class LockoutInfo
        {
            public int FailedAttempts { get; set; }
            public DateTime? LockoutEnd { get; set; }
            public DateTime LastAttempt { get; set; }
        }

        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        // Ventana tras la cual un registro de intentos sin lockout activo se
        // considera obsoleto y se purga (evita crecimiento sin limite del dict).
        private static readonly TimeSpan LimpiezaIntentosObsoletos = TimeSpan.FromMinutes(30);

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

            // Emails normalizados a minusculas al persistir. La busqueda usa
            // SqlFunctions.Lower para no depender de la collation de SQL y tolerar
            // filas legacy con mayusculas, sin traer toda la tabla a memoria.
            var emailNormalizado = (email ?? "").Trim().ToLowerInvariant();

            var user = _context.Users
                .FirstOrDefault(x => x.Email.ToLower() == emailNormalizado);

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

            // Cuenta desactivada: un usuario dado de baja no puede iniciar sesion.
            if (!user.IsActive)
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
            // Limpieza oportunista: evita que el diccionario crezca sin limite
            // con emails inexistentes (branda de memoria / DoS de baja severidad).
            LimpiarLockoutsExpirados();

            var ahora = DateTime.UtcNow;

            var info = _lockouts.AddOrUpdate(email,
                new LockoutInfo { FailedAttempts = 1, LastAttempt = ahora },
                (key, existing) =>
                {
                    existing.FailedAttempts++;
                    existing.LastAttempt = ahora;
                    if (existing.FailedAttempts >= MaxFailedAttempts)
                        existing.LockoutEnd = ahora.Add(LockoutDuration);
                    return existing;
                });
        }

        // Purga entradas obsoletas: lockout ya vencido, o intentos sin lockout
        // inactivos por mas de la ventana. Nunca toca intentos recientes (no
        // resetea el contador de brute-force en curso).
        private static void LimpiarLockoutsExpirados()
        {
            var ahora = DateTime.UtcNow;

            foreach (var par in _lockouts)
            {
                var info = par.Value;

                var expirado =
                    (info.LockoutEnd.HasValue && ahora >= info.LockoutEnd.Value)
                    || (!info.LockoutEnd.HasValue
                        && ahora - info.LastAttempt >= LimpiezaIntentosObsoletos);

                if (expirado)
                {
                    LockoutInfo _;
                    _lockouts.TryRemove(par.Key, out _);
                }
            }
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
