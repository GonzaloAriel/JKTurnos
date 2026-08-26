namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPagosSuscripciones : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PagoSuscripcions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        PlanCodigo = c.String(),
                        Monto = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Fecha = c.DateTime(nullable: false),
                        Observacion = c.String(maxLength: 500),
                        UsuarioId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PagoSuscripcions");
        }
    }
}
