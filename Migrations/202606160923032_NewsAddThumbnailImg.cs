namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewsAddThumbnailImg : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NewsImages", "NewsId", "dbo.News");
            AddColumn("dbo.News", "ThumbnailImageId", c => c.Guid());
            AddColumn("dbo.NewsImages", "News_Id", c => c.Int());
            CreateIndex("dbo.News", "ThumbnailImageId");
            CreateIndex("dbo.NewsImages", "News_Id");
            AddForeignKey("dbo.News", "ThumbnailImageId", "dbo.NewsImages", "Id");
            AddForeignKey("dbo.NewsImages", "News_Id", "dbo.News", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NewsImages", "News_Id", "dbo.News");
            DropForeignKey("dbo.News", "ThumbnailImageId", "dbo.NewsImages");
            DropIndex("dbo.NewsImages", new[] { "News_Id" });
            DropIndex("dbo.News", new[] { "ThumbnailImageId" });
            DropColumn("dbo.NewsImages", "News_Id");
            DropColumn("dbo.News", "ThumbnailImageId");
            AddForeignKey("dbo.NewsImages", "NewsId", "dbo.News", "Id", cascadeDelete: true);
        }
    }
}
