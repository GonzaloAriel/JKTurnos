namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRecursoServicio : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RecursoServicios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        RecursoId = c.Int(nullable: false),
                        ServicioId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Recursoes", t => t.RecursoId, cascadeDelete: true)
                .ForeignKey("dbo.Servicios", t => t.ServicioId, cascadeDelete: true)
                .Index(t => t.RecursoId)
                .Index(t => t.ServicioId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RecursoServicios", "ServicioId", "dbo.Servicios");
            DropForeignKey("dbo.RecursoServicios", "RecursoId", "dbo.Recursoes");
            DropIndex("dbo.RecursoServicios", new[] { "ServicioId" });
            DropIndex("dbo.RecursoServicios", new[] { "RecursoId" });
            DropTable("dbo.RecursoServicios");
        }
    }
}
