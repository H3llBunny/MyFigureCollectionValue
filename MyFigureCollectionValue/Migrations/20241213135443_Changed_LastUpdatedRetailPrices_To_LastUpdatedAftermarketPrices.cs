using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFigureCollectionValue.Migrations
{
    /// <inheritdoc />
    public partial class Changed_LastUpdatedRetailPrices_To_LastUpdatedAftermarketPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdatedRetailPrices",
                table: "Figures",
                newName: "LastUpdatedAftermarketPrices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdatedAftermarketPrices",
                table: "Figures",
                newName: "LastUpdatedRetailPrices");
        }
    }
}
