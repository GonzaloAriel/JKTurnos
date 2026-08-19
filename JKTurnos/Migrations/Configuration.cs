using System;
using System.Data.Entity.Migrations;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Migrations
{
    internal sealed class Configuration
        : DbMigrationsConfiguration<JKContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
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
                    PasswordHash = PasswordHasher.Hash("123456"),
                    Role = Roles.Admin,
                    TenantId = tenant.Id,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
            }

            // 2b. ADMIN GLOBAL (plataforma, sin tenant: TenantId = 0)
            var globalEmail = "global@jkturnos.com";

            if (!context.Users.Any(x => x.Email == globalEmail))
            {
                context.Users.Add(new User
                {
                    Nombre = "Administrador Global",
                    Email = globalEmail,
                    PasswordHash = PasswordHasher.Hash("123456"),
                    Role = Roles.AdminGlobal,
                    TenantId = 0,
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
                    WhatsAppTemplate = NotificacionService.DefaultTemplate,
                    EmailAsunto = NotificacionService.DefaultEmailAsunto,
                    EmailTemplate = NotificacionService.DefaultEmailTemplate,
                    SmtpHabilitado = false,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            // 4. ESTADOS DE TURNO
            if (!context.EstadosTurno.Any(x => x.TenantId == tenant.Id))
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

            // 5. CLIENTES DEMO
            var clientes = new[]
            {
                new { Nombre = "Juan Perez",    Telefono = "1122334455", Email = "juan@email.com" },
                new { Nombre = "Maria Garcia",  Telefono = "1122334466", Email = "maria@email.com" },
                new { Nombre = "Carlos Lopez",  Telefono = "1122334477", Email = "carlos@email.com" }
            };

            foreach (var c in clientes)
            {
                if (!context.Clientes.Any(x => x.Email == c.Email && x.TenantId == tenant.Id))
                {
                    context.Clientes.Add(new Cliente
                    {
                        TenantId = tenant.Id,
                        Nombre = c.Nombre,
                        Telefono = c.Telefono,
                        Email = c.Email,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });
                }
            }

            context.SaveChanges();

            // 6. SERVICIOS DEMO
            var servicios = new[]
            {
                new { Nombre = "Corte",   Descripcion = "Corte de cabello",         Precio = 3500m, DuracionMinutos = 30 },
                new { Nombre = "Color",   Descripcion = "Coloracion completa",       Precio = 8000m, DuracionMinutos = 60 },
                new { Nombre = "Peinado", Descripcion = "Peinado y styling",         Precio = 5000m, DuracionMinutos = 45 }
            };

            foreach (var s in servicios)
            {
                if (!context.Servicios.Any(x => x.Nombre == s.Nombre && x.TenantId == tenant.Id))
                {
                    context.Servicios.Add(new Servicio
                    {
                        TenantId = tenant.Id,
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        Precio = s.Precio,
                        DuracionMinutos = s.DuracionMinutos,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });
                }
            }

            context.SaveChanges();

            // 7. RECURSOS DEMO
            var recursos = new[]
            {
                new { Nombre = "Dr. Juan Martinez", Descripcion = "Profesional principal", Tipo = TipoRecurso.Profesional, Color = "#2563eb" },
                new { Nombre = "Consultorio A", Descripcion = "Consultorio principal", Tipo = TipoRecurso.Consultorio, Color = "#34c759" },
                new { Nombre = "Equipo de Odontologia", Descripcion = "Equipo dental", Tipo = TipoRecurso.Equipo, Color = "#ff9500" }
            };

            foreach (var r in recursos)
            {
                if (!context.Recursos.Any(x => x.Nombre == r.Nombre && x.TenantId == tenant.Id))
                {
                    context.Recursos.Add(new Recurso
                    {
                        TenantId = tenant.Id,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion,
                        Tipo = r.Tipo,
                        Color = r.Color,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });
                }
            }

            context.SaveChanges();

            // 8. TURNOS DEMO
            var estadoPendiente = context.EstadosTurno
                .FirstOrDefault(x => x.Nombre == "Pendiente" && x.TenantId == tenant.Id);
            var estadoConfirmado = context.EstadosTurno
                .FirstOrDefault(x => x.Nombre == "Confirmado" && x.TenantId == tenant.Id);
            var recurso = context.Recursos.FirstOrDefault(x => x.TenantId == tenant.Id);
            var hoy = DateTime.Today;

            var turnosDemo = new[]
            {
                new {
                    ClienteEmail = "juan@email.com",
                    ServicioNombre = "Corte",
                    HoraInicio = hoy.AddHours(10),
                    HoraFin = hoy.AddHours(10).AddMinutes(30),
                    Estado = estadoPendiente,
                    Obs = "Cliente nuevo"
                },
                new {
                    ClienteEmail = "maria@email.com",
                    ServicioNombre = "Color",
                    HoraInicio = hoy.AddHours(11),
                    HoraFin = hoy.AddHours(11).AddMinutes(60),
                    Estado = estadoConfirmado,
                    Obs = ""
                },
                new {
                    ClienteEmail = "carlos@email.com",
                    ServicioNombre = "Peinado",
                    HoraInicio = hoy.AddHours(14),
                    HoraFin = hoy.AddHours(14).AddMinutes(45),
                    Estado = estadoPendiente,
                    Obs = "Confirmar horario"
                }
            };

            foreach (var t in turnosDemo)
            {
                var cliente = context.Clientes.FirstOrDefault(x => x.Email == t.ClienteEmail && x.TenantId == tenant.Id);
                var servicio = context.Servicios.FirstOrDefault(x => x.Nombre == t.ServicioNombre && x.TenantId == tenant.Id);

                if (cliente == null || servicio == null || t.Estado == null || recurso == null)
                    continue;

                var existeTurno = context.Turnos.Any(x =>
                    x.ClienteId == cliente.Id &&
                    x.ServicioId == servicio.Id &&
                    x.FechaHoraInicio == t.HoraInicio &&
                    x.TenantId == tenant.Id);

                if (!existeTurno)
                {
                    context.Turnos.Add(new Turno
                    {
                        TenantId = tenant.Id,
                        ClienteId = cliente.Id,
                        ServicioId = servicio.Id,
                        RecursoId = recurso.Id,
                        EstadoTurnoId = t.Estado.Id,
                        FechaHoraInicio = t.HoraInicio,
                        FechaHoraFin = t.HoraFin,
                        Observaciones = t.Obs,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
