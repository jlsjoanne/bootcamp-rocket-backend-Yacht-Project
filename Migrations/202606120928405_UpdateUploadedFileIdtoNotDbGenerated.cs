namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUploadedFileIdtoNotDbGenerated : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.YachtImages");
            DropPrimaryKey("dbo.YachtDownloads");
            DropPrimaryKey("dbo.YachtInteriors");
            DropPrimaryKey("dbo.NewsFiles");
            DropPrimaryKey("dbo.NewsImages");
            AlterColumn("dbo.YachtImages", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.YachtDownloads", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.YachtInteriors", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.NewsFiles", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.NewsImages", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.YachtImages", "Id");
            AddPrimaryKey("dbo.YachtDownloads", "Id");
            AddPrimaryKey("dbo.YachtInteriors", "Id");
            AddPrimaryKey("dbo.NewsFiles", "Id");
            AddPrimaryKey("dbo.NewsImages", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.NewsImages");
            DropPrimaryKey("dbo.NewsFiles");
            DropPrimaryKey("dbo.YachtInteriors");
            DropPrimaryKey("dbo.YachtDownloads");
            DropPrimaryKey("dbo.YachtImages");
            AlterColumn("dbo.NewsImages", "Id", c => c.Guid(nullable: false, identity: true));
            AlterColumn("dbo.NewsFiles", "Id", c => c.Guid(nullable: false, identity: true));
            AlterColumn("dbo.YachtInteriors", "Id", c => c.Guid(nullable: false, identity: true));
            AlterColumn("dbo.YachtDownloads", "Id", c => c.Guid(nullable: false, identity: true));
            AlterColumn("dbo.YachtImages", "Id", c => c.Guid(nullable: false, identity: true));
            AddPrimaryKey("dbo.NewsImages", "Id");
            AddPrimaryKey("dbo.NewsFiles", "Id");
            AddPrimaryKey("dbo.YachtInteriors", "Id");
            AddPrimaryKey("dbo.YachtDownloads", "Id");
            AddPrimaryKey("dbo.YachtImages", "Id");
        }
    }
}
