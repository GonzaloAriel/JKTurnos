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
            if (!context.Tenants.Any())
            {
                var tenant = new Tenant
                {
                    Nombre = "JK Demo",
                    Subdominio = "local",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.Tenants.Add(tenant);

                context.SaveChanges();

                var user = new User
                {
                    Nombre = "Administrador",
                    Email = "admin@jkturnos.com",
                    PasswordHash = PasswordHasher.Hash("admin123"),
                    Role = "Admin",
                    TenantId = tenant.Id,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.Users.Add(user);

                context.SaveChanges();
            }
        }
    }
}