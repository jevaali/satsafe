using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThresholdDistance",
                table: "NotificationSettings",
                newName: "ConjunctionThresholdKm");

            migrationBuilder.AddColumn<double>(
                name: "AlertThresholdKm",
                table: "NotificationSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertThresholdKm",
                table: "NotificationSettings");

            migrationBuilder.RenameColumn(
                name: "ConjunctionThresholdKm",
                table: "NotificationSettings",
                newName: "ThresholdDistance");
        }
    }
}
