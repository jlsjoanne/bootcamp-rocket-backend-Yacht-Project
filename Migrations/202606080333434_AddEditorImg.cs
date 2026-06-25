namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEditorImg : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NewsFiles", "NewsId", "dbo.News");
            DropForeignKey("dbo.NewsImages", "NewsId", "dbo.News");
            DropForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts");
            DropIndex("dbo.NewsFiles", new[] { "NewsId" });
            DropIndex("dbo.NewsImages", new[] { "NewsId" });
            DropIndex("dbo.YachtDownloads", new[] { "YachtId" });
            DropIndex("dbo.YachtImages", new[] { "YachtId" });
            DropIndex("dbo.YachtInteriors", new[] { "YachtId" });
            CreateTable(
                "dbo.DealerImages",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DealerId = c.Int(),
                        OriginalFileName = c.String(maxLength: 255),
                        StoredFileName = c.String(maxLength: 255),
                        FileExtension = c.String(maxLength: 20),
                        ContentType = c.String(maxLength: 100),
                        FileSizeBytes = c.Long(nullable: false),
                        ImageUrl = c.String(maxLength: 500),
                        UploadedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dealers", t => t.DealerId)
                .Index(t => t.DealerId);
            
            CreateTable(
                "dbo.YachtEditorImages",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        YachtId = c.Int(),
                        OriginalFileName = c.String(maxLength: 255),
                        StoredFileName = c.String(maxLength: 255),
                        FileExtension = c.String(maxLength: 20),
                        ContentType = c.String(maxLength: 100),
                        FileSizeBytes = c.Long(nullable: false),
                        ImageUrl = c.String(maxLength: 500),
                        UploadedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Yachts", t => t.YachtId)
                .Index(t => t.YachtId);
            
            AlterColumn("dbo.NewsFiles", "NewsId", c => c.Int());
            AlterColumn("dbo.NewsImages", "NewsId", c => c.Int());
            AlterColumn("dbo.YachtDownloads", "YachtId", c => c.Int());
            AlterColumn("dbo.YachtImages", "YachtId", c => c.Int());
            AlterColumn("dbo.YachtInteriors", "YachtId", c => c.Int());
            AlterColumn("dbo.Yachts", "Specification", c => c.String());
            CreateIndex("dbo.YachtImages", "YachtId");
            CreateIndex("dbo.YachtDownloads", "YachtId");
            CreateIndex("dbo.YachtInteriors", "YachtId");
            CreateIndex("dbo.NewsFiles", "NewsId");
            CreateIndex("dbo.NewsImages", "NewsId");
            AddForeignKey("dbo.NewsFiles", "NewsId", "dbo.News", "Id");
            AddForeignKey("dbo.NewsImages", "NewsId", "dbo.News", "Id");
            AddForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts", "Id");
            AddForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts", "Id");
            AddForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts", "Id");
            DropColumn("dbo.Dealers", "Image_Id");
            DropColumn("dbo.Yachts", "OverviewImage_Id");
            DropTable("dbo.UploadedFiles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UploadedFiles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        ContentType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Yachts", "OverviewImage_Id", c => c.Guid());
            AddColumn("dbo.Dealers", "Image_Id", c => c.Guid());
            DropForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.NewsImages", "NewsId", "dbo.News");
            DropForeignKey("dbo.NewsFiles", "NewsId", "dbo.News");
            DropForeignKey("dbo.YachtEditorImages", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.DealerImages", "DealerId", "dbo.Dealers");
            DropIndex("dbo.NewsImages", new[] { "NewsId" });
            DropIndex("dbo.NewsFiles", new[] { "NewsId" });
            DropIndex("dbo.YachtInteriors", new[] { "YachtId" });
            DropIndex("dbo.YachtEditorImages", new[] { "YachtId" });
            DropIndex("dbo.YachtDownloads", new[] { "YachtId" });
            DropIndex("dbo.YachtImages", new[] { "YachtId" });
            DropIndex("dbo.DealerImages", new[] { "DealerId" });
            AlterColumn("dbo.Yachts", "Specification", c => c.String(nullable: false));
            AlterColumn("dbo.YachtInteriors", "YachtId", c => c.Int(nullable: false));
            AlterColumn("dbo.YachtImages", "YachtId", c => c.Int(nullable: false));
            AlterColumn("dbo.YachtDownloads", "YachtId", c => c.Int(nullable: false));
            AlterColumn("dbo.NewsImages", "NewsId", c => c.Int(nullable: false));
            AlterColumn("dbo.NewsFiles", "NewsId", c => c.Int(nullable: false));
            DropTable("dbo.YachtEditorImages");
            DropTable("dbo.DealerImages");
            CreateIndex("dbo.YachtInteriors", "YachtId");
            CreateIndex("dbo.YachtImages", "YachtId");
            CreateIndex("dbo.YachtDownloads", "YachtId");
            CreateIndex("dbo.NewsImages", "NewsId");
            CreateIndex("dbo.NewsFiles", "NewsId");
            AddForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NewsImages", "NewsId", "dbo.News", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NewsFiles", "NewsId", "dbo.News", "Id", cascadeDelete: true);
        }
    }
}
