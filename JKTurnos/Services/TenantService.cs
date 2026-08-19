using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// D-022: CRUD de Tenants via GlobalRepository (Tenant no implementa
    /// ITenantEntity y no pertenece a ningun negocio). Reglas: subdominio
    /// unico (normalizado a minusculas) y alta opcional del primer Admin
    /// del tenant al crear.
    /// </summary>
    public class TenantService
    {
        private readonly GlobalRepository<Tenant> _tenantRepo;
        private readonly GlobalRepository<User> _userRepo;
        private readonly GlobalRepository<EstadoTurno> _estadoRepo;
        private readonly GlobalRepository<ConfiguracionTenant> _configRepo;

        public TenantService()
        {
            _tenantRepo = new GlobalRepository<Tenant>();
            _userRepo = new GlobalRepository<User>();
            _estadoRepo = new GlobalRepository<EstadoTurno>();
            _configRepo = new GlobalRepository<ConfiguracionTenant>();
        }

        public List<Tenant> GetAll()
        {
            return _tenantRepo.GetAll()
                              .OrderBy(x => x.Nombre)
                              .ToList();
        }

        public Tenant GetById(int id)
        {
            return _tenantRepo.GetById(id);
        }

        public void Create(Tenant tenant, string adminNombre = null, string adminEmail = null, string adminPassword = null)
        {
            if (string.IsNullOrWhiteSpace(tenant.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(tenant.Subdominio))
                throw new ArgumentException("El subdominio es obligatorio.");

            tenant.Subdominio = tenant.Subdominio.Trim().ToLower();

            if (_tenantRepo.GetAll().Any(x => x.Subdominio == tenant.Subdominio))
                throw new InvalidOperationException("Ya existe un tenant con ese subdominio.");

            _tenantRepo.Add(tenant);

            // Datos base del negocio: sin estados de turno no se pueden crear turnos.
            SeedBaseData(tenant, adminEmail);

            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                if (string.IsNullOrWhiteSpace(adminPassword))
                    throw new InvalidOperationException("Debe indicar un password para el administrador.");

                adminEmail = adminEmail.Trim();

                if (_userRepo.GetAll().Any(u => u.Email == adminEmail))
                    throw new InvalidOperationException("Ya existe un usuario con ese email.");

                _userRepo.Add(new User
                {
                    Nombre = string.IsNullOrWhiteSpace(adminNombre) ? tenant.Nombre : adminNombre.Trim(),
                    Email = adminEmail,
                    PasswordHash = PasswordHasher.Hash(adminPassword),
                    Role = Roles.Admin,
                    TenantId = tenant.Id
                });
            }
        }

        public void Update(Tenant tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(tenant.Subdominio))
                throw new ArgumentException("El subdominio es obligatorio.");

            tenant.Subdominio = tenant.Subdominio.Trim().ToLower();

            if (_tenantRepo.GetAll().Any(x => x.Subdominio == tenant.Subdominio && x.Id != tenant.Id))
                throw new InvalidOperationException("Ya existe un tenant con ese subdominio.");

            _tenantRepo.Update(tenant);
        }

        public void Delete(int id)
        {
            _tenantRepo.Delete(id);
        }

        private void SeedBaseData(Tenant tenant, string adminEmail)
        {
            var estados = new[]
            {
                new { Nombre = "Pendiente",  Color = "#ffc107" },
                new { Nombre = "Confirmado", Color = "#0d6efd" },
                new { Nombre = "Atendido",   Color = "#198754" }
            };

            foreach (var e in estados)
            {
                _estadoRepo.Add(new EstadoTurno
                {
                    TenantId = tenant.Id,
                    Nombre = e.Nombre,
                    Color = e.Color,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }

            _configRepo.Add(new ConfiguracionTenant
            {
                TenantId = tenant.Id,
                NombreComercial = tenant.Nombre,
                Email = adminEmail ?? string.Empty,
                Telefono = string.Empty,
                Direccion = string.Empty,
                WhatsAppTemplate = NotificacionService.DefaultTemplate,
                EmailAsunto = NotificacionService.DefaultEmailAsunto,
                EmailTemplate = NotificacionService.DefaultEmailTemplate,
                SmtpHabilitado = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }
    }
}