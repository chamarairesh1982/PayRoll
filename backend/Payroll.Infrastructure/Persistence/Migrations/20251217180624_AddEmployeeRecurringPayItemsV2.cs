using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeRecurringPayItemsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BeforeSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AfterSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayRunApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    ActionedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActionedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayRunApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayRunApprovals_PayRuns_PayRunId",
                        column: x => x.PayRunId,
                        principalTable: "PayRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayRunApprovals_PayRunId",
                table: "PayRunApprovals",
                column: "PayRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecurringPayItems_AllowanceTypes_AllowanceTypeId",
                table: "EmployeeRecurringPayItems",
                column: "AllowanceTypeId",
                principalTable: "AllowanceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecurringPayItems_DeductionTypes_DeductionTypeId",
                table: "EmployeeRecurringPayItems",
                column: "DeductionTypeId",
                principalTable: "DeductionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRecurringPayItems_Employees_EmployeeId",
                table: "EmployeeRecurringPayItems",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecurringPayItems_AllowanceTypes_AllowanceTypeId",
                table: "EmployeeRecurringPayItems");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecurringPayItems_DeductionTypes_DeductionTypeId",
                table: "EmployeeRecurringPayItems");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRecurringPayItems_Employees_EmployeeId",
                table: "EmployeeRecurringPayItems");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "PayRunApprovals");
        }
    }
}
