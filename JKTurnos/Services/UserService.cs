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
            var query = _repo.GetAll();

            if (CurrentRole == Roles.Admin)
                query = query.Where(u => u.TenantId == CurrentTenantId);
            else if (tenantId.HasValue)
                query = query.Where(u => u.TenantId == tenantId.Value);

            return query.OrderBy(u => u.Nombre).ToList();
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

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("El password es obligatorio.");

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

                if (user.Role != Roles.Admin && user.Role != Roles.User && user.Role != Roles.AdminGlobal)
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
        }

        public void Update(User user, string password = null)
        {
            var existente = GetById(user.Id);

            if (existente == null)
                throw new InvalidOperationException("Usuario no encontrado.");

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

                if (user.Role != Roles.Admin && user.Role != Roles.User && user.Role != Roles.AdminGlobal)
                    throw new ArgumentException("Rol invalido.");

                existente.Nombre = user.Nombre;
                existente.Email = user.Email;
                existente.Role = user.Role;
                existente.TenantId = user.TenantId;
            }

            if (_repo.GetAll().Any(u => u.Email == existente.Email && u.Id != existente.Id))
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            if (!string.IsNullOrWhiteSpace(password))
                existente.PasswordHash = PasswordHasher.Hash(password);

            _repo.Update(existente);
        }

        public void Delete(int id)
        {
            var user = GetById(id);

            if (user == null)
                return;

            if (CurrentRole == Roles.Admin && user.Role == Roles.AdminGlobal)
                throw new UnauthorizedAccessException("No puede eliminar un Administrador Global.");

            if (id == CurrentUserId)
                throw new InvalidOperationException("No puedes eliminar tu propia cuenta.");

            _repo.Delete(id);
        }

        public void ResetPassword(int userId, string newPassword)
        {
            var user = GetById(userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("El password es obligatorio.");

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _repo.Update(user);
        }

        public void CambiarMiPassword(int userId, string currentPassword, string newPassword)
        {
            var user = _repo.GetById(userId);

            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !PasswordHasher.Verify(currentPassword ?? "", user.PasswordHash))
                throw new ArgumentException("La contrasena actual es incorrecta.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("El nuevo password es obligatorio.");

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            _repo.Update(user);
        }
    }
}