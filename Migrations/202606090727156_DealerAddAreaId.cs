namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DealerAddAreaId : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Dealers", name: "Area_Id", newName: "AreaId");
            RenameIndex(table: "dbo.Dealers", name: "IX_Area_Id", newName: "IX_AreaId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Dealers", name: "IX_AreaId", newName: "IX_Area_Id");
            RenameColumn(table: "dbo.Dealers", name: "AreaId", newName: "Area_Id");
        }
    }
}
