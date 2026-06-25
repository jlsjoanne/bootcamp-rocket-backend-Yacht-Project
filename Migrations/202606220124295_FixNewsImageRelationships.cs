namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FixNewsImageRelationships : DbMigration
    {
        public override void Up()
        {
            Sql(@"
IF object_id(N'[dbo].[FK_dbo.NewsImages_dbo.News_News_Id]', N'F') IS NOT NULL
      ALTER TABLE [dbo].[NewsImages] DROP CONSTRAINT [FK_dbo.NewsImages_dbo.News_News_Id];

  IF EXISTS (
      SELECT name FROM sys.indexes
      WHERE name = N'IX_News_Id'
        AND object_id = object_id(N'[dbo].[NewsImages]', N'U')
  )
      DROP INDEX [IX_News_Id] ON [dbo].[NewsImages];

  IF COL_LENGTH('dbo.NewsImages', 'News_Id') IS NOT NULL
      ALTER TABLE [dbo].[NewsImages] DROP COLUMN [News_Id];
  ");

            AlterColumn("dbo.NewsImages", "NewsId", c => c.Int(nullable: false));

            CreateIndex("dbo.NewsImages", "NewsId");

            AddForeignKey(
                "dbo.NewsImages",
                "NewsId",
                "dbo.News",
                "Id",
                cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.NewsImages", "NewsId", "dbo.News");

            DropIndex("dbo.NewsImages", new[] { "NewsId" });

            AlterColumn("dbo.NewsImages", "NewsId", c => c.Int());

            AddColumn("dbo.NewsImages", "News_Id", c => c.Int());

            CreateIndex("dbo.NewsImages", "News_Id");

            AddForeignKey("dbo.NewsImages", "News_Id", "dbo.News", "Id");
        }
    }
}
