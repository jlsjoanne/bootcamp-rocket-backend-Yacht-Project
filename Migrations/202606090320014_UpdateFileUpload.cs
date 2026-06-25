namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateFileUpload : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Dealers", "Area_Id", "dbo.Areas");
            DropIndex("dbo.Dealers", new[] { "Area_Id" });
            AddColumn("dbo.DealerImages", "OriginalFileName", c => c.String());
            AddColumn("dbo.DealerImages", "FilePath", c => c.String());
            AddColumn("dbo.YachtImages", "OriginalFileName", c => c.String());
            AddColumn("dbo.YachtImages", "FilePath", c => c.String());
            AddColumn("dbo.YachtDownloads", "OriginalFileName", c => c.String());
            AddColumn("dbo.YachtDownloads", "FilePath", c => c.String());
            AddColumn("dbo.YachtInteriors", "OriginalFileName", c => c.String());
            AddColumn("dbo.YachtInteriors", "FilePath", c => c.String());
            AddColumn("dbo.NewsFiles", "OriginalFileName", c => c.String());
            AddColumn("dbo.NewsFiles", "FilePath", c => c.String());
            AddColumn("dbo.NewsImages", "OriginalFileName", c => c.String());
            AddColumn("dbo.NewsImages", "FilePath", c => c.String());
            AlterColumn("dbo.Dealers", "Area_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.Dealers", "Area_Id");
            AddForeignKey("dbo.Dealers", "Area_Id", "dbo.Areas", "Id", cascadeDelete: true);
            DropColumn("dbo.DealerImages", "FileName");
            DropColumn("dbo.YachtImages", "FileName");
            DropColumn("dbo.YachtDownloads", "FileName");
            DropColumn("dbo.YachtInteriors", "FileName");
            DropColumn("dbo.NewsFiles", "FileName");
            DropColumn("dbo.NewsImages", "FileName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NewsImages", "FileName", c => c.String());
            AddColumn("dbo.NewsFiles", "FileName", c => c.String());
            AddColumn("dbo.YachtInteriors", "FileName", c => c.String());
            AddColumn("dbo.YachtDownloads", "FileName", c => c.String());
            AddColumn("dbo.YachtImages", "FileName", c => c.String());
            AddColumn("dbo.DealerImages", "FileName", c => c.String());
            DropForeignKey("dbo.Dealers", "Area_Id", "dbo.Areas");
            DropIndex("dbo.Dealers", new[] { "Area_Id" });
            AlterColumn("dbo.Dealers", "Area_Id", c => c.Int());
            DropColumn("dbo.NewsImages", "FilePath");
            DropColumn("dbo.NewsImages", "OriginalFileName");
            DropColumn("dbo.NewsFiles", "FilePath");
            DropColumn("dbo.NewsFiles", "OriginalFileName");
            DropColumn("dbo.YachtInteriors", "FilePath");
            DropColumn("dbo.YachtInteriors", "OriginalFileName");
            DropColumn("dbo.YachtDownloads", "FilePath");
            DropColumn("dbo.YachtDownloads", "OriginalFileName");
            DropColumn("dbo.YachtImages", "FilePath");
            DropColumn("dbo.YachtImages", "OriginalFileName");
            DropColumn("dbo.DealerImages", "FilePath");
            DropColumn("dbo.DealerImages", "OriginalFileName");
            CreateIndex("dbo.Dealers", "Area_Id");
            AddForeignKey("dbo.Dealers", "Area_Id", "dbo.Areas", "Id");
        }
    }
}
