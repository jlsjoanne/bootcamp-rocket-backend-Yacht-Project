namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameYachtNameIndex : DbMigration
    {
        public override void Up()
        {
            RenameIndex(table: "dbo.Yachts", name: "IX_Name", newName: "IX_Yacht_Name");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Yachts", name: "IX_Yacht_Name", newName: "IX_Name");
        }
    }
}
