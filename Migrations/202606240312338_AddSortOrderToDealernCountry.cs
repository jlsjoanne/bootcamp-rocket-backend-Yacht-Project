namespace TayanaYachts.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddSortOrderToDealernCountry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Countries", "SortOrder", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Dealers", "SortOrder", c => c.Int(nullable: false, defaultValue: 0));

            Sql(@"
;WITH OrderedCountries AS
          (
              SELECT
                  Id,
                  ROW_NUMBER() OVER (
                      ORDER BY
                          CASE
                              WHEN Name IN ('USA', 'United States', 'Unite States') THEN 0
                              ELSE 1
                          END,
                          Name,
                          Id
                  ) AS RowNumber
              FROM dbo.Countries
          )
          UPDATE c
          SET SortOrder = oc.RowNumber * 10
          FROM dbo.Countries c
          INNER JOIN OrderedCountries oc ON c.Id = oc.Id;
"); 
        }

        public override void Down()
        {
            DropColumn("dbo.Dealers", "SortOrder");
            DropColumn("dbo.Countries", "SortOrder");
        }
    }
}
