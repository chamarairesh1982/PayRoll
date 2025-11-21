using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddLeaveRequestsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LeaveRequests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LeaveType = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                TotalDays = table.Column<double>(type: "float", nullable: false),
                Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                IsHalfDay = table.Column<bool>(type: "bit", nullable: true),
                HalfDaySession = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LeaveRequests", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LeaveRequests_EmployeeId_StartDate",
            table: "LeaveRequests",
            columns: new[] { "EmployeeId", "StartDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LeaveRequests");
    }
}
