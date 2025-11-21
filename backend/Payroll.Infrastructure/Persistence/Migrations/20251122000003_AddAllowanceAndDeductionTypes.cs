using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddAllowanceAndDeductionTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AllowanceTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Basis = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_AllowanceTypes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DeductionTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Basis = table.Column<int>(type: "int", nullable: false),
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
                table.PrimaryKey("PK_DeductionTypes", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AllowanceTypes_Code",
            table: "AllowanceTypes",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DeductionTypes_Code",
            table: "DeductionTypes",
            column: "Code",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AllowanceTypes");

        migrationBuilder.DropTable(
            name: "DeductionTypes");
    }
}
