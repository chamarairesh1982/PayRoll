using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations
{
    public partial class AddRulePackages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EpfRuleVersionId",
                table: "PayRuns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EtfRuleVersionId",
                table: "PayRuns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RulesSnapshotJson",
                table: "PayRuns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxRuleVersionId",
                table: "PayRuns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RulePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RulePackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RulePackageVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RulePackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RulePackageVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RulePackageVersions_RulePackages_RulePackageId",
                        column: x => x.RulePackageId,
                        principalTable: "RulePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.CheckConstraint("CK_RulePackageVersions_EffectiveDates", "([EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom])");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayRuns_EpfRuleVersionId",
                table: "PayRuns",
                column: "EpfRuleVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PayRuns_EtfRuleVersionId",
                table: "PayRuns",
                column: "EtfRuleVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PayRuns_TaxRuleVersionId",
                table: "PayRuns",
                column: "TaxRuleVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RulePackages_CompanyId_RuleType_Name",
                table: "RulePackages",
                columns: new[] { "CompanyId", "RuleType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RulePackageVersions_RulePackageId_EffectiveFrom",
                table: "RulePackageVersions",
                columns: new[] { "RulePackageId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_RulePackageVersions_RulePackageId_VersionNumber",
                table: "RulePackageVersions",
                columns: new[] { "RulePackageId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PayRuns_RulePackageVersions_EpfRuleVersionId",
                table: "PayRuns",
                column: "EpfRuleVersionId",
                principalTable: "RulePackageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayRuns_RulePackageVersions_EtfRuleVersionId",
                table: "PayRuns",
                column: "EtfRuleVersionId",
                principalTable: "RulePackageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayRuns_RulePackageVersions_TaxRuleVersionId",
                table: "PayRuns",
                column: "TaxRuleVersionId",
                principalTable: "RulePackageVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayRuns_RulePackageVersions_EpfRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_PayRuns_RulePackageVersions_EtfRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_PayRuns_RulePackageVersions_TaxRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropTable(
                name: "RulePackageVersions");

            migrationBuilder.DropTable(
                name: "RulePackages");

            migrationBuilder.DropIndex(
                name: "IX_PayRuns_EpfRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropIndex(
                name: "IX_PayRuns_EtfRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropIndex(
                name: "IX_PayRuns_TaxRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "EpfRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "EtfRuleVersionId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "RulesSnapshotJson",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "TaxRuleVersionId",
                table: "PayRuns");
        }
    }
}
