namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPortalPublico : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfiguracionTenants", "PortalHabilitado", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfiguracionTenants", "PortalHabilitado");
        }
    }
}
