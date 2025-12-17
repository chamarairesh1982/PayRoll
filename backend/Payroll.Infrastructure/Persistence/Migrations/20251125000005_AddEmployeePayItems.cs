using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payroll.Infrastructure.Persistence;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PayrollDbContext))]
[Migration("20251125000005_AddEmployeePayItems")]
public partial class AddEmployeePayItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EmployeePayItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayItemType = table.Column<int>(type: "int", nullable: false),
                PayItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                table.PrimaryKey("PK_EmployeePayItems", x => x.Id);
            });

        migrationBuilder.AddCheckConstraint(
            name: "CK_EmployeePayItems_AmountOrPercentage",
            table: "EmployeePayItems",
            sql: "((Amount IS NOT NULL AND Percentage IS NULL) OR (Amount IS NULL AND Percentage IS NOT NULL))");

        migrationBuilder.AddCheckConstraint(
            name: "CK_EmployeePayItems_PositiveValues",
            table: "EmployeePayItems",
            sql: "((Amount IS NULL OR Amount > 0) AND (Percentage IS NULL OR Percentage > 0))");

        migrationBuilder.AddCheckConstraint(
            name: "CK_EmployeePayItems_EffectiveDates",
            table: "EmployeePayItems",
            sql: "([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeePayItems_EmployeeId_PayItemCode_PayItemType_EffectiveFrom",
            table: "EmployeePayItems",
            columns: new[] { "EmployeeId", "PayItemCode", "PayItemType", "EffectiveFrom" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EmployeePayItems");
    }
}
