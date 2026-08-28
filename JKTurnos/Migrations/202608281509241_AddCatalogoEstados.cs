namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCatalogoEstados : DbMigration
    {
        public override void Up()
        {
            // D-040: codigo estable de estado (catalogo fijo)
            AddColumn("dbo.EstadoTurnoes", "Codigo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EstadoTurnoes", "Codigo");
        }
    }
}
