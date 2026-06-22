namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateYachtInterior : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts");
            DropIndex("dbo.YachtInteriors", new[] { "YachtId" });
            AlterColumn("dbo.YachtInteriors", "YachtId", c => c.Int(nullable: false));
            CreateIndex("dbo.YachtInteriors", "YachtId");
            AddForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts");
            DropIndex("dbo.YachtInteriors", new[] { "YachtId" });
            AlterColumn("dbo.YachtInteriors", "YachtId", c => c.Int());
            CreateIndex("dbo.YachtInteriors", "YachtId");
            AddForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts", "Id");
        }
    }
}
