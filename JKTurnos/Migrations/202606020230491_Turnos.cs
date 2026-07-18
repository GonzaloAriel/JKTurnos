namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Turnos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EstadoTurnoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(),
                        Color = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Turnoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        ClienteId = c.Int(nullable: false),
                        ServicioId = c.Int(nullable: false),
                        EstadoTurnoId = c.Int(nullable: false),
                        FechaHoraInicio = c.DateTime(nullable: false),
                        FechaHoraFin = c.DateTime(nullable: false),
                        Observaciones = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clientes", t => t.ClienteId, cascadeDelete: true)
                .ForeignKey("dbo.EstadoTurnoes", t => t.EstadoTurnoId, cascadeDelete: true)
                .ForeignKey("dbo.Servicios", t => t.ServicioId, cascadeDelete: true)
                .Index(t => t.ClienteId)
                .Index(t => t.ServicioId)
                .Index(t => t.EstadoTurnoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Turnoes", "ServicioId", "dbo.Servicios");
            DropForeignKey("dbo.Turnoes", "EstadoTurnoId", "dbo.EstadoTurnoes");
            DropForeignKey("dbo.Turnoes", "ClienteId", "dbo.Clientes");
            DropIndex("dbo.Turnoes", new[] { "EstadoTurnoId" });
            DropIndex("dbo.Turnoes", new[] { "ServicioId" });
            DropIndex("dbo.Turnoes", new[] { "ClienteId" });
            DropTable("dbo.Turnoes");
            DropTable("dbo.EstadoTurnoes");
        }
    }
}
