using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObjects_PrimaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObjects_SecondaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DebrisObjects_SpaceObjects_Id",
                table: "DebrisObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Satellites_SpaceObjects_Id",
                table: "Satellites");

            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObjects_Orbits_OrbitId",
                table: "SpaceObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TleRecords_SpaceObjects_SpaceObjectId",
                table: "TleRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpaceObjects",
                table: "SpaceObjects");

            migrationBuilder.RenameTable(
                name: "SpaceObjects",
                newName: "SpaceObject");

            migrationBuilder.RenameIndex(
                name: "IX_SpaceObjects_OrbitId",
                table: "SpaceObject",
                newName: "IX_SpaceObject_OrbitId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpaceObject",
                table: "SpaceObject",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConjunctionEvents_SpaceObject_PrimaryObjectId",
                table: "ConjunctionEvents",
                column: "PrimaryObjectId",
                principalTable: "SpaceObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConjunctionEvents_SpaceObject_SecondaryObjectId",
                table: "ConjunctionEvents",
                column: "SecondaryObjectId",
                principalTable: "SpaceObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DebrisObjects_SpaceObject_Id",
                table: "DebrisObjects",
                column: "Id",
                principalTable: "SpaceObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Satellites_SpaceObject_Id",
                table: "Satellites",
                column: "Id",
                principalTable: "SpaceObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceObject_Orbits_OrbitId",
                table: "SpaceObject",
                column: "OrbitId",
                principalTable: "Orbits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TleRecords_SpaceObject_SpaceObjectId",
                table: "TleRecords",
                column: "SpaceObjectId",
                principalTable: "SpaceObject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObject_PrimaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObject_SecondaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DebrisObjects_SpaceObject_Id",
                table: "DebrisObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Satellites_SpaceObject_Id",
                table: "Satellites");

            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObject_Orbits_OrbitId",
                table: "SpaceObject");

            migrationBuilder.DropForeignKey(
                name: "FK_TleRecords_SpaceObject_SpaceObjectId",
                table: "TleRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SpaceObject",
                table: "SpaceObject");

            migrationBuilder.RenameTable(
                name: "SpaceObject",
                newName: "SpaceObjects");

            migrationBuilder.RenameIndex(
                name: "IX_SpaceObject_OrbitId",
                table: "SpaceObjects",
                newName: "IX_SpaceObjects_OrbitId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpaceObjects",
                table: "SpaceObjects",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConjunctionEvents_SpaceObjects_PrimaryObjectId",
                table: "ConjunctionEvents",
                column: "PrimaryObjectId",
                principalTable: "SpaceObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConjunctionEvents_SpaceObjects_SecondaryObjectId",
                table: "ConjunctionEvents",
                column: "SecondaryObjectId",
                principalTable: "SpaceObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DebrisObjects_SpaceObjects_Id",
                table: "DebrisObjects",
                column: "Id",
                principalTable: "SpaceObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Satellites_SpaceObjects_Id",
                table: "Satellites",
                column: "Id",
                principalTable: "SpaceObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceObjects_Orbits_OrbitId",
                table: "SpaceObjects",
                column: "OrbitId",
                principalTable: "Orbits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TleRecords_SpaceObjects_SpaceObjectId",
                table: "TleRecords",
                column: "SpaceObjectId",
                principalTable: "SpaceObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
