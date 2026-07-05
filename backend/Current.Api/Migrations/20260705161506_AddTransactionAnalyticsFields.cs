using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Current.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionAnalyticsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Transfer");

            migrationBuilder.AddColumn<string>(
                name: "Merchant",
                table: "Transactions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Merchant",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Transactions");
        }
    }
}
