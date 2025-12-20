using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddPayslipDocuments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PayslipDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                GeneratedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                GeneratedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ChecksumSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                ErrorSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayslipDocuments", x => x.Id);
                table.ForeignKey(
                    name: "FK_PayslipDocuments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PayslipDocuments_PayRuns_PayRunId",
                    column: x => x.PayRunId,
                    principalTable: "PayRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PayslipDocuments_EmployeeId",
            table: "PayslipDocuments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_PayslipDocuments_PayRunId_EmployeeId",
            table: "PayslipDocuments",
            columns: new[] { "PayRunId", "EmployeeId" });

        migrationBuilder.CreateIndex(
            name: "IX_PayslipDocuments_PayRunId",
            table: "PayslipDocuments",
            column: "PayRunId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PayslipDocuments");
    }
}
