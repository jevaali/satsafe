using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CalculatedAt",
                table: "RiskAssessments",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Operators",
                newName: "CreatedBy");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TleRecords",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TleRecords",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TleRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TleRecords",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "SpaceObject",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RiskCategory",
                table: "SpaceObject",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SpaceObject",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SpaceObject",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SpaceObject",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SpaceObject",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RiskAssessments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RiskAssessments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "RiskAssessments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SpaceObjectId",
                table: "Orbits",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Orbits",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Orbits",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orbits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Orbits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Operators",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OperatorType",
                table: "Operators",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorTypeString",
                table: "Operators",
                type: "TEXT",
                maxLength: 124,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Operators",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Operators",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "NotificationSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "NotificationSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "NotificationSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "NotificationSettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ConjunctionEvents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ConjunctionEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ConjunctionEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ConjunctionEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ConjunctionEvents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Severity",
                table: "Alerts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Alerts",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Alerts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Alerts",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Alerts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TleRecords");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TleRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TleRecords");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TleRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SpaceObject");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RiskAssessments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RiskAssessments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "RiskAssessments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Orbits");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Orbits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Orbits");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Orbits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "OperatorType",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "OperatorTypeString",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Operators");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ConjunctionEvents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ConjunctionEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ConjunctionEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ConjunctionEvents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "RiskAssessments",
                newName: "CalculatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Operators",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SpaceObject",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RiskCategory",
                table: "SpaceObject",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SpaceObjectId",
                table: "Orbits",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ConjunctionEvents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Alerts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
