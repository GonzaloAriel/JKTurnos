namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificacionesWhatsApp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfiguracionTenants", "WhatsAppTemplate", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfiguracionTenants", "WhatsAppTemplate");
        }
    }
}
