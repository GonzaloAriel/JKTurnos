namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuditoria : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Auditorias",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Fecha = c.DateTime(nullable: false),
                        Accion = c.String(),
                        Entidad = c.String(),
                        EntidadId = c.Int(),
                        UsuarioId = c.Int(),
                        UsuarioNombre = c.String(),
                        Rol = c.String(),
                        Detalle = c.String(),
                        Cambios = c.String(),
                        Ip = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Auditorias");
        }
    }
}
