using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Current.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TreasuryAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Accounts_TreasuryAccountId",
                        column: x => x.TreasuryAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TreasuryAccountId",
                table: "Branches",
                column: "TreasuryAccountId");

            var seedTimestamp = new DateTime(2026, 7, 22, 9, 40, 51, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[]
                {
                    "Id", "FirstName", "LastName", "Email", "PasswordHash", "Role",
                    "CreatedAt", "UpdatedAt"
                },
                values: new object[]
                {
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Current",
                    "Branch",
                    "branch-system@current.internal",
                    "SYSTEM_NO_LOGIN",
                    "Admin",
                    seedTimestamp,
                    seedTimestamp
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[]
                {
                    "Id", "UserId", "Name", "AccountType", "CurrentBalance", "Currency",
                    "CreatedAt", "UpdatedAt"
                },
                values: new object[]
                {
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Current HQ Treasury",
                    "Branch",
                    10000000m,
                    "AUD",
                    seedTimestamp,
                    seedTimestamp
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[]
                {
                    "Id", "Name", "Code", "TreasuryAccountId", "CreatedAt", "UpdatedAt"
                },
                values: new object[]
                {
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    "Current HQ",
                    "HQ",
                    Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    seedTimestamp,
                    seedTimestamp
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: Guid.Parse("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: Guid.Parse("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: Guid.Parse("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DropTable(
                name: "Branches");
        }
    }
}
