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
        /// Busca un tenant incluyendo los dados de baja (IsActive = false).
        /// Excepcion justificada a D-022: GlobalRepository filtra por IsActive, por lo
        /// que para operar sobre registros inactivos (ver/reactivar bajas) se consulta
        /// el contexto directamente. Solo toca entidades de plataforma, sin TenantId.
        /// </summary>
        public Tenant GetByIdInactive(int id)
        {
            using (var context = new JKContext())
            {
                return context.Tenants.FirstOrDefault(t => t.Id == id);
            }
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
        /// AG-8: historial de pagos de suscripcion del propio tenant (para Configuracion).
        /// </summary>
        public List<PagoSuscripcion> GetPagosTenant(int tenantId)
        {
            var service = new PagoSuscripcionService();
            return service.GetByTenant(tenantId);
        }

        /// <summary>
        /// AG-7: suscripciones de TODOS los tenants para la grilla del Admin Global.
        /// Incluye tenants dados de baja (IsActive = false) para que el AdminGlobal
        /// pueda verlos, reactivarlos o gestionarlos.
        /// </summary>
        public List<SuscripcionTenantViewModel> GetSuscripciones()
        {
            var hoy = DateTime.Today;

            using (var context = new JKContext())
            {
                return context.Tenants
                              .OrderBy(t => t.Nombre)
                              .ToList()
                              .Select(t => BuildSuscripcionVm(t, hoy))
                              .ToList();
            }
        }

        /// <summary>
        /// AG-7: panel completo (grilla + contadores por estado) del Admin Global.
        /// Filtros: busqueda (nombre/subdominio) y estado (activo/desactivado).
        /// </summary>
        public PanelTenantsViewModel GetPanelTenants(string busqueda = null, string estado = null)
        {
            var tenants = GetSuscripciones();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim().ToLower();
                tenants = tenants.Where(x =>
                    (x.Nombre != null && x.Nombre.ToLower().Contains(busqueda)) ||
                    (x.Subdominio != null && x.Subdominio.ToLower().Contains(busqueda))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (estado == "activos")
                    tenants = tenants.Where(x => x.IsActive).ToList();
                else if (estado == "desactivados")
                    tenants = tenants.Where(x => !x.IsActive).ToList();
            }

            return new PanelTenantsViewModel
            {
                Tenants = tenants,
                TotalActivos = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoActivo),
                TotalPorVencer = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoPorVencer),
                TotalEnGracia = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoEnGracia),
                TotalVencidos = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoVencido),
                TotalSuspendidos = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoSuspendidoManual),
                TotalSinPlan = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoSinPlan),
                TotalBaja = tenants.Count(x => x.Estado == PlanesSuscripcion.EstadoBaja)
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
            vm.IsActive = tenant.IsActive;
            vm.PlanCodigo = tenant.PlanCodigo;
            vm.FechaInicioPeriodo = tenant.FechaInicioPeriodo;
            vm.FechaVencimiento = tenant.FechaVencimiento;

            if (!tenant.IsActive)
            {
                vm.Estado = PlanesSuscripcion.EstadoBaja;
                vm.TiempoRestanteTexto = "Tenant dado de baja";
                return vm;
            }

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
            using (var context = new JKContext())
            using (var transaction = context.Database.BeginTransaction())
            {
                var tenant = context.Tenants.FirstOrDefault(t => t.Id == id);

                if (tenant == null)
                    return;

                var now = DateTime.Now;

                var users = context.Users.Where(u => u.TenantId == id).ToList();
                foreach (var u in users)
                {
                    u.IsActive = false;
                    u.UpdatedAt = now;
                }

                var config = context.ConfiguracionesTenant.Where(c => c.TenantId == id).ToList();
                foreach (var c in config)
                {
                    c.IsActive = false;
                    c.UpdatedAt = now;
                }

                var estados = context.EstadosTurno.Where(e => e.TenantId == id).ToList();
                foreach (var e in estados)
                {
                    e.IsActive = false;
                    e.UpdatedAt = now;
                }

                tenant.IsActive = false;
                tenant.UpdatedAt = now;

                context.SaveChanges();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Reactiva un tenant dado de baja (IsActive = false -> true).
        /// En transaccion (simetria con Delete): tenant + users, todo o nada.
        /// </summary>
        public void Reactivar(int id)
        {
            using (var context = new JKContext())
            using (var transaction = context.Database.BeginTransaction())
            {
                var tenant = context.Tenants.FirstOrDefault(t => t.Id == id);

                if (tenant == null || tenant.IsActive)
                    return;

                var now = DateTime.Now;
                tenant.IsActive = true;
                tenant.UpdatedAt = now;

                var users = context.Users.Where(u => u.TenantId == id).ToList();
                foreach (var u in users)
                {
                    u.IsActive = true;
                    u.UpdatedAt = now;
                }

                context.SaveChanges();
                transaction.Commit();
            }
        }

        /// <summary>
        /// D-038 / AG-2: suspension SOLO manual e informativa. Se elimino el toggle
        /// del UI (D-039); el campo SuspendidoManual persiste en el modelo solo para
        /// el estado derivado de la card/dashboard, sin metodos de escritura activos.
        /// </summary>

        /// <summary>
        /// Semilla de estados genericos para cualquier negocio (D-024/D-040).
        /// Siembra exactamente los 4 estados del catalogo fijo, con su Codigo
        /// estable, iconos geometricos (no colores) para distinguir del color
        /// del recurso en la Agenda.
        /// </summary>
        private void SeedBaseData(Tenant tenant, string adminEmail)
        {
            foreach (var e in EstadosTurnoBase.Todos)
            {
                _estadoRepo.Add(new EstadoTurno
                {
                    TenantId = tenant.Id,
                    Nombre = e.Nombre,
                    Codigo = e.Codigo,
                    Color = e.Color,
                    Icono = e.Icono,
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
