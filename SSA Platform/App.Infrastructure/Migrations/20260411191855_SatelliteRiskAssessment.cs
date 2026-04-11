using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SatelliteRiskAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessments_ConjunctionEvents_ConjunctionEventId",
                table: "RiskAssessments");

            migrationBuilder.DropColumn(
                name: "RiskAssessmentId",
                table: "ConjunctionEvents");

            migrationBuilder.RenameColumn(
                name: "ConjunctionEventId",
                table: "RiskAssessments",
                newName: "SatelliteId");

            migrationBuilder.RenameIndex(
                name: "IX_RiskAssessments_ConjunctionEventId",
                table: "RiskAssessments",
                newName: "IX_RiskAssessments_SatelliteId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CalculatedAt",
                table: "RiskAssessments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_RiskAssessments_SpaceObjects_SatelliteId",
                table: "RiskAssessments",
                column: "SatelliteId",
                principalTable: "SpaceObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessments_SpaceObjects_SatelliteId",
                table: "RiskAssessments");

            migrationBuilder.DropColumn(
                name: "CalculatedAt",
                table: "RiskAssessments");

            migrationBuilder.RenameColumn(
                name: "SatelliteId",
                table: "RiskAssessments",
                newName: "ConjunctionEventId");

            migrationBuilder.RenameIndex(
                name: "IX_RiskAssessments_SatelliteId",
                table: "RiskAssessments",
                newName: "IX_RiskAssessments_ConjunctionEventId");

            migrationBuilder.AddColumn<Guid>(
                name: "RiskAssessmentId",
                table: "ConjunctionEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskAssessments_ConjunctionEvents_ConjunctionEventId",
                table: "RiskAssessments",
                column: "ConjunctionEventId",
                principalTable: "ConjunctionEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
