namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContactAddIscompleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Contacts", "IsCompleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Contacts", "IsCompleted");
        }
    }
}
