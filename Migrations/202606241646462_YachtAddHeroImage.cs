namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class YachtAddHeroImage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.YachtHeroImages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        YachtId = c.Int(nullable: false),
                        OriginalFileName = c.String(nullable: false, maxLength: 255),
                        FileType = c.String(nullable: false, maxLength: 20),
                        ContentType = c.String(nullable: false, maxLength: 100),
                        FilePath = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Yachts", t => t.YachtId, cascadeDelete: true)
                .Index(t => t.YachtId, unique: true, name: "IX_YachtHeroImage_YachtId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.YachtHeroImages", "YachtId", "dbo.Yachts");
            DropIndex("dbo.YachtHeroImages", "IX_YachtHeroImage_YachtId");
            DropTable("dbo.YachtHeroImages");
        }
    }
}
