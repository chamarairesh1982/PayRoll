using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
        public partial class AddPayRunIdToOvertimeRecords : Migration
        {
            /// <inheritdoc />
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.AddColumn<Guid>(
                    name: "PayRunId",
                    table: "OvertimeRecords",
                    type: "uniqueidentifier",
                    nullable: true);
            }

            /// <inheritdoc />
            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropColumn(
                    name: "PayRunId",
                    table: "OvertimeRecords");
            }
        }
}
