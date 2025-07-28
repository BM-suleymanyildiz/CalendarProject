namespace CalendarProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mig1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Events", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Events", new[] { "CategoryId" });
            AlterColumn("dbo.Events", "CategoryId", c => c.Int());
            CreateIndex("dbo.Events", "CategoryId");
            AddForeignKey("dbo.Events", "CategoryId", "dbo.Categories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Events", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Events", new[] { "CategoryId" });
            AlterColumn("dbo.Events", "CategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.Events", "CategoryId");
            AddForeignKey("dbo.Events", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
        }
    }
}
