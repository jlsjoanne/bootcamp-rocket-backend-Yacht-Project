namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateFileStringLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DealerImages", "OriginalFileName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.DealerImages", "FileType", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.DealerImages", "ContentType", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.DealerImages", "FilePath", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.YachtImages", "OriginalFileName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.YachtImages", "FileType", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.YachtImages", "ContentType", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.YachtImages", "FilePath", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.YachtDownloads", "OriginalFileName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.YachtDownloads", "FileType", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.YachtDownloads", "ContentType", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.YachtDownloads", "FilePath", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.YachtInteriors", "OriginalFileName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.YachtInteriors", "FileType", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.YachtInteriors", "ContentType", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.YachtInteriors", "FilePath", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.NewsFiles", "OriginalFileName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.NewsFiles", "FileType", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.NewsFiles", "ContentType", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.NewsFiles", "FilePath", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.NewsImages", "OriginalFileName", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.NewsImages", "FileType", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.NewsImages", "ContentType", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.NewsImages", "FilePath", c => c.String(nullable: false, maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NewsImages", "FilePath", c => c.String());
            AlterColumn("dbo.NewsImages", "ContentType", c => c.String());
            AlterColumn("dbo.NewsImages", "FileType", c => c.String());
            AlterColumn("dbo.NewsImages", "OriginalFileName", c => c.String());
            AlterColumn("dbo.NewsFiles", "FilePath", c => c.String());
            AlterColumn("dbo.NewsFiles", "ContentType", c => c.String());
            AlterColumn("dbo.NewsFiles", "FileType", c => c.String());
            AlterColumn("dbo.NewsFiles", "OriginalFileName", c => c.String());
            AlterColumn("dbo.YachtInteriors", "FilePath", c => c.String());
            AlterColumn("dbo.YachtInteriors", "ContentType", c => c.String());
            AlterColumn("dbo.YachtInteriors", "FileType", c => c.String());
            AlterColumn("dbo.YachtInteriors", "OriginalFileName", c => c.String());
            AlterColumn("dbo.YachtDownloads", "FilePath", c => c.String());
            AlterColumn("dbo.YachtDownloads", "ContentType", c => c.String());
            AlterColumn("dbo.YachtDownloads", "FileType", c => c.String());
            AlterColumn("dbo.YachtDownloads", "OriginalFileName", c => c.String());
            AlterColumn("dbo.YachtImages", "FilePath", c => c.String());
            AlterColumn("dbo.YachtImages", "ContentType", c => c.String());
            AlterColumn("dbo.YachtImages", "FileType", c => c.String());
            AlterColumn("dbo.YachtImages", "OriginalFileName", c => c.String());
            AlterColumn("dbo.DealerImages", "FilePath", c => c.String());
            AlterColumn("dbo.DealerImages", "ContentType", c => c.String());
            AlterColumn("dbo.DealerImages", "FileType", c => c.String());
            AlterColumn("dbo.DealerImages", "OriginalFileName", c => c.String());
        }
    }
}
