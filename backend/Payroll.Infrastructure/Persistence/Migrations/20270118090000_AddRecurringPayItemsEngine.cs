using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payroll.Infrastructure.Persistence;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PayrollDbContext))]
[Migration("20270118090000_AddRecurringPayItemsEngine")]
public partial class AddRecurringPayItemsEngine : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RecurringPayItemRules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                RuleType = table.Column<int>(type: "int", nullable: false),
                AllowanceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeductionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Frequency = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                Taxable = table.Column<bool>(type: "bit", nullable: false),
                EpfEtfContributable = table.Column<bool>(type: "bit", nullable: false),
                Prorate = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecurringPayItemRules", x => x.Id);
                table.ForeignKey(
                    name: "FK_RecurringPayItemRules_AllowanceTypes_AllowanceTypeId",
                    column: x => x.AllowanceTypeId,
                    principalTable: "AllowanceTypes",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_RecurringPayItemRules_DeductionTypes_DeductionTypeId",
                    column: x => x.DeductionTypeId,
                    principalTable: "DeductionTypes",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "RecurringPayItemAssignments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecurringPayItemAssignments", x => x.Id);
                table.ForeignKey(
                    name: "FK_RecurringPayItemAssignments_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RecurringPayItemAssignments_RecurringPayItemRules_RuleId",
                    column: x => x.RuleId,
                    principalTable: "RecurringPayItemRules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PayRunRecurringLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaySlipLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LineType = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayRunRecurringLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_PayRunRecurringLines_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PayRunRecurringLines_PayRuns_PayRunId",
                    column: x => x.PayRunId,
                    principalTable: "PayRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PayRunRecurringLines_RecurringPayItemRules_RuleId",
                    column: x => x.RuleId,
                    principalTable: "RecurringPayItemRules",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.AddCheckConstraint(
            name: "CK_RecurringPayItemAssignments_EffectiveDates",
            table: "RecurringPayItemAssignments",
            sql: "([EndDate] IS NULL OR [EndDate] >= [StartDate])");

        migrationBuilder.AddCheckConstraint(
            name: "CK_RecurringPayItemRules_ComponentType",
            table: "RecurringPayItemRules",
            sql: "((RuleType = 1 AND AllowanceTypeId IS NOT NULL AND DeductionTypeId IS NULL) OR (RuleType = 2 AND DeductionTypeId IS NOT NULL AND AllowanceTypeId IS NULL))");

        migrationBuilder.AddCheckConstraint(
            name: "CK_RecurringPayItemRules_EffectiveDates",
            table: "RecurringPayItemRules",
            sql: "([EndDate] IS NULL OR [EndDate] >= [StartDate])");

        migrationBuilder.CreateIndex(
            name: "IX_PayRunRecurringLines_EmployeeId",
            table: "PayRunRecurringLines",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_PayRunRecurringLines_PayRunId",
            table: "PayRunRecurringLines",
            column: "PayRunId");

        migrationBuilder.CreateIndex(
            name: "IX_PayRunRecurringLines_RuleId",
            table: "PayRunRecurringLines",
            column: "RuleId");

        migrationBuilder.CreateIndex(
            name: "IX_PayRunRecurringLines_PayRunId_EmployeeId_RuleId",
            table: "PayRunRecurringLines",
            columns: new[] { "PayRunId", "EmployeeId", "RuleId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RecurringPayItemAssignments_EmployeeId",
            table: "RecurringPayItemAssignments",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_RecurringPayItemAssignments_RuleId",
            table: "RecurringPayItemAssignments",
            column: "RuleId");

        migrationBuilder.CreateIndex(
            name: "IX_RecurringPayItemAssignments_RuleId_EmployeeId",
            table: "RecurringPayItemAssignments",
            columns: new[] { "RuleId", "EmployeeId" });

        migrationBuilder.CreateIndex(
            name: "IX_RecurringPayItemRules_AllowanceTypeId",
            table: "RecurringPayItemRules",
            column: "AllowanceTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_RecurringPayItemRules_DeductionTypeId",
            table: "RecurringPayItemRules",
            column: "DeductionTypeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PayRunRecurringLines");

        migrationBuilder.DropTable(
            name: "RecurringPayItemAssignments");

        migrationBuilder.DropTable(
            name: "RecurringPayItemRules");
    }
}
