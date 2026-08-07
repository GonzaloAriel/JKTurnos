namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixCascadeDeleteTurnoRecurso : DbMigration
    {
        public override void Up()
        {
            // Drop the existing FK (EF naming convention)
            DropForeignKey("dbo.Turnoes", "RecursoId", "dbo.Recursoes");
            
            // Re-add FK without Cascade Delete
            AddForeignKey("dbo.Turnoes", "RecursoId", "dbo.Recursoes", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            // Revert back to Cascade Delete if necessary
            DropForeignKey("dbo.Turnoes", "RecursoId", "dbo.Recursoes");
            AddForeignKey("dbo.Turnoes", "RecursoId", "dbo.Recursoes", "Id", cascadeDelete: true);
        }
    }
}
