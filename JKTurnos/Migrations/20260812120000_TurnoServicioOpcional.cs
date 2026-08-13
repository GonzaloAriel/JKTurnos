using System;
using System.Data.Entity.Migrations;

namespace JKTurnos.Migrations
{
    public partial class TurnoServicioOpcional : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Turnoes", "ServicioId", c => c.Int());
        }
        
        public override void Down()
        {
            // Opcional: manejar reversión si es necesario, 
            // aunque requeriría asegurar que no haya nulos.
            AlterColumn("dbo.Turnoes", "ServicioId", c => c.Int(nullable: false));
        }
    }
}
