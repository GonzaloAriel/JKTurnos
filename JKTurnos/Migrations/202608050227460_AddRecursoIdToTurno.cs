namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRecursoIdToTurno : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Turnoes", "RecursoId", c => c.Int());

            Sql(@"
UPDATE	dbo.Turnoes
SET		RecursoId = (
            SELECT TOP 1 r.Id
            FROM	dbo.Recursoes r
            WHERE	r.TenantId = dbo.Turnoes.TenantId
                    AND r.IsActive = 1
            ORDER BY r.Id ASC
        )
WHERE	RecursoId IS NULL;
");

            Sql(@"
IF EXISTS (SELECT 1 FROM dbo.Turnoes t WHERE t.RecursoId IS NULL)
    THROW 50001, 'No se pudo asignar un RecursoId valido a todos los Turnoes existentes.', 1;
");

            AlterColumn("dbo.Turnoes", "RecursoId", c => c.Int(nullable: false));
            CreateIndex("dbo.Turnoes", "RecursoId");
            AddForeignKey("dbo.Turnoes", "RecursoId", "dbo.Recursoes", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Turnoes", "RecursoId", "dbo.Recursoes");
            DropIndex("dbo.Turnoes", new[] { "RecursoId" });
            DropColumn("dbo.Turnoes", "RecursoId");
        }
    }
}
