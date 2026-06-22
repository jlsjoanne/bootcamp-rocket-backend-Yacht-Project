namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeYachtEditorImageYachtIdNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.YachtEditorImages", "YachtId", "dbo.Yachts");
            DropIndex("dbo.YachtEditorImages", new[] { "YachtId" });

            AlterColumn("dbo.YachtEditorImages", "YachtId", c => c.Int());

            CreateIndex("dbo.YachtEditorImages", "YachtId");
            AddForeignKey("dbo.YachtEditorImages", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.YachtEditorImages", "YachtId", "dbo.Yachts");
            DropIndex("dbo.YachtEditorImages", new[] { "YachtId" });

            AlterColumn("dbo.YachtEditorImages", "YachtId", c => c.Int(nullable: false));

            CreateIndex("dbo.YachtEditorImages", "YachtId");
            AddForeignKey("dbo.YachtEditorImages", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
        }
    }
}
