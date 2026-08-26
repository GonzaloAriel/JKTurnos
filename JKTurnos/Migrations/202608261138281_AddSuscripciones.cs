namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSuscripciones : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tenants", "PlanCodigo", c => c.String());
            AddColumn("dbo.Tenants", "FechaInicioPeriodo", c => c.DateTime());
            AddColumn("dbo.Tenants", "FechaVencimiento", c => c.DateTime());
            AddColumn("dbo.Tenants", "SuspendidoManual", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tenants", "SuspendidoManual");
            DropColumn("dbo.Tenants", "FechaVencimiento");
            DropColumn("dbo.Tenants", "FechaInicioPeriodo");
            DropColumn("dbo.Tenants", "PlanCodigo");
        }
    }
}
