namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddConfiguracionTenant : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConfiguracionTenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        NombreComercial = c.String(nullable: false, maxLength: 150),
                        Telefono = c.String(maxLength: 50),
                        Email = c.String(maxLength: 150),
                        Direccion = c.String(maxLength: 250),
                        LogoUrl = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tenants", t => t.TenantId, cascadeDelete: true)
                .Index(t => t.TenantId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ConfiguracionTenants", "TenantId", "dbo.Tenants");
            DropIndex("dbo.ConfiguracionTenants", new[] { "TenantId" });
            DropTable("dbo.ConfiguracionTenants");
        }
    }
}
