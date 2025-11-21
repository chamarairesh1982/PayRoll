using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddEpfEtfAndTaxRuleSets : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EpfEtfRuleSets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                EmployeeEpfRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                EmployerEpfRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                EmployerEtfRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                MinimumWageForEpf = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                MaximumEarningForEpf = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                MaximumEarningForEtf = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EpfEtfRuleSets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TaxRuleSets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                YearOfAssessment = table.Column<int>(type: "int", nullable: false),
                EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaxRuleSets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TaxSlabs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TaxRuleSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FromAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ToAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                RatePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaxSlabs", x => x.Id);
                table.ForeignKey(
                    name: "FK_TaxSlabs_TaxRuleSets_TaxRuleSetId",
                    column: x => x.TaxRuleSetId,
                    principalTable: "TaxRuleSets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EpfEtfRuleSets_EffectiveFrom_IsActive_IsDefault",
            table: "EpfEtfRuleSets",
            columns: new[] { "EffectiveFrom", "IsActive", "IsDefault" });

        migrationBuilder.CreateIndex(
            name: "IX_TaxRuleSets_YearOfAssessment_IsActive_IsDefault",
            table: "TaxRuleSets",
            columns: new[] { "YearOfAssessment", "IsActive", "IsDefault" });

        migrationBuilder.CreateIndex(
            name: "IX_TaxSlabs_TaxRuleSetId",
            table: "TaxSlabs",
            column: "TaxRuleSetId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EpfEtfRuleSets");

        migrationBuilder.DropTable(
            name: "TaxSlabs");

        migrationBuilder.DropTable(
            name: "TaxRuleSets");
    }
}
