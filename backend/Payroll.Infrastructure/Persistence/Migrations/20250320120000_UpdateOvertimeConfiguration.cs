using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payroll.Infrastructure.Persistence;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(PayrollDbContext))]
    [Migration("20250320120000_UpdateOvertimeConfiguration")]
    public partial class UpdateOvertimeConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "AppliesOnHoliday",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "AppliesOnWeekend",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "DailyCapHours",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "HolidayMultiplier",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "PayRunCapHours",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "RoundingMinutes",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "WeekdayMultiplier",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "WeekendMultiplier",
                table: "OTRules");

            migrationBuilder.AddColumn<double>(
                name: "DailyHoursCap",
                table: "OTRules",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                table: "OTRules",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2020, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                table: "OTRules",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MonthlyHoursCap",
                table: "OTRules",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Multiplier",
                table: "OTRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.AddColumn<int>(
                name: "RoundToMinutes",
                table: "OTRules",
                type: "int",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<int>(
                name: "RoundingMode",
                table: "OTRules",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "OTRules",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "OvertimeRecords");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "OvertimeRecords");

            migrationBuilder.DropColumn(
                name: "Hours",
                table: "OvertimeRecords");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "OvertimeRecords");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAtUtc",
                table: "OvertimeRecords",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "OvertimeRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "OvertimeRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "OvertimeRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RawMinutes",
                table: "OvertimeRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OvertimeRecords",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DailyHoursCap",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "MonthlyHoursCap",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "Multiplier",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "RoundToMinutes",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "RoundingMode",
                table: "OTRules");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "OTRules");

            migrationBuilder.AddColumn<bool>(
                name: "AppliesOnHoliday",
                table: "OTRules",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AppliesOnWeekend",
                table: "OTRules",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "DailyCapHours",
                table: "OTRules",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayMultiplier",
                table: "OTRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "OTRules",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PayRunCapHours",
                table: "OTRules",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "RoundingMinutes",
                table: "OTRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WeekdayMultiplier",
                table: "OTRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WeekendMultiplier",
                table: "OTRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "OvertimeRecords");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "OvertimeRecords");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "OvertimeRecords");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "OvertimeRecords");

            migrationBuilder.DropColumn(
                name: "RawMinutes",
                table: "OvertimeRecords");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovedAt",
                table: "OvertimeRecords",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById",
                table: "OvertimeRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Hours",
                table: "OvertimeRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "OvertimeRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
