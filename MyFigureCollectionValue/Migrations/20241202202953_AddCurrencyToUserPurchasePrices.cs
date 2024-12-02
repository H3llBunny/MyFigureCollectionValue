using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFigureCollectionValue.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToUserPurchasePrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "UserPurchasePrices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "UserPurchasePrices");
        }
    }
}
