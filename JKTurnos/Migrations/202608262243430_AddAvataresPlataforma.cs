namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAvataresPlataforma : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AvatarPlataformas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Url = c.String(nullable: false, maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ConfiguracionTenants", "AvatarUrl", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfiguracionTenants", "AvatarUrl");
            DropTable("dbo.AvatarPlataformas");
        }
    }
}
