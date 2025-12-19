using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceNoPaySettingsAndDeductionSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AttendanceHalfDayHours",
                table: "PayrollSettings",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 4m);

            migrationBuilder.AddColumn<int>(
                name: "NoPayCalculationBasis",
                table: "PayrollSettings",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "DeductionLines",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceHalfDayHours",
                table: "PayrollSettings");

            migrationBuilder.DropColumn(
                name: "NoPayCalculationBasis",
                table: "PayrollSettings");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "DeductionLines");
        }

        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            new PayrollDbContextModelSnapshot().BuildModel(modelBuilder);
        }
    }
}
