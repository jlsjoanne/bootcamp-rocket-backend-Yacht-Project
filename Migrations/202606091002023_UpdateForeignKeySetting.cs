namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateForeignKeySetting : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DealerImages", "DealerId", "dbo.Dealers");
            DropForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.NewsFiles", "NewsId", "dbo.News");
            DropForeignKey("dbo.NewsImages", "NewsId", "dbo.News");
            DropIndex("dbo.YachtImages", new[] { "YachtId" });
            DropIndex("dbo.YachtDownloads", new[] { "YachtId" });
            DropIndex("dbo.NewsFiles", new[] { "NewsId" });
            DropIndex("dbo.NewsImages", new[] { "NewsId" });
            AddColumn("dbo.DealerImages", "StoredFileName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.YachtImages", "YachtId", c => c.Int(nullable: false));
            AlterColumn("dbo.YachtDownloads", "YachtId", c => c.Int(nullable: false));
            AlterColumn("dbo.NewsFiles", "NewsId", c => c.Int(nullable: false));
            AlterColumn("dbo.NewsImages", "NewsId", c => c.Int(nullable: false));
            CreateIndex("dbo.YachtImages", "YachtId");
            CreateIndex("dbo.YachtDownloads", "YachtId");
            CreateIndex("dbo.NewsFiles", "NewsId");
            CreateIndex("dbo.NewsImages", "NewsId");
            AddForeignKey("dbo.DealerImages", "DealerId", "dbo.Dealers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NewsFiles", "NewsId", "dbo.News", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NewsImages", "NewsId", "dbo.News", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NewsImages", "NewsId", "dbo.News");
            DropForeignKey("dbo.NewsFiles", "NewsId", "dbo.News");
            DropForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.DealerImages", "DealerId", "dbo.Dealers");
            DropIndex("dbo.NewsImages", new[] { "NewsId" });
            DropIndex("dbo.NewsFiles", new[] { "NewsId" });
            DropIndex("dbo.YachtDownloads", new[] { "YachtId" });
            DropIndex("dbo.YachtImages", new[] { "YachtId" });
            AlterColumn("dbo.NewsImages", "NewsId", c => c.Int());
            AlterColumn("dbo.NewsFiles", "NewsId", c => c.Int());
            AlterColumn("dbo.YachtDownloads", "YachtId", c => c.Int());
            AlterColumn("dbo.YachtImages", "YachtId", c => c.Int());
            DropColumn("dbo.DealerImages", "StoredFileName");
            CreateIndex("dbo.NewsImages", "NewsId");
            CreateIndex("dbo.NewsFiles", "NewsId");
            CreateIndex("dbo.YachtDownloads", "YachtId");
            CreateIndex("dbo.YachtImages", "YachtId");
            AddForeignKey("dbo.NewsImages", "NewsId", "dbo.News", "Id");
            AddForeignKey("dbo.NewsFiles", "NewsId", "dbo.News", "Id");
            AddForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts", "Id");
            AddForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts", "Id");
            AddForeignKey("dbo.DealerImages", "DealerId", "dbo.Dealers", "Id");
        }
    }
}
