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
    }
}