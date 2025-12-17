using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payroll.Infrastructure.Persistence;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PayrollDbContext))]
[Migration("20251125001500_AddEmployeeRecurringPayItems")]
public partial class AddEmployeeRecurringPayItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EmployeeRecurringPayItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayItemKind = table.Column<int>(type: "int", nullable: false),
                AllowanceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeductionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                Percentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmployeeRecurringPayItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmployeeRecurringPayItems_AllowanceTypes_AllowanceTypeId",
                    column: x => x.AllowanceTypeId,
                    principalTable: "AllowanceTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_EmployeeRecurringPayItems_DeductionTypes_DeductionTypeId",
                    column: x => x.DeductionTypeId,
                    principalTable: "DeductionTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_EmployeeRecurringPayItems_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.AddCheckConstraint(
            name: "CK_EmployeeRecurringPayItems_AmountOrPercentage",
            table: "EmployeeRecurringPayItems",
            sql: "((Amount IS NOT NULL AND Percentage IS NULL) OR (Amount IS NULL AND Percentage IS NOT NULL))");

        migrationBuilder.AddCheckConstraint(
            name: "CK_EmployeeRecurringPayItems_PositiveValues",
            table: "EmployeeRecurringPayItems",
            sql: "((Amount IS NULL OR Amount > 0) AND (Percentage IS NULL OR Percentage > 0))");

        migrationBuilder.AddCheckConstraint(
            name: "CK_EmployeeRecurringPayItems_EffectiveDates",
            table: "EmployeeRecurringPayItems",
            sql: "([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])");

        migrationBuilder.AddCheckConstraint(
            name: "CK_EmployeeRecurringPayItems_KindAndType",
            table: "EmployeeRecurringPayItems",
            sql: "((PayItemKind = 1 AND AllowanceTypeId IS NOT NULL AND DeductionTypeId IS NULL) OR (PayItemKind = 2 AND DeductionTypeId IS NOT NULL AND AllowanceTypeId IS NULL))");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeRecurringPayItems_AllowanceTypeId",
            table: "EmployeeRecurringPayItems",
            column: "AllowanceTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeRecurringPayItems_DeductionTypeId",
            table: "EmployeeRecurringPayItems",
            column: "DeductionTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeRecurringPayItems_EmployeeId",
            table: "EmployeeRecurringPayItems",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeRecurringPayItems_EmployeeId_PayItemKind_AllowanceTypeId_DeductionTypeId_EffectiveFrom",
            table: "EmployeeRecurringPayItems",
            columns: new[] { "EmployeeId", "PayItemKind", "AllowanceTypeId", "DeductionTypeId", "EffectiveFrom" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EmployeeRecurringPayItems");
    }
}
