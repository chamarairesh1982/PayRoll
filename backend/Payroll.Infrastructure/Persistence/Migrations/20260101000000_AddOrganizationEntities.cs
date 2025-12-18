using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddOrganizationEntities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "BranchId",
            table: "PayRuns",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "CompanyId",
            table: "PayRuns",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "CostCenterId",
            table: "PayRuns",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsConsolidated",
            table: "PayRuns",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<Guid>(
            name: "BranchId",
            table: "Employees",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "CompanyId",
            table: "Employees",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "CostCenterId",
            table: "Employees",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Companies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Companies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Branches",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Branches", x => x.Id);
                table.ForeignKey(
                    name: "FK_Branches_Companies_CompanyId",
                    column: x => x.CompanyId,
                    principalTable: "Companies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CostCenters",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CostCenters", x => x.Id);
                table.ForeignKey(
                    name: "FK_CostCenters_Branches_BranchId",
                    column: x => x.BranchId,
                    principalTable: "Branches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CostCenters_Companies_CompanyId",
                    column: x => x.CompanyId,
                    principalTable: "Companies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Branches_Code",
            table: "Branches",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Branches_CompanyId",
            table: "Branches",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_Companies_Code",
            table: "Companies",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CostCenters_BranchId",
            table: "CostCenters",
            column: "BranchId");

        migrationBuilder.CreateIndex(
            name: "IX_CostCenters_Code",
            table: "CostCenters",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CostCenters_CompanyId",
            table: "CostCenters",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_BranchId",
            table: "Employees",
            column: "BranchId");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_CompanyId",
            table: "Employees",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_CostCenterId",
            table: "Employees",
            column: "CostCenterId");

        migrationBuilder.CreateIndex(
            name: "IX_PayRuns_BranchId",
            table: "PayRuns",
            column: "BranchId");

        migrationBuilder.CreateIndex(
            name: "IX_PayRuns_CompanyId",
            table: "PayRuns",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_PayRuns_CostCenterId",
            table: "PayRuns",
            column: "CostCenterId");

        migrationBuilder.AddForeignKey(
            name: "FK_Employees_Branches_BranchId",
            table: "Employees",
            column: "BranchId",
            principalTable: "Branches",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Employees_Companies_CompanyId",
            table: "Employees",
            column: "CompanyId",
            principalTable: "Companies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Employees_CostCenters_CostCenterId",
            table: "Employees",
            column: "CostCenterId",
            principalTable: "CostCenters",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PayRuns_Branches_BranchId",
            table: "PayRuns",
            column: "BranchId",
            principalTable: "Branches",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PayRuns_Companies_CompanyId",
            table: "PayRuns",
            column: "CompanyId",
            principalTable: "Companies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_PayRuns_CostCenters_CostCenterId",
            table: "PayRuns",
            column: "CostCenterId",
            principalTable: "CostCenters",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Employees_Branches_BranchId",
            table: "Employees");

        migrationBuilder.DropForeignKey(
            name: "FK_Employees_Companies_CompanyId",
            table: "Employees");

        migrationBuilder.DropForeignKey(
            name: "FK_Employees_CostCenters_CostCenterId",
            table: "Employees");

        migrationBuilder.DropForeignKey(
            name: "FK_PayRuns_Branches_BranchId",
            table: "PayRuns");

        migrationBuilder.DropForeignKey(
            name: "FK_PayRuns_Companies_CompanyId",
            table: "PayRuns");

        migrationBuilder.DropForeignKey(
            name: "FK_PayRuns_CostCenters_CostCenterId",
            table: "PayRuns");

        migrationBuilder.DropTable(
            name: "CostCenters");

        migrationBuilder.DropTable(
            name: "Branches");

        migrationBuilder.DropTable(
            name: "Companies");

        migrationBuilder.DropIndex(
            name: "IX_PayRuns_BranchId",
            table: "PayRuns");

        migrationBuilder.DropIndex(
            name: "IX_PayRuns_CompanyId",
            table: "PayRuns");

        migrationBuilder.DropIndex(
            name: "IX_PayRuns_CostCenterId",
            table: "PayRuns");

        migrationBuilder.DropIndex(
            name: "IX_Employees_BranchId",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Employees_CompanyId",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Employees_CostCenterId",
            table: "Employees");

        migrationBuilder.DropColumn(
            name: "BranchId",
            table: "PayRuns");

        migrationBuilder.DropColumn(
            name: "CompanyId",
            table: "PayRuns");

        migrationBuilder.DropColumn(
            name: "CostCenterId",
            table: "PayRuns");

        migrationBuilder.DropColumn(
            name: "IsConsolidated",
            table: "PayRuns");

        migrationBuilder.DropColumn(
            name: "BranchId",
            table: "Employees");

        migrationBuilder.DropColumn(
            name: "CompanyId",
            table: "Employees");

        migrationBuilder.DropColumn(
            name: "CostCenterId",
            table: "Employees");
    }
}
