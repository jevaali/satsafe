using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Oparatorname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObjects_Operators_OperatorId",
                table: "SpaceObjects");

            migrationBuilder.AddColumn<string>(
                name: "OperatorName",
                table: "SpaceObjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceObjects_Operators_OperatorId",
                table: "SpaceObjects",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObjects_Operators_OperatorId",
                table: "SpaceObjects");

            migrationBuilder.DropColumn(
                name: "OperatorName",
                table: "SpaceObjects");

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceObjects_Operators_OperatorId",
                table: "SpaceObjects",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
