using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddEmployeesTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Initials = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                CallingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                NicNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                Gender = table.Column<int>(type: "int", nullable: false),
                MaritalStatus = table.Column<int>(type: "int", nullable: false),
                EmploymentStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ProbationEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ConfirmationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Employees_EmployeeCode",
            table: "Employees",
            column: "EmployeeCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Employees_NicNumber",
            table: "Employees",
            column: "NicNumber",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Employees");
    }
}
