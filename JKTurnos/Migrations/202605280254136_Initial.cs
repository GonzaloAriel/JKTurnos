namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EmpleadoServicios", "EmpleadoId", "dbo.Empleadoes");
            DropForeignKey("dbo.EmpleadoServicios", "ServicioId", "dbo.Servicios");
            DropForeignKey("dbo.HorarioExcepcionEmpleadoes", "EmpleadoId", "dbo.Empleadoes");
            DropForeignKey("dbo.HorarioEmpleadoes", "EmpleadoId", "dbo.Empleadoes");
            DropForeignKey("dbo.ConfiguracionTenants", "TenantId", "dbo.Tenants");
            DropForeignKey("dbo.ThemeSettings", "TenantId", "dbo.Tenants");
            DropForeignKey("dbo.Turnoes", "ClienteId", "dbo.Clientes");
            DropForeignKey("dbo.Turnoes", "EmpleadoId", "dbo.Empleadoes");
            DropForeignKey("dbo.Turnoes", "ServicioId", "dbo.Servicios");
            DropForeignKey("dbo.TurnoHistorials", "TurnoId", "dbo.Turnoes");
            DropIndex("dbo.Clientes", new[] { "TenantId" });
            DropIndex("dbo.ConfiguracionTenants", new[] { "TenantId" });
            DropIndex("dbo.Empleadoes", new[] { "TenantId" });
            DropIndex("dbo.EmpleadoServicios", new[] { "TenantId" });
            DropIndex("dbo.EmpleadoServicios", new[] { "EmpleadoId", "ServicioId" });
            DropIndex("dbo.Servicios", new[] { "TenantId" });
            DropIndex("dbo.HorarioExcepcionEmpleadoes", new[] { "EmpleadoId" });
            DropIndex("dbo.HorarioEmpleadoes", new[] { "EmpleadoId" });
            DropIndex("dbo.Users", new[] { "Email" });
            DropIndex("dbo.Users", new[] { "TenantId" });
            DropIndex("dbo.ThemeSettings", new[] { "TenantId" });
            DropIndex("dbo.TurnoHistorials", new[] { "TurnoId" });
            DropIndex("dbo.Turnoes", new[] { "TenantId" });
            DropIndex("dbo.Turnoes", new[] { "TenantId", "FechaInicio" });
            DropIndex("dbo.Turnoes", new[] { "ClienteId" });
            DropIndex("dbo.Turnoes", new[] { "EmpleadoId" });
            DropIndex("dbo.Turnoes", new[] { "ServicioId" });
            DropIndex("dbo.Turnoes", new[] { "FechaInicio" });
            AlterColumn("dbo.Tenants", "Nombre", c => c.String());
            AlterColumn("dbo.Tenants", "Subdominio", c => c.String());
            AlterColumn("dbo.Users", "Email", c => c.String());
            AlterColumn("dbo.Users", "PasswordHash", c => c.String());
            AlterColumn("dbo.Users", "Nombre", c => c.String());
            AlterColumn("dbo.Users", "Role", c => c.String());
            CreateIndex("dbo.Users", "TenantId");
            DropColumn("dbo.Tenants", "CreatedByUserId");
            DropColumn("dbo.Tenants", "UpdatedByUserId");
            DropColumn("dbo.Users", "CreatedByUserId");
            DropColumn("dbo.Users", "UpdatedByUserId");
            DropTable("dbo.Clientes");
            DropTable("dbo.ConfiguracionTenants");
            DropTable("dbo.Empleadoes");
            DropTable("dbo.EmpleadoServicios");
            DropTable("dbo.Servicios");
            DropTable("dbo.HorarioExcepcionEmpleadoes");
            DropTable("dbo.HorarioEmpleadoes");
            DropTable("dbo.ThemeSettings");
            DropTable("dbo.TurnoHistorials");
            DropTable("dbo.Turnoes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Turnoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        ClienteId = c.Int(nullable: false),
                        EmpleadoId = c.Int(nullable: false),
                        ServicioId = c.Int(nullable: false),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(nullable: false),
                        Estado = c.Int(nullable: false),
                        Notas = c.String(maxLength: 255),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TurnoHistorials",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        TurnoId = c.Int(nullable: false),
                        FechaEvento = c.DateTime(nullable: false),
                        EstadoAnterior = c.Int(nullable: false),
                        EstadoNuevo = c.Int(nullable: false),
                        UserId = c.Int(),
                        Notas = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ThemeSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        ThemeName = c.String(nullable: false, maxLength: 50),
                        PrimaryColor = c.String(maxLength: 20),
                        SecondaryColor = c.String(maxLength: 20),
                        SidebarColor = c.String(maxLength: 20),
                        BackgroundColor = c.String(maxLength: 20),
                        CardColor = c.String(maxLength: 20),
                        TableHeaderColor = c.String(maxLength: 20),
                        FontFamily = c.String(maxLength: 100),
                        LogoUrl = c.String(maxLength: 255),
                        LoginBackgroundUrl = c.String(maxLength: 255),
                        IsDarkMode = c.Boolean(nullable: false),
                        CustomCss = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HorarioEmpleadoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        EmpleadoId = c.Int(nullable: false),
                        DiaSemana = c.Int(nullable: false),
                        HoraInicio = c.Time(nullable: false, precision: 7),
                        HoraFin = c.Time(nullable: false, precision: 7),
                        IntervaloMinutos = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HorarioExcepcionEmpleadoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        EmpleadoId = c.Int(nullable: false),
                        FechaEspecifica = c.DateTime(nullable: false, storeType: "date"),
                        HoraInicio = c.Time(precision: 7),
                        HoraFin = c.Time(precision: 7),
                        TipoExcepcion = c.String(nullable: false, maxLength: 20),
                        Motivo = c.String(maxLength: 100),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Servicios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Descripcion = c.String(maxLength: 255),
                        DuracionMinutos = c.Int(nullable: false),
                        Precio = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Color = c.String(maxLength: 20),
                        TenantId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EmpleadoServicios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        EmpleadoId = c.Int(nullable: false),
                        ServicioId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Empleadoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Apellido = c.String(maxLength: 100),
                        Telefono = c.String(maxLength: 20),
                        Email = c.String(maxLength: 100),
                        ColorAgenda = c.String(maxLength: 20),
                        TenantId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ConfiguracionTenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Clave = c.String(nullable: false, maxLength: 50),
                        Valor = c.String(nullable: false),
                        TenantId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Clientes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Email = c.String(maxLength: 100),
                        Telefono = c.String(maxLength: 20),
                        TenantId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.Int(),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Users", "UpdatedByUserId", c => c.Int());
            AddColumn("dbo.Users", "CreatedByUserId", c => c.Int());
            AddColumn("dbo.Tenants", "UpdatedByUserId", c => c.Int());
            AddColumn("dbo.Tenants", "CreatedByUserId", c => c.Int());
            DropIndex("dbo.Users", new[] { "TenantId" });
            AlterColumn("dbo.Users", "Role", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Users", "Nombre", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Email", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Tenants", "Subdominio", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Tenants", "Nombre", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.Turnoes", "FechaInicio");
            CreateIndex("dbo.Turnoes", "ServicioId");
            CreateIndex("dbo.Turnoes", "EmpleadoId");
            CreateIndex("dbo.Turnoes", "ClienteId");
            CreateIndex("dbo.Turnoes", new[] { "TenantId", "FechaInicio" });
            CreateIndex("dbo.Turnoes", "TenantId");
            CreateIndex("dbo.TurnoHistorials", "TurnoId");
            CreateIndex("dbo.ThemeSettings", "TenantId");
            CreateIndex("dbo.Users", "TenantId");
            CreateIndex("dbo.Users", "Email", unique: true);
            CreateIndex("dbo.HorarioEmpleadoes", "EmpleadoId");
            CreateIndex("dbo.HorarioExcepcionEmpleadoes", "EmpleadoId");
            CreateIndex("dbo.Servicios", "TenantId");
            CreateIndex("dbo.EmpleadoServicios", new[] { "EmpleadoId", "ServicioId" });
            CreateIndex("dbo.EmpleadoServicios", "TenantId");
            CreateIndex("dbo.Empleadoes", "TenantId");
            CreateIndex("dbo.ConfiguracionTenants", "TenantId");
            CreateIndex("dbo.Clientes", "TenantId");
            AddForeignKey("dbo.TurnoHistorials", "TurnoId", "dbo.Turnoes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Turnoes", "ServicioId", "dbo.Servicios", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Turnoes", "EmpleadoId", "dbo.Empleadoes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Turnoes", "ClienteId", "dbo.Clientes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ThemeSettings", "TenantId", "dbo.Tenants", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ConfiguracionTenants", "TenantId", "dbo.Tenants", "Id", cascadeDelete: true);
            AddForeignKey("dbo.HorarioEmpleadoes", "EmpleadoId", "dbo.Empleadoes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.HorarioExcepcionEmpleadoes", "EmpleadoId", "dbo.Empleadoes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EmpleadoServicios", "ServicioId", "dbo.Servicios", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EmpleadoServicios", "EmpleadoId", "dbo.Empleadoes", "Id", cascadeDelete: true);
        }
    }
}
