namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateNewsContentType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.News", "Content", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.News", "Content", c => c.String(maxLength: 1000));
        }
    }
}
