using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrbitForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObjects_Orbits_OrbitId",
                table: "SpaceObjects");

            migrationBuilder.DropIndex(
                name: "IX_SpaceObjects_OrbitId",
                table: "SpaceObjects");

            migrationBuilder.CreateIndex(
                name: "IX_Orbits_SpaceObjectId",
                table: "Orbits",
                column: "SpaceObjectId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orbits_SpaceObjects_SpaceObjectId",
                table: "Orbits",
                column: "SpaceObjectId",
                principalTable: "SpaceObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orbits_SpaceObjects_SpaceObjectId",
                table: "Orbits");

            migrationBuilder.DropIndex(
                name: "IX_Orbits_SpaceObjectId",
                table: "Orbits");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceObjects_OrbitId",
                table: "SpaceObjects",
                column: "OrbitId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceObjects_Orbits_OrbitId",
                table: "SpaceObjects",
                column: "OrbitId",
                principalTable: "Orbits",
                principalColumn: "Id");
        }
    }
}
