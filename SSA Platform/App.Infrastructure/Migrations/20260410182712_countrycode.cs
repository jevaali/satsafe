using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class countrycode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Operators");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Operators",
                type: "TEXT",
                maxLength: 124,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Operators");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Operators",
                type: "jsonb",
                maxLength: 124,
                nullable: false,
                defaultValue: "");
        }
    }
}
