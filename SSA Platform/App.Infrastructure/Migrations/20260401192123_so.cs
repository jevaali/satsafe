using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class so : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObject_PrimaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObject_SecondaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObject_Operators_OperatorId",
                table: "SpaceObject");

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

            migrationBuilder.RenameIndex(
                name: "IX_SpaceObject_OperatorId",
                table: "SpaceObjects",
                newName: "IX_SpaceObjects_OperatorId");

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
                name: "FK_SpaceObjects_Operators_OperatorId",
                table: "SpaceObjects",
                column: "OperatorId",
                principalTable: "Operators",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObjects_PrimaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ConjunctionEvents_SpaceObjects_SecondaryObjectId",
                table: "ConjunctionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObjects_Operators_OperatorId",
                table: "SpaceObjects");

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

            migrationBuilder.RenameIndex(
                name: "IX_SpaceObjects_OperatorId",
                table: "SpaceObject",
                newName: "IX_SpaceObject_OperatorId");

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
                name: "FK_SpaceObject_Operators_OperatorId",
                table: "SpaceObject",
                column: "OperatorId",
                principalTable: "Operators",
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
    }
}
