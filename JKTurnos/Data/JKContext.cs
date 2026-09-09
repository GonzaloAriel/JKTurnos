using System.Data.Entity;
using JKTurnos.Models.Entities;

namespace JKTurnos.Data
{
    public class JKContext : DbContext
    {
        public JKContext()
            : base("JKTurnosConnection")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ConfiguracionTenant> ConfiguracionesTenant { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<EstadoTurno> EstadosTurno { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<RecursoServicio> RecursoServicios { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<PagoSuscripcion> PagosSuscripciones { get; set; }
        public DbSet<NotificacionSistema> NotificacionesSistema { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<AvatarPlataforma> AvataresPlataforma { get; set; }
        public DbSet<VidrieraPublica> VidrieraPublicas { get; set; }
        public DbSet<BloqueoRecurso> BloqueosRecurso { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // M-09: EF6 no soporta Global Query Filters (a diferencia de EF Core).
            // El aislamiento multi-tenant depende de que cada consulta pase por
            // BaseRepository (filtra TenantId+IsActive) o filtre TenantId explicitamente.
            // NUNCA consultar JKContext.Set<T>() directamente sin filtrar por TenantId.

            modelBuilder.Entity<Turno>()
                .HasRequired(t => t.Recurso)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Pago>()
                .HasRequired(p => p.Turno)
                .WithMany()
                .WillCascadeOnDelete(false);

            // D-037 / AG-0: observacion del pago de plataforma limitada a 500 caracteres.
            modelBuilder.Entity<PagoSuscripcion>()
                .Property(x => x.Observacion)
                .HasMaxLength(500);

            // C4: clave de idempotencia (sin indice unico: la idempotencia se hace por
            // verificacion transaccional en el servicio; un indice unico rechazaria
            // registros historicos con NULL duplicados en este SQL Server).
            modelBuilder.Entity<PagoSuscripcion>()
                .Property(x => x.IdempotencyKey)
                .HasMaxLength(36);

            modelBuilder.Entity<AvatarPlataforma>()
                .Property(x => x.Nombre)
                .HasMaxLength(100);

            modelBuilder.Entity<AvatarPlataforma>()
                .Property(x => x.Url)
                .HasMaxLength(500);

            // D-042: VidrieraPublica 1:1 con Tenant. TenantId unico (1:1) y Slug
            // unico (la llave publica de URL /vidriera/{slug}).
            modelBuilder.Entity<VidrieraPublica>()
                .Property(x => x.Slug)
                .HasMaxLength(100);

            modelBuilder.Entity<VidrieraPublica>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            modelBuilder.Entity<VidrieraPublica>()
                .HasIndex(x => x.TenantId)
                .IsUnique();

            // D-046 / M-01: BloqueoRecurso (recurso no disponible). Relacion con Recurso
            // sin cascada de borrado. Indice compuesto para el filtro por tenant/recurso
            // y el barrido por tipo en la disponibilidad.
            modelBuilder.Entity<BloqueoRecurso>()
                .HasRequired(b => b.Recurso)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BloqueoRecurso>()
                .Property(b => b.Descripcion)
                .HasMaxLength(500);

            modelBuilder.Entity<BloqueoRecurso>()
                .HasIndex(b => new { b.TenantId, b.RecursoId, b.Tipo });
        }
    }
}
