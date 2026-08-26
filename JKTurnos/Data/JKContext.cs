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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}
