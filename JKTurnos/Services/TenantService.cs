using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;

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

        /// <summary>
        /// AG-5: suscripcion del tenant calculada para mostrar (card de Configuracion).
        /// Solo lectura: aplica la derivacion de estado congelada en AG-0 (D-038:
        /// nunca persiste ni bloquea acceso).
        /// </summary>
        public SuscripcionTenantViewModel GetSuscripcionTenant(int tenantId)
        {
            var tenant = _tenantRepo.GetById(tenantId);
            return BuildSuscripcionVm(tenant, DateTime.Today);
        }

        /// <summary>
        /// AG-7: suscripciones de TODOS los tenants para la grilla del Admin Global.
        /// Un solo query (sin N+1): cada VM se construye desde la entidad ya cargada.
        /// </summary>
        public List<SuscripcionTenantViewModel> GetSuscripciones()
        {
            var hoy = DateTime.Today;

            return _tenantRepo.GetAll()
                              .OrderBy(t => t.Nombre)
                              .ToList()
                              .Select(t => BuildSuscripcionVm(t, hoy))
                              .ToList();
        }

        /// <summary>
        /// AG-7: panel completo (grilla + contadores por estado) del Admin Global.
        /// </summary>
        public PanelTenantsViewModel GetPanelTenants()
        {
            var tenants = GetSuscripciones();

            return new PanelTenantsViewModel
            {
                Tenants = tenants,
                TotalActivos = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoActivo),
                TotalPorVencer = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoPorVencer),
                TotalEnGracia = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoEnGracia),
                TotalVencidos = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoVencido),
                TotalSuspendidos = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoSuspendidoManual),
                TotalSinPlan = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoSinPlan)
            };
        }

        private static SuscripcionTenantViewModel BuildSuscripcionVm(Tenant tenant, DateTime hoy)
        {
            var vm = new SuscripcionTenantViewModel
            {
                Estado = PlanesSuscripcion.EstadoSinPlan,
                TiempoRestanteTexto = "Sin plan asignado"
            };

            if (tenant == null)
                return vm;

            vm.TenantId = tenant.Id;
            vm.Nombre = tenant.Nombre;
            vm.Subdominio = tenant.Subdominio;
            vm.SuspendidoManual = tenant.SuspendidoManual;
            vm.PlanCodigo = tenant.PlanCodigo;
            vm.FechaInicioPeriodo = tenant.FechaInicioPeriodo;
            vm.FechaVencimiento = tenant.FechaVencimiento;
            vm.Estado = PlanesSuscripcion.CalcularEstado(tenant.SuspendidoManual, tenant.PlanCodigo, tenant.FechaVencimiento, hoy);

            PlanSuscripcion plan = null;

            if (!string.IsNullOrEmpty(tenant.PlanCodigo))
            {
                foreach (var p in PlanesSuscripcion.Todos)
                {
                    if (p.Codigo == tenant.PlanCodigo)
                    {
                        plan = p;
                        break;
                    }
                }
            }

            vm.PlanNombre = plan != null ? plan.Nombre : null;

            if (vm.Estado == PlanesSuscripcion.EstadoSuspendidoManual)
            {
                vm.TiempoRestanteTexto = "Cuenta suspendida manualmente";
                return vm;
            }

            if (!tenant.FechaVencimiento.HasValue)
                return vm;

            var dias = (tenant.FechaVencimiento.Value.Date - hoy).Days;
            vm.DiasRestantes = dias;

            if (vm.Estado == PlanesSuscripcion.EstadoEnGracia || vm.Estado == PlanesSuscripcion.EstadoVencido)
                vm.TiempoRestanteTexto = "Vencido hace " + Math.Abs(dias) + " dias";
            else
                vm.TiempoRestanteTexto = dias + " dias restantes";

            return vm;
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

        /// <summary>
        /// D-038 / AG-2: suspension SOLO manual e informativa. Toca unicamente
        /// SuspendidoManual; NUNCA modifica IsActive, fechas de suscripcion ni pagos.
        /// El corte real de uso se hace desactivando usuarios desde UsuariosController.
        /// </summary>
        public void Desactivar(int id)
        {
            SetSuspendidoManual(id, true);
        }

        /// <summary>
        /// D-038 / AG-2: quita la suspension manual. NO modifica fechas ni pagos;
        /// para extender vigencia se registra un pago (AG-4).
        /// </summary>
        public void Activar(int id)
        {
            SetSuspendidoManual(id, false);
        }

        private void SetSuspendidoManual(int id, bool suspendido)
        {
            var tenant = _tenantRepo.GetById(id);

            if (tenant == null)
                return;

            if (tenant.SuspendidoManual == suspendido)
                return;

            tenant.SuspendidoManual = suspendido;
            _tenantRepo.Update(tenant);
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