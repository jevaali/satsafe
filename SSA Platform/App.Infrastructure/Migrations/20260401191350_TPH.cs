using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TPH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebrisObjects");

            migrationBuilder.DropTable(
                name: "Satellites");

            migrationBuilder.AddColumn<DateTime>(
                name: "LaunchDate",
                table: "SpaceObject",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SpaceObject",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorId",
                table: "SpaceObject",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskCategory",
                table: "SpaceObject",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SizeEstimate",
                table: "SpaceObject",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpaceObjectType",
                table: "SpaceObject",
                type: "TEXT",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SpaceObject",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpaceObject_OperatorId",
                table: "SpaceObject",
                column: "OperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceObject_Operators_OperatorId",
                table: "SpaceObject",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpaceObject_Operators_OperatorId",
                table: "SpaceObject");

            migrationBuilder.DropIndex(
                name: "IX_SpaceObject_OperatorId",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "LaunchDate",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "RiskCategory",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "SizeEstimate",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "SpaceObjectType",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SpaceObject");

            migrationBuilder.CreateTable(
                name: "DebrisObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RiskCategory = table.Column<int>(type: "INTEGER", nullable: false),
                    SizeEstimate = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebrisObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebrisObjects_SpaceObject_Id",
                        column: x => x.Id,
                        principalTable: "SpaceObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Satellites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OperatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LaunchDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Satellites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Satellites_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Satellites_SpaceObject_Id",
                        column: x => x.Id,
                        principalTable: "SpaceObject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Satellites_OperatorId",
                table: "Satellites",
                column: "OperatorId");
        }
    }
}
