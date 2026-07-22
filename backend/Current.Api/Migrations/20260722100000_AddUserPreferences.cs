using System;
using Current.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Current.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260722100000_AddUserPreferences")]
    public partial class AddUserPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "en-AU");

            migrationBuilder.AddColumn<string>(
                name: "PreferredCurrency",
                table: "Users",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "AUD");

            migrationBuilder.AddColumn<string>(
                name: "ThemePreference",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Australia/Sydney");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Locale", table: "Users");
            migrationBuilder.DropColumn(name: "PreferredCurrency", table: "Users");
            migrationBuilder.DropColumn(name: "ThemePreference", table: "Users");
            migrationBuilder.DropColumn(name: "Timezone", table: "Users");
        }
    }
}
