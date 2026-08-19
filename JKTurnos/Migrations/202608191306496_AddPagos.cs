namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPagos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Pagoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        TurnoId = c.Int(nullable: false),
                        Monto = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Fecha = c.DateTime(nullable: false),
                        TipoPago = c.Int(nullable: false),
                        Observacion = c.String(),
                        UsuarioId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Turnoes", t => t.TurnoId)
                .Index(t => t.TurnoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Pagoes", "TurnoId", "dbo.Turnoes");
            DropIndex("dbo.Pagoes", new[] { "TurnoId" });
            DropTable("dbo.Pagoes");
        }
    }
}
