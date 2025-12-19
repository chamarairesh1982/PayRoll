using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payroll.Infrastructure.Persistence;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PayrollDbContext))]
[Migration("20270119090000_AddStatutoryReportsAndEmployeeEpfNumber")]
public partial class AddStatutoryReportsAndEmployeeEpfNumber : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "EpfNumber",
            table: "Employees",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "StatutoryReports",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                PayRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                GeneratedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Checksum = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                WarningCount = table.Column<int>(type: "int", nullable: false),
                WarningFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                WarningFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                WarningContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StatutoryReports", x => x.Id);
                table.ForeignKey(
                    name: "FK_StatutoryReports_PayRuns_PayRunId",
                    column: x => x.PayRunId,
                    principalTable: "PayRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_StatutoryReports_GeneratedAtUtc",
            table: "StatutoryReports",
            column: "GeneratedAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_StatutoryReports_PayRunId_Type",
            table: "StatutoryReports",
            columns: new[] { "PayRunId", "Type" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "StatutoryReports");

        migrationBuilder.DropColumn(
            name: "EpfNumber",
            table: "Employees");
    }
}
