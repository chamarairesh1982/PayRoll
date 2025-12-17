using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payroll.Infrastructure.Persistence;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PayrollDbContext))]
[Migration("20250601120000_AddPayrollSettingsAndOvertimeLocking")]
public partial class AddPayrollSettingsAndOvertimeLocking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "PayRunId",
            table: "OvertimeRecords",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "PayrollSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WorkingDaysPerMonth = table.Column<int>(type: "int", nullable: false),
                WorkingHoursPerDay = table.Column<int>(type: "int", nullable: false),
                WeekdayOvertimeMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                WeekendOvertimeMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                HolidayOvertimeMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayrollSettings", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PayrollSettings");

        migrationBuilder.DropColumn(
            name: "PayRunId",
            table: "OvertimeRecords");
    }
}
