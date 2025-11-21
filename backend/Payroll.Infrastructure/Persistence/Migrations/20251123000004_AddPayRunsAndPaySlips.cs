using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddPayRunsAndPaySlips : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PayRuns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                PeriodType = table.Column<int>(type: "int", nullable: false),
                PeriodStart = table.Column<DateTime>(type: "date", nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "date", nullable: false),
                PayDate = table.Column<DateTime>(type: "date", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                IsLocked = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PayRuns", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PaySlips",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PayRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                TotalEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                EmployeeEpf = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                EmployerEpf = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                EmployerEtf = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                PayeTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaySlips", x => x.Id);
                table.ForeignKey(
                    name: "FK_PaySlips_PayRuns_PayRunId",
                    column: x => x.PayRunId,
                    principalTable: "PayRuns",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PaySlipDeductionLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaySlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                IsPreTax = table.Column<bool>(type: "bit", nullable: false),
                IsPostTax = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaySlipDeductionLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_PaySlipDeductionLines_PaySlips_PaySlipId",
                    column: x => x.PaySlipId,
                    principalTable: "PaySlips",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PaySlipEarningLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PaySlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                IsEpfApplicable = table.Column<bool>(type: "bit", nullable: false),
                IsEtfApplicable = table.Column<bool>(type: "bit", nullable: false),
                IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaySlipEarningLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_PaySlipEarningLines_PaySlips_PaySlipId",
                    column: x => x.PaySlipId,
                    principalTable: "PaySlips",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PayRuns_Code",
            table: "PayRuns",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PaySlipDeductionLines_PaySlipId",
            table: "PaySlipDeductionLines",
            column: "PaySlipId");

        migrationBuilder.CreateIndex(
            name: "IX_PaySlipEarningLines_PaySlipId",
            table: "PaySlipEarningLines",
            column: "PaySlipId");

        migrationBuilder.CreateIndex(
            name: "IX_PaySlips_PayRunId_EmployeeId",
            table: "PaySlips",
            columns: new[] { "PayRunId", "EmployeeId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PaySlipDeductionLines");

        migrationBuilder.DropTable(
            name: "PaySlipEarningLines");

        migrationBuilder.DropTable(
            name: "PaySlips");

        migrationBuilder.DropTable(
            name: "PayRuns");
    }
}
