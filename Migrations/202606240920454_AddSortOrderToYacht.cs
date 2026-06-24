namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddSortOrderToYacht : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Yachts", "SortOrder", c => c.Int(nullable: false, defaultValue: 0));

            Sql(@"
;WITH OrderedYachts AS
        (
            SELECT
                Id,
                ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
            FROM dbo.Yachts
        )
        UPDATE y
        SET SortOrder = oy.RowNumber * 10
        FROM dbo.Yachts y
        INNER JOIN OrderedYachts oy ON y.Id = oy.Id;
");
        }

        public override void Down()
        {
            DropColumn("dbo.Yachts", "SortOrder");
        }
    }
}
