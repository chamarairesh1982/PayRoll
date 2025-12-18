using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddOvertimeRuleConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<double>(
            name: "OvertimeDailyCapHours",
            table: "PayrollSettings",
            type: "float",
            nullable: false,
            defaultValue: 12.0);

        migrationBuilder.AddColumn<double>(
            name: "OvertimePayRunCapHours",
            table: "PayrollSettings",
            type: "float",
            nullable: false,
            defaultValue: 80.0);

        migrationBuilder.AddColumn<int>(
            name: "OvertimeRoundingMinutes",
            table: "PayrollSettings",
            type: "int",
            nullable: false,
            defaultValue: 15);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "OvertimeDailyCapHours",
            table: "PayrollSettings");

        migrationBuilder.DropColumn(
            name: "OvertimePayRunCapHours",
            table: "PayrollSettings");

        migrationBuilder.DropColumn(
            name: "OvertimeRoundingMinutes",
            table: "PayrollSettings");
    }
}
