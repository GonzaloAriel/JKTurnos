using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// D-022: CRUD de usuarios con scope por rol (lee claims).
    /// - AdminGlobal: cualquier tenant, roles Admin/User/AdminGlobal.
    /// - Admin: solo su tenant y solo rol User.
    /// Asigna TenantId y hashea el password server-side (nunca se bindea
    /// PasswordHash). Protege: Admin no toca AdminGlobal; nadie elimina su
    /// propia cuenta; email unico.
    /// </summary>
    public class UserService
    {
        private readonly GlobalRepository<User> _repo;

        public UserService()
        {
            _repo = new GlobalRepository<User>();
        }

        private int CurrentTenantId
        {
            get { return new TenantProvider().GetTenantId(); }
        }

        private int CurrentUserId
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                var claim = identity != null ? identity.FindFirst(ClaimTypes.NameIdentifier) : null;
                return claim != null ? int.Parse(claim.Value) : 0;
            }
        }

        private string CurrentRole
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
                var claim = identity != null ? identity.FindFirst(ClaimTypes.Role) : null;
                return claim != null ? claim.Value : null;
            }
        }

        public List<User> GetAll(int? tenantId = null)
        {
            // M-11: fail-closed. Solo AdminGlobal puede ver usuarios de otros tenants.
            // Admin siempre ve solo su tenant; cualquier otro rol queda sin acceso.
            if (CurrentRole == Roles.Admin)
                return _repo.GetAll().Where(u => u.TenantId == CurrentTenantId)
                    .OrderBy(u => u.Nombre).ToList();

            if (CurrentRole == Roles.AdminGlobal)
            {
                var query = _repo.GetAll();
                if (tenantId.HasValue)
                    query = query.Where(u => u.TenantId == tenantId.Value);
                return query.OrderBy(u => u.Nombre).ToList();
            }

            return _repo.GetAll().Where(u => u.TenantId == CurrentTenantId)
                .OrderBy(u => u.Nombre).ToList();
        }

        public User GetById(int id)
        {
            var user = _repo.GetById(id);

            if (user == null)
                return null;

            if (CurrentRole == Roles.Admin && (user.TenantId != CurrentTenantId || user.Role != Roles.User))
                return null;

            return user;
        }

        public void Create(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("El email es obligatorio.");

            PasswordPolicy.Validar(password);

            user.Email = user.Email.Trim();

            if (CurrentRole == Roles.Admin)
            {
                user.TenantId = CurrentTenantId;
                user.Role = Roles.User;
            }
            else if (CurrentRole == Roles.AdminGlobal)
            {
                if (user.TenantId <= 0)
                    throw new ArgumentException("Debe indicar un tenant.");

                // L-3: separacion de deberes. Un AdminGlobal NO puede crear otro
                // AdminGlobal (evita clonar el power). Solo Admin/User via UI.
                if (user.Role != Roles.Admin && user.Role != Roles.User)
                    throw new ArgumentException("Rol invalido.");
            }
            else
            {
                throw new UnauthorizedAccessException("No tiene permisos para crear usuarios.");
            }

            if (_repo.GetAll().Any(u => u.Email == user.Email))
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            user.PasswordHash = PasswordHasher.Hash(password);
            _repo.Add(user);

            new AuditService().Registrar(
                AuditService.A_Crear,
                AuditService.E_Usuario,
                "Creo el usuario " + (user.Nombre ?? user.Email) +
                " (rol " + user.Role + ", tenant " + user.TenantId + ")",
                user.Id);
        }

        public void Update(User user, string password = null)
        {
            var existente = GetById(user.Id);

            if (existente == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            var antesNombre = existente.Nombre;
            var antesEmail = existente.Email;
            var antesRol = existente.Role;
            var antesTenant = existente.TenantId;

            user.Email = (user.Email ?? "").Trim();

            if (CurrentRole == Roles.Admin)
            {
                if (existente.Role != Roles.User || existente.TenantId != CurrentTenantId)
                    throw new UnauthorizedAccessException("No puede modificar este usuario.");

                existente.Nombre = user.Nombre;
                existente.Email = user.Email;
                existente.Role = Roles.User;
                existente.TenantId = CurrentTenantId;
            }
            else if (CurrentRole == Roles.AdminGlobal)
            {
                if (user.TenantId <= 0)
                    throw new ArgumentException("Debe indicar un tenant.");

                // L-3: separacion de deberes. Un AdminGlobal NO puede promover a otro
                // AdminGlobal (ni a si mismo, ni clonar el power). Solo Admin/User.
                if (existente.Role == Roles.AdminGlobal)
                    throw new UnauthorizedAccessException("No puede modificar un Administrador Global.");

                if (user.Role != Roles.Admin && user.Role != Roles.User)
                    throw new ArgumentException("Rol invalido.");

                existente.Nombre = user.Nombre;
                existente.Email = user.Email;
                existente.Role = user.Role;
                existente.TenantId = user.TenantId;
            }

            if (_repo.GetAll().Any(u => u.Email == existente.Email && u.Id != existente.Id))
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            if (!string.IsNullOrWhiteSpace(password))
            {
                PasswordPolicy.Validar(password);
                existente.PasswordHash = PasswordHasher.Hash(password);
            }

            _repo.Update(existente);

            var cambios = AuditService.FormatearCambios(
                AuditService.Campo("Nombre", antesNombre, existente.Nombre),
                AuditService.Campo("Email", antesEmail, existente.Email),
                AuditService.Campo("Rol", antesRol, existente.Role),
                AuditService.Campo("Tenant", antesTenant.ToString(), existente.TenantId.ToString()),
                AuditService.Campo("Password", string.IsNullOrWhiteSpace(password) ? null : "(no se registra)", string.IsNullOrWhiteSpace(password) ? null : "(cambiado)"));

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Usuario,
                "Edito el usuario " + (existente.Nombre ?? existente.Email),
                existente.Id,
                cambios);
        }

        public void Delete(int id)
        {
            var user = GetById(id);

            if (user == null)
                return;

            if (CurrentRole == Roles.Admin && user.Role == Roles.AdminGlobal)
                throw new UnauthorizedAccessException("No puede eliminar un Administrador Global.");

            // L-3: un AdminGlobal tampoco puede eliminar a otro AdminGlobal.
            if (CurrentRole == Roles.AdminGlobal && user.Role == Roles.AdminGlobal)
                throw new UnauthorizedAccessException("No puede eliminar un Administrador Global.");

            if (id == CurrentUserId)
                throw new InvalidOperationException("No puedes eliminar tu propia cuenta.");

            _repo.Delete(id);

            new AuditService().Registrar(
                AuditService.A_Eliminar,
                AuditService.E_Usuario,
                "Elimino el usuario " + (user.Nombre ?? user.Email),
                id);
        }

        public void ResetPassword(int userId, string newPassword)
        {
            var user = GetById(userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("El password es obligatorio.");

            PasswordPolicy.Validar(newPassword);

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _repo.Update(user);

            new AuditService().Registrar(
                AuditService.A_ResetPassword,
                AuditService.E_Usuario,
                "Restablecio la contraseña de " + (user.Nombre ?? user.Email),
                user.Id);
        }

        public void CambiarMiPassword(int userId, string currentPassword, string newPassword)
        {
            var user = _repo.GetById(userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !PasswordHasher.Verify(currentPassword ?? "", user.PasswordHash))
                throw new ArgumentException("La contraseña actual es incorrecta.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("La nueva contraseña es obligatoria.");

            PasswordPolicy.Validar(newPassword);

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _repo.Update(user);

            new AuditService().Registrar(
                AuditService.A_ResetPassword,
                AuditService.E_Usuario,
                "Cambio su propia contraseña",
                user.Id);
        }
    }
}