using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Current.Api.Migrations
{
    /// <inheritdoc />
    public partial class BackfillTransactionCategoryTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Transactions"
                SET "Category" = 'Transfer'
                WHERE "Category" = '' OR "Category" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
