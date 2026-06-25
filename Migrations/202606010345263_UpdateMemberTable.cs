namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMemberTable : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Members", name: "id", newName: "Id");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.Members", name: "Id", newName: "id");
        }
    }
}
