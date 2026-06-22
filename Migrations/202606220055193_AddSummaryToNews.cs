namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSummaryToNews : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.News", "Summary", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.News", "Summary");
        }
    }
}
