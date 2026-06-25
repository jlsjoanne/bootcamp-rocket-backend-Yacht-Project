namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContactAddIsDeleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Contacts", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Contacts", "IsDeleted");
        }
    }
}
