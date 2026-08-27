namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdempotencyKeyToPagoSuscripcion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PagoSuscripcions", "IdempotencyKey", c => c.String(maxLength: 36));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PagoSuscripcions", "IdempotencyKey");
        }
    }
}
