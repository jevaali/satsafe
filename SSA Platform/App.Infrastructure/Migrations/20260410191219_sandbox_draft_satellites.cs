using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class sandbox_draft_satellites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftSatellites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    SemiMajorAxis = table.Column<double>(type: "REAL", nullable: false),
                    Eccentricity = table.Column<double>(type: "REAL", nullable: false),
                    Inclination = table.Column<double>(type: "REAL", nullable: false),
                    RAAN = table.Column<double>(type: "REAL", nullable: false),
                    ArgumentOfPerigee = table.Column<double>(type: "REAL", nullable: false),
                    MeanAnomaly = table.Column<double>(type: "REAL", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftSatellites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftSatellites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftSatellites_UserId",
                table: "DraftSatellites",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftSatellites");
        }
    }
}
