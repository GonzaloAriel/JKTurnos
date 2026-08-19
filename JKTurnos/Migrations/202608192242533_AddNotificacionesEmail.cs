namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificacionesEmail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfiguracionTenants", "EmailAsunto", c => c.String(maxLength: 200));
            AddColumn("dbo.ConfiguracionTenants", "EmailTemplate", c => c.String(maxLength: 1000));
            AddColumn("dbo.ConfiguracionTenants", "EmailDesde", c => c.String(maxLength: 150));
            AddColumn("dbo.ConfiguracionTenants", "SmtpServidor", c => c.String(maxLength: 200));
            AddColumn("dbo.ConfiguracionTenants", "SmtpPuerto", c => c.Int());
            AddColumn("dbo.ConfiguracionTenants", "SmtpUsuario", c => c.String(maxLength: 150));
            AddColumn("dbo.ConfiguracionTenants", "SmtpPassword", c => c.String(maxLength: 150));
            AddColumn("dbo.ConfiguracionTenants", "SmtpHabilitado", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfiguracionTenants", "SmtpHabilitado");
            DropColumn("dbo.ConfiguracionTenants", "SmtpPassword");
            DropColumn("dbo.ConfiguracionTenants", "SmtpUsuario");
            DropColumn("dbo.ConfiguracionTenants", "SmtpPuerto");
            DropColumn("dbo.ConfiguracionTenants", "SmtpServidor");
            DropColumn("dbo.ConfiguracionTenants", "EmailDesde");
            DropColumn("dbo.ConfiguracionTenants", "EmailTemplate");
            DropColumn("dbo.ConfiguracionTenants", "EmailAsunto");
        }
    }
}
