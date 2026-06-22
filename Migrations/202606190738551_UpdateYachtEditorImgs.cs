namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateYachtEditorImgs : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.YachtEditorImages");

            CreateTable(
                "dbo.YachtEditorImages",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    OriginalFileName = c.String(nullable: false, maxLength: 255),
                    FileType = c.String(nullable: false, maxLength: 20),
                    ContentType = c.String(nullable: false, maxLength: 100),
                    FilePath = c.String(nullable: false, maxLength: 500),
                    YachtId = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Yachts", t => t.YachtId, cascadeDelete: true)
                .Index(t => t.YachtId);
        }
        
        public override void Down()
        {
            DropTable("dbo.YachtEditorImages");

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
        }
    }
}
