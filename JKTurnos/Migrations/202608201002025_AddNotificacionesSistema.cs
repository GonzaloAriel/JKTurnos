namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificacionesSistema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificacionSistemas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Tipo = c.String(),
                        Titulo = c.String(),
                        Mensaje = c.String(),
                        EntidadTipo = c.String(),
                        EntidadId = c.Int(),
                        Fecha = c.DateTime(nullable: false),
                        Leida = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NotificacionSistemas");
        }
    }
}
