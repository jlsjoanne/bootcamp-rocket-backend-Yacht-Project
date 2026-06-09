namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDealer : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DealerImages", new[] { "DealerId" });
            DropPrimaryKey("dbo.DealerImages");
            AddColumn("dbo.DealerImages", "FileName", c => c.String());
            AddColumn("dbo.DealerImages", "FileType", c => c.String());
            AlterColumn("dbo.DealerImages", "DealerId", c => c.Int(nullable: false));
            AlterColumn("dbo.DealerImages", "ContentType", c => c.String());
            AddPrimaryKey("dbo.DealerImages", "DealerId");
            CreateIndex("dbo.DealerImages", "DealerId");
            DropColumn("dbo.DealerImages", "Id");
            DropColumn("dbo.DealerImages", "OriginalFileName");
            DropColumn("dbo.DealerImages", "StoredFileName");
            DropColumn("dbo.DealerImages", "FileExtension");
            DropColumn("dbo.DealerImages", "FileSizeBytes");
            DropColumn("dbo.DealerImages", "ImageUrl");
            DropColumn("dbo.DealerImages", "UploadedAt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DealerImages", "UploadedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.DealerImages", "ImageUrl", c => c.String(maxLength: 500));
            AddColumn("dbo.DealerImages", "FileSizeBytes", c => c.Long(nullable: false));
            AddColumn("dbo.DealerImages", "FileExtension", c => c.String(maxLength: 20));
            AddColumn("dbo.DealerImages", "StoredFileName", c => c.String(maxLength: 255));
            AddColumn("dbo.DealerImages", "OriginalFileName", c => c.String(maxLength: 255));
            AddColumn("dbo.DealerImages", "Id", c => c.Int(nullable: false));
            DropIndex("dbo.DealerImages", new[] { "DealerId" });
            DropPrimaryKey("dbo.DealerImages");
            AlterColumn("dbo.DealerImages", "ContentType", c => c.String(maxLength: 100));
            AlterColumn("dbo.DealerImages", "DealerId", c => c.Int());
            DropColumn("dbo.DealerImages", "FileType");
            DropColumn("dbo.DealerImages", "FileName");
            AddPrimaryKey("dbo.DealerImages", "Id");
            CreateIndex("dbo.DealerImages", "DealerId");
        }
    }
}
