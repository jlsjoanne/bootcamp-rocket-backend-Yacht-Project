namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDbMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Area",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        CountryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Country", t => t.CountryId, cascadeDelete: true)
                .Index(t => t.CountryId);
            
            CreateTable(
                "dbo.Country",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Dealer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Content = c.String(),
                        Area_Id = c.Int(),
                        Image_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Area", t => t.Area_Id)
                .ForeignKey("dbo.UploadedFile", t => t.Image_Id)
                .Index(t => t.Area_Id)
                .Index(t => t.Image_Id);
            
            CreateTable(
                "dbo.UploadedFile",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FileName = c.String(),
                        FileType = c.String(),
                        ContentType = c.String(),
                        Yacht_Id = c.Int(),
                        Yacht_Id1 = c.Int(),
                        Yacht_Id2 = c.Int(),
                        News_Id = c.Int(),
                        News_Id1 = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Yacht", t => t.Yacht_Id)
                .ForeignKey("dbo.Yacht", t => t.Yacht_Id1)
                .ForeignKey("dbo.Yacht", t => t.Yacht_Id2)
                .ForeignKey("dbo.News", t => t.News_Id)
                .ForeignKey("dbo.News", t => t.News_Id1)
                .Index(t => t.Yacht_Id)
                .Index(t => t.Yacht_Id1)
                .Index(t => t.Yacht_Id2)
                .Index(t => t.News_Id)
                .Index(t => t.News_Id1);
            
            CreateTable(
                "dbo.Contact",
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
                .ForeignKey("dbo.Country", t => t.CountryId, cascadeDelete: true)
                .ForeignKey("dbo.Yacht", t => t.YachtId, cascadeDelete: true)
                .Index(t => t.CountryId)
                .Index(t => t.YachtId);
            
            CreateTable(
                "dbo.Yacht",
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
                .ForeignKey("dbo.UploadedFile", t => t.OverviewImage_Id)
                .Index(t => t.Name, unique: true)
                .Index(t => t.OverviewImage_Id);
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UploadedFile", "News_Id1", "dbo.News");
            DropForeignKey("dbo.UploadedFile", "News_Id", "dbo.News");
            DropForeignKey("dbo.Contact", "YachtId", "dbo.Yacht");
            DropForeignKey("dbo.Yacht", "OverviewImage_Id", "dbo.UploadedFile");
            DropForeignKey("dbo.UploadedFile", "Yacht_Id2", "dbo.Yacht");
            DropForeignKey("dbo.UploadedFile", "Yacht_Id1", "dbo.Yacht");
            DropForeignKey("dbo.UploadedFile", "Yacht_Id", "dbo.Yacht");
            DropForeignKey("dbo.Contact", "CountryId", "dbo.Country");
            DropForeignKey("dbo.Dealer", "Image_Id", "dbo.UploadedFile");
            DropForeignKey("dbo.Dealer", "Area_Id", "dbo.Area");
            DropForeignKey("dbo.Area", "CountryId", "dbo.Country");
            DropIndex("dbo.Yacht", new[] { "OverviewImage_Id" });
            DropIndex("dbo.Yacht", new[] { "Name" });
            DropIndex("dbo.Contact", new[] { "YachtId" });
            DropIndex("dbo.Contact", new[] { "CountryId" });
            DropIndex("dbo.UploadedFile", new[] { "News_Id1" });
            DropIndex("dbo.UploadedFile", new[] { "News_Id" });
            DropIndex("dbo.UploadedFile", new[] { "Yacht_Id2" });
            DropIndex("dbo.UploadedFile", new[] { "Yacht_Id1" });
            DropIndex("dbo.UploadedFile", new[] { "Yacht_Id" });
            DropIndex("dbo.Dealer", new[] { "Image_Id" });
            DropIndex("dbo.Dealer", new[] { "Area_Id" });
            DropIndex("dbo.Area", new[] { "CountryId" });
            DropTable("dbo.News");
            DropTable("dbo.Yacht");
            DropTable("dbo.Contact");
            DropTable("dbo.UploadedFile");
            DropTable("dbo.Dealer");
            DropTable("dbo.Country");
            DropTable("dbo.Area");
        }
    }
}
