using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Current.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccountDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contacts_UserId_Email",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contacts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Contacts",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bsb",
                table: "Contacts",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Accounts",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bsb",
                table: "Accounts",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Accounts"
                SET "Bsb" = '932-001',
                    "AccountNumber" = '00000001'
                WHERE "AccountType" = 'Branch';

                WITH numbered AS (
                    SELECT "Id",
                           ROW_NUMBER() OVER (ORDER BY "CreatedAt", "Id") AS row_number
                    FROM "Accounts"
                    WHERE "AccountType" <> 'Branch'
                )
                UPDATE "Accounts" AS account
                SET "Bsb" = '932-000',
                    "AccountNumber" = LPAD((10000000 + numbered.row_number)::text, 8, '0')
                FROM numbered
                WHERE account."Id" = numbered."Id";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Accounts",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bsb",
                table: "Accounts",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_UserId_Bsb_AccountNumber",
                table: "Contacts",
                columns: new[] { "UserId", "Bsb", "AccountNumber" },
                unique: true,
                filter: "\"Bsb\" IS NOT NULL AND \"AccountNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_UserId_Email",
                table: "Contacts",
                columns: new[] { "UserId", "Email" },
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Bsb_AccountNumber",
                table: "Accounts",
                columns: new[] { "Bsb", "AccountNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contacts_UserId_Bsb_AccountNumber",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_UserId_Email",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Bsb_AccountNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Bsb",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Bsb",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Contacts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_UserId_Email",
                table: "Contacts",
                columns: new[] { "UserId", "Email" },
                unique: true);
        }
    }
}
