using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Payroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "OvertimeRecords",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    HoursWorked = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingPrincipal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PeriodType = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoanRepayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanRepayments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaySlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaySlips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaySlips_PayRuns_PayRunId",
                        column: x => x.PayRunId,
                        principalTable: "PayRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeductionLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaySlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPreTax = table.Column<bool>(type: "bit", nullable: false),
                    IsPostTax = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeductionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeductionLines_PaySlips_PaySlipId",
                        column: x => x.PaySlipId,
                        principalTable: "PaySlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EarningLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaySlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsEpfApplicable = table.Column<bool>(type: "bit", nullable: false),
                    IsEtfApplicable = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarningLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EarningLines_PaySlips_PaySlipId",
                        column: x => x.PaySlipId,
                        principalTable: "PaySlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AllowanceTypes",
                columns: new[] { "Id", "Basis", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "IsEpfApplicable", "IsEtfApplicable", "IsTaxable", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 1, "BASIC", new DateTime(2025, 11, 24, 18, 24, 11, 11, DateTimeKind.Utc).AddTicks(8895), "", null, true, true, true, true, null, null, "Basic Salary" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1, "TRA", new DateTime(2025, 11, 24, 18, 24, 11, 12, DateTimeKind.Utc).AddTicks(587), "", null, true, true, true, true, null, null, "Transport Allowance" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 1, "ATD", new DateTime(2025, 11, 24, 18, 24, 11, 12, DateTimeKind.Utc).AddTicks(597), "", null, true, true, true, true, null, null, "Attendance Allowance" }
                });

            migrationBuilder.InsertData(
                table: "DeductionTypes",
                columns: new[] { "Id", "Basis", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "IsPostTax", "IsPreTax", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), 1, "EPF_EE", new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(7179), "", null, true, false, true, null, null, "Employee EPF" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), 1, "LOAN", new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(8039), "", null, true, false, true, null, null, "Loan Installment" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), 1, "NOPAY", new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(8045), "", null, true, false, true, null, null, "No Pay Deduction" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), 1, "PAYE", new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(8048), "", null, true, true, false, null, null, "PAYE Tax" }
                });

            migrationBuilder.InsertData(
                table: "EpfEtfRuleSets",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "EffectiveFrom", "EffectiveTo", "EmployeeEpfRate", "EmployerEpfRate", "EmployerEtfRate", "IsActive", "IsDefault", "MaximumEarningForEpf", "MaximumEarningForEtf", "MinimumWageForEpf", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[] { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2025, 11, 24, 18, 24, 11, 63, DateTimeKind.Utc).AddTicks(7323), "", new DateOnly(2020, 1, 1), null, 8m, 12m, 3m, true, true, null, null, null, null, null, "Sri Lanka Default EPF/ETF" });

            migrationBuilder.InsertData(
                table: "TaxRuleSets",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDefault", "ModifiedAt", "ModifiedBy", "Name", "YearOfAssessment" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2025, 11, 24, 18, 24, 11, 82, DateTimeKind.Utc).AddTicks(2664), "", new DateOnly(2025, 4, 1), null, true, true, null, null, "Sri Lanka PAYE YA 2025/26", 2025 });

            migrationBuilder.InsertData(
                table: "TaxSlabs",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "FromAmount", "IsActive", "ModifiedAt", "ModifiedBy", "Order", "RatePercent", "TaxRuleSetId", "ToAmount" },
                values: new object[,]
                {
                    { new Guid("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(7601), "", 0m, true, null, null, 1, 0m, new Guid("99999999-9999-9999-9999-999999999999"), 100000m },
                    { new Guid("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(9080), "", 100000m, true, null, null, 2, 6m, new Guid("99999999-9999-9999-9999-999999999999"), 141667m },
                    { new Guid("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(9090), "", 141667m, true, null, null, 3, 12m, new Guid("99999999-9999-9999-9999-999999999999"), 183333m },
                    { new Guid("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4"), new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(9093), "", 183333m, true, null, null, 4, 18m, new Guid("99999999-9999-9999-9999-999999999999"), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeductionLines_PaySlipId",
                table: "DeductionLines",
                column: "PaySlipId");

            migrationBuilder.CreateIndex(
                name: "IX_EarningLines_PaySlipId",
                table: "EarningLines",
                column: "PaySlipId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRepayments_LoanId",
                table: "LoanRepayments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_EmployeeId_Status",
                table: "Loans",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PayRuns_Code",
                table: "PayRuns",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayRuns_Reference",
                table: "PayRuns",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaySlips_EmployeeId",
                table: "PaySlips",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaySlips_PayRunId",
                table: "PaySlips",
                column: "PayRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "DeductionLines");

            migrationBuilder.DropTable(
                name: "EarningLines");

            migrationBuilder.DropTable(
                name: "LoanRepayments");

            migrationBuilder.DropTable(
                name: "PaySlips");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "PayRuns");

            migrationBuilder.DeleteData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "EpfEtfRuleSets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "TaxRuleSets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "OvertimeRecords",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
