using System;
using System.Data.Entity.Migrations;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;

namespace JKTurnos.Migrations
{
    internal sealed class Configuration
        : DbMigrationsConfiguration<JKContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(JKContext context)
        {
            // 1. TENANT
            var tenant = context.Tenants.FirstOrDefault(x => x.Subdominio == "local");

            if (tenant == null)
            {
                tenant = new Tenant
                {
                    Nombre = "JK Demo",
                    Subdominio = "local",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.Tenants.Add(tenant);
                context.SaveChanges();
            }

            // 2. USER
            var adminEmail = "admin@jkturnos.com";

            if (!context.Users.Any(x => x.Email == adminEmail && x.TenantId == tenant.Id))
            {
                context.Users.Add(new User
                {
                    Nombre = "Administrador",
                    Email = adminEmail,
                    PasswordHash = PasswordHasher.Hash("admin123"),
                    Role = "Admin",
                    TenantId = tenant.Id,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }

            // 3. CONFIGURACION TENANT
            if (!context.ConfiguracionesTenant.Any(x => x.TenantId == tenant.Id))
            {
                context.ConfiguracionesTenant.Add(new ConfiguracionTenant
                {
                    TenantId = tenant.Id,
                    NombreComercial = "JKTurnos Demo",
                    Telefono = "",
                    Email = adminEmail,
                    Direccion = "",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            // 4. CONFIGURACION Turnos
            if (!context.EstadosTurno.Any())
            {
                context.EstadosTurno.Add(
                    new EstadoTurno
                    {
                        TenantId = tenant.Id,
                        Nombre = "Pendiente",
                        Color = "#ffc107",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });

                context.EstadosTurno.Add(
                    new EstadoTurno
                    {
                        TenantId = tenant.Id,
                        Nombre = "Confirmado",
                        Color = "#0d6efd",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });

                context.EstadosTurno.Add(
                    new EstadoTurno
                    {
                        TenantId = tenant.Id,
                        Nombre = "Atendido",
                        Color = "#198754",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });

                context.SaveChanges();
            }

            context.SaveChanges();
        }
    }
}