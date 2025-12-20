using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddTaxSlabSetsAndTaxBreakdown : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "RatePercent",
            table: "TaxSlabs",
            newName: "Rate");

        migrationBuilder.AlterColumn<decimal>(
            name: "Rate",
            table: "TaxSlabs",
            type: "decimal(5,4)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(5,2)");

        migrationBuilder.AddColumn<int>(
            name: "Frequency",
            table: "TaxRuleSets",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<string>(
            name: "TaxCalculationJson",
            table: "PaySlips",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "EmployeeTaxProfiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsTaxExempt = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                SlabSetOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmployeeTaxProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmployeeTaxProfiles_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EmployeeTaxProfiles_TaxRuleSets_SlabSetOverrideId",
                    column: x => x.SlabSetOverrideId,
                    principalTable: "TaxRuleSets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeTaxProfiles_EmployeeId",
            table: "EmployeeTaxProfiles",
            column: "EmployeeId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeTaxProfiles_SlabSetOverrideId",
            table: "EmployeeTaxProfiles",
            column: "SlabSetOverrideId");

        migrationBuilder.UpdateData(
            table: "TaxSlabs",
            keyColumn: "Id",
            keyValue: new Guid("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
            column: "Rate",
            value: 0.06m);

        migrationBuilder.UpdateData(
            table: "TaxSlabs",
            keyColumn: "Id",
            keyValue: new Guid("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
            column: "Rate",
            value: 0.12m);

        migrationBuilder.UpdateData(
            table: "TaxSlabs",
            keyColumn: "Id",
            keyValue: new Guid("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
            column: "Rate",
            value: 0.18m);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EmployeeTaxProfiles");

        migrationBuilder.DropColumn(
            name: "TaxCalculationJson",
            table: "PaySlips");

        migrationBuilder.DropColumn(
            name: "Frequency",
            table: "TaxRuleSets");

        migrationBuilder.AlterColumn<decimal>(
            name: "Rate",
            table: "TaxSlabs",
            type: "decimal(5,2)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(5,4)");

        migrationBuilder.RenameColumn(
            name: "Rate",
            table: "TaxSlabs",
            newName: "RatePercent");

        migrationBuilder.UpdateData(
            table: "TaxSlabs",
            keyColumn: "Id",
            keyValue: new Guid("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
            column: "RatePercent",
            value: 6m);

        migrationBuilder.UpdateData(
            table: "TaxSlabs",
            keyColumn: "Id",
            keyValue: new Guid("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
            column: "RatePercent",
            value: 12m);

        migrationBuilder.UpdateData(
            table: "TaxSlabs",
            keyColumn: "Id",
            keyValue: new Guid("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
            column: "RatePercent",
            value: 18m);
    }
}
