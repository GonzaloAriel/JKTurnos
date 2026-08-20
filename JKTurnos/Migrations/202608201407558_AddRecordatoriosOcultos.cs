namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRecordatoriosOcultos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfiguracionTenants", "RecordatoriosOcultos", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfiguracionTenants", "RecordatoriosOcultos");
        }
    }
}
