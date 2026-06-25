namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDbMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Areas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        CountryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Countries", t => t.CountryId, cascadeDelete: true)
                .Index(t => t.CountryId);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Dealers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Content = c.String(),
                        Area_Id = c.Int(),
                        Image_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Areas", t => t.Area_Id)
                .Index(t => t.Area_Id);
            
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
            
            CreateTable(
                "dbo.News",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 255),
                        Content = c.String(maxLength: 1000),
                        PublishDate = c.DateTime(nullable: false),
                        IsPinned = c.Boolean(nullable: false),
                        IsPublished = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Yachts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsNew = c.Boolean(nullable: false),
                        IsPublished = c.Boolean(nullable: false),
                        PostDate = c.DateTime(nullable: false),
                        Overview = c.String(),
                        Dimensions = c.String(),
                        Specification = c.String(nullable: false),
                        OverviewImage_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.Contacts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        Phone = c.String(nullable: false, maxLength: 25),
                        CountryId = c.Int(nullable: false),
                        YachtId = c.Int(nullable: false),
                        Comment = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Countries", t => t.CountryId, cascadeDelete: true)
                .ForeignKey("dbo.Yachts", t => t.YachtId, cascadeDelete: true)
                .Index(t => t.CountryId)
                .Index(t => t.YachtId);
            
            CreateTable(
                "dbo.NewsFiles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        ContentType = c.String(),
                        NewsId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.News", t => t.NewsId, cascadeDelete: true)
                .Index(t => t.NewsId);
            
            CreateTable(
                "dbo.NewsImages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        ContentType = c.String(),
                        NewsId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.News", t => t.NewsId, cascadeDelete: true)
                .Index(t => t.NewsId);
            
            CreateTable(
                "dbo.YachtDownloads",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        ContentType = c.String(),
                        YachtId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Yachts", t => t.YachtId, cascadeDelete: true)
                .Index(t => t.YachtId);
            
            CreateTable(
                "dbo.YachtImages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        ContentType = c.String(),
                        YachtId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Yachts", t => t.YachtId, cascadeDelete: true)
                .Index(t => t.YachtId);
            
            CreateTable(
                "dbo.YachtInteriors",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        ContentType = c.String(),
                        YachtId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Yachts", t => t.YachtId, cascadeDelete: true)
                .Index(t => t.YachtId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.YachtInteriors", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtImages", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.YachtDownloads", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.NewsImages", "NewsId", "dbo.News");
            DropForeignKey("dbo.NewsFiles", "NewsId", "dbo.News");
            DropForeignKey("dbo.Contacts", "YachtId", "dbo.Yachts");
            DropForeignKey("dbo.Contacts", "CountryId", "dbo.Countries");
            DropForeignKey("dbo.Dealers", "Area_Id", "dbo.Areas");
            DropForeignKey("dbo.Areas", "CountryId", "dbo.Countries");
            DropIndex("dbo.YachtInteriors", new[] { "YachtId" });
            DropIndex("dbo.YachtImages", new[] { "YachtId" });
            DropIndex("dbo.YachtDownloads", new[] { "YachtId" });
            DropIndex("dbo.NewsImages", new[] { "NewsId" });
            DropIndex("dbo.NewsFiles", new[] { "NewsId" });
            DropIndex("dbo.Contacts", new[] { "YachtId" });
            DropIndex("dbo.Contacts", new[] { "CountryId" });
            DropIndex("dbo.Yachts", new[] { "Name" });
            DropIndex("dbo.Dealers", new[] { "Area_Id" });
            DropIndex("dbo.Areas", new[] { "CountryId" });
            DropTable("dbo.YachtInteriors");
            DropTable("dbo.YachtImages");
            DropTable("dbo.YachtDownloads");
            DropTable("dbo.NewsImages");
            DropTable("dbo.NewsFiles");
            DropTable("dbo.Contacts");
            DropTable("dbo.Yachts");
            DropTable("dbo.News");
            DropTable("dbo.UploadedFiles");
            DropTable("dbo.Dealers");
            DropTable("dbo.Countries");
            DropTable("dbo.Areas");
        }
    }
}
