namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clientes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 120),
                        Telefono = c.String(maxLength: 50),
                        Email = c.String(maxLength: 120),
                        Direccion = c.String(maxLength: 250),
                        Notas = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ConfiguracionTenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        NombreComercial = c.String(nullable: false, maxLength: 150),
                        Telefono = c.String(maxLength: 50),
                        Email = c.String(maxLength: 150),
                        Direccion = c.String(maxLength: 250),
                        LogoUrl = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tenants", t => t.TenantId, cascadeDelete: true)
                .Index(t => t.TenantId);
            
            CreateTable(
                "dbo.Tenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Subdominio = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EstadoTurnoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(),
                        Color = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Recursoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Descripcion = c.String(),
                        Tipo = c.Int(nullable: false),
                        Color = c.String(maxLength: 7),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Servicios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(),
                        Descripcion = c.String(),
                        Precio = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DuracionMinutos = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Turnoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        ClienteId = c.Int(nullable: false),
                        ServicioId = c.Int(nullable: false),
                        EstadoTurnoId = c.Int(nullable: false),
                        FechaHoraInicio = c.DateTime(nullable: false),
                        FechaHoraFin = c.DateTime(nullable: false),
                        Observaciones = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clientes", t => t.ClienteId, cascadeDelete: true)
                .ForeignKey("dbo.EstadoTurnoes", t => t.EstadoTurnoId, cascadeDelete: true)
                .ForeignKey("dbo.Servicios", t => t.ServicioId, cascadeDelete: true)
                .Index(t => t.ClienteId)
                .Index(t => t.ServicioId)
                .Index(t => t.EstadoTurnoId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        PasswordHash = c.String(),
                        Nombre = c.String(),
                        Role = c.String(),
                        TenantId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tenants", t => t.TenantId, cascadeDelete: true)
                .Index(t => t.TenantId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "TenantId", "dbo.Tenants");
            DropForeignKey("dbo.Turnoes", "ServicioId", "dbo.Servicios");
            DropForeignKey("dbo.Turnoes", "EstadoTurnoId", "dbo.EstadoTurnoes");
            DropForeignKey("dbo.Turnoes", "ClienteId", "dbo.Clientes");
            DropForeignKey("dbo.ConfiguracionTenants", "TenantId", "dbo.Tenants");
            DropIndex("dbo.Users", new[] { "TenantId" });
            DropIndex("dbo.Turnoes", new[] { "EstadoTurnoId" });
            DropIndex("dbo.Turnoes", new[] { "ServicioId" });
            DropIndex("dbo.Turnoes", new[] { "ClienteId" });
            DropIndex("dbo.ConfiguracionTenants", new[] { "TenantId" });
            DropTable("dbo.Users");
            DropTable("dbo.Turnoes");
            DropTable("dbo.Servicios");
            DropTable("dbo.Recursoes");
            DropTable("dbo.EstadoTurnoes");
            DropTable("dbo.Tenants");
            DropTable("dbo.ConfiguracionTenants");
            DropTable("dbo.Clientes");
        }
    }
}
