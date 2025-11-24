using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 641, DateTimeKind.Utc).AddTicks(3833));

            migrationBuilder.UpdateData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 641, DateTimeKind.Utc).AddTicks(5485));

            migrationBuilder.UpdateData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 641, DateTimeKind.Utc).AddTicks(5496));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 675, DateTimeKind.Utc).AddTicks(5172));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 675, DateTimeKind.Utc).AddTicks(6021));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 675, DateTimeKind.Utc).AddTicks(6028));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 675, DateTimeKind.Utc).AddTicks(6031));

            migrationBuilder.UpdateData(
                table: "EpfEtfRuleSets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 683, DateTimeKind.Utc).AddTicks(14));

            migrationBuilder.UpdateData(
                table: "TaxRuleSets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 700, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 702, DateTimeKind.Utc).AddTicks(6332));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 702, DateTimeKind.Utc).AddTicks(7549));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 702, DateTimeKind.Utc).AddTicks(7583));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 53, 10, 702, DateTimeKind.Utc).AddTicks(7585));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 11, DateTimeKind.Utc).AddTicks(8895));

            migrationBuilder.UpdateData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 12, DateTimeKind.Utc).AddTicks(587));

            migrationBuilder.UpdateData(
                table: "AllowanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 12, DateTimeKind.Utc).AddTicks(597));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(7179));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(8039));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(8045));

            migrationBuilder.UpdateData(
                table: "DeductionTypes",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 55, DateTimeKind.Utc).AddTicks(8048));

            migrationBuilder.UpdateData(
                table: "EpfEtfRuleSets",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 63, DateTimeKind.Utc).AddTicks(7323));

            migrationBuilder.UpdateData(
                table: "TaxRuleSets",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 82, DateTimeKind.Utc).AddTicks(2664));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(7601));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa2-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(9080));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa3-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(9090));

            migrationBuilder.UpdateData(
                table: "TaxSlabs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa4-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 24, 18, 24, 11, 84, DateTimeKind.Utc).AddTicks(9093));
        }
    }
}
