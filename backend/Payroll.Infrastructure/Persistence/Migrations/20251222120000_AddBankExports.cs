using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddBankExports : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BankExportTemplates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Format = table.Column<int>(type: "int", nullable: false),
                Delimiter = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                HeaderRowCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BankExportTemplates", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PayRunBankExports",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                GeneratedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                GeneratedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                DownloadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                DownloadedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                DownloadedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                table.PrimaryKey("PK_PayRunBankExports", x => x.Id);
                table.ForeignKey(
                    name: "FK_PayRunBankExports_BankExportTemplates_TemplateId",
                    column: x => x.TemplateId,
                    principalTable: "BankExportTemplates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PayRunBankExports_PayRuns_PayRunId",
                    column: x => x.PayRunId,
                    principalTable: "PayRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PayRunBankExportErrors",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayRunBankExportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayRunBankExportErrors", x => x.Id);
                table.ForeignKey(
                    name: "FK_PayRunBankExportErrors_PayRunBankExports_PayRunBankExportId",
                    column: x => x.PayRunBankExportId,
                    principalTable: "PayRunBankExports",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PayRunBankExportErrors_PayRunBankExportId",
            table: "PayRunBankExportErrors",
            column: "PayRunBankExportId");

        migrationBuilder.CreateIndex(
            name: "IX_PayRunBankExports_PayRunId_TemplateId",
            table: "PayRunBankExports",
            columns: new[] { "PayRunId", "TemplateId" });

        migrationBuilder.CreateIndex(
            name: "IX_PayRunBankExports_TemplateId",
            table: "PayRunBankExports",
            column: "TemplateId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PayRunBankExportErrors");

        migrationBuilder.DropTable(
            name: "PayRunBankExports");

        migrationBuilder.DropTable(
            name: "BankExportTemplates");
    }
}
