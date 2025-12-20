using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddGeneratedTaxDocuments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GeneratedTaxDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                Year = table.Column<int>(type: "int", nullable: true),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                GeneratedByUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                GeneratedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ChecksumSha256 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ErrorSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GeneratedTaxDocuments", x => x.Id);
                table.ForeignKey(
                    name: "FK_GeneratedTaxDocuments_Branches_BranchId",
                    column: x => x.BranchId,
                    principalTable: "Branches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_GeneratedTaxDocuments_Companies_CompanyId",
                    column: x => x.CompanyId,
                    principalTable: "Companies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_GeneratedTaxDocuments_CostCenters_CostCenterId",
                    column: x => x.CostCenterId,
                    principalTable: "CostCenters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_GeneratedTaxDocuments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GeneratedTaxDocuments_BranchId",
            table: "GeneratedTaxDocuments",
            column: "BranchId");

        migrationBuilder.CreateIndex(
            name: "IX_GeneratedTaxDocuments_CompanyId",
            table: "GeneratedTaxDocuments",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_GeneratedTaxDocuments_CostCenterId",
            table: "GeneratedTaxDocuments",
            column: "CostCenterId");

        migrationBuilder.CreateIndex(
            name: "IX_GeneratedTaxDocuments_EmployeeId",
            table: "GeneratedTaxDocuments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_GeneratedTaxDocuments_Type_Year_PeriodStart_PeriodEnd_EmployeeId_CompanyId_BranchId_CostCenterId",
            table: "GeneratedTaxDocuments",
            columns: new[] { "Type", "Year", "PeriodStart", "PeriodEnd", "EmployeeId", "CompanyId", "BranchId", "CostCenterId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "GeneratedTaxDocuments");
    }
}
