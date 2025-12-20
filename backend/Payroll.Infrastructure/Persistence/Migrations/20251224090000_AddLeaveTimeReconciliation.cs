using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddLeaveTimeReconciliation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "NoPayDays",
            table: "DeductionLines",
            type: "decimal(5,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "NoPayHours",
            table: "DeductionLines",
            type: "decimal(7,2)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MetadataJson",
            table: "DeductionLines",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "LeaveEncashmentRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LeaveType = table.Column<int>(type: "int", nullable: false),
                Days = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LeaveEncashmentRequests", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "LeaveTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                IsPaid = table.Column<bool>(type: "bit", nullable: false),
                AllowsHalfDay = table.Column<bool>(type: "bit", nullable: false),
                Encashable = table.Column<bool>(type: "bit", nullable: false),
                EncashmentRateMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LeaveTypes", x => x.Id);
            });

        migrationBuilder.UpdateData(
            table: "DeductionTypes",
            keyColumn: "Id",
            keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
            columns: new[] { "Code", "Name" },
            values: new object[] { "DED_NO_PAY", "No Pay" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LeaveEncashmentRequests");

        migrationBuilder.DropTable(
            name: "LeaveTypes");

        migrationBuilder.DropColumn(
            name: "NoPayDays",
            table: "DeductionLines");

        migrationBuilder.DropColumn(
            name: "NoPayHours",
            table: "DeductionLines");

        migrationBuilder.DropColumn(
            name: "MetadataJson",
            table: "DeductionLines");

        migrationBuilder.UpdateData(
            table: "DeductionTypes",
            keyColumn: "Id",
            keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
            columns: new[] { "Code", "Name" },
            values: new object[] { "NOPAY", "No Pay Deduction" });
    }
}
