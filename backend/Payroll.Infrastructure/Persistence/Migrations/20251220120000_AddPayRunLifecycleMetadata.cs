using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayRunLifecycleMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "PayRunApprovals",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "ActionedBy",
                table: "PayRunApprovals",
                newName: "ActorUserName");

            migrationBuilder.AddColumn<string>(
                name: "ActorUserId",
                table: "PayRunApprovals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "PayRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "PayRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserName",
                table: "PayRuns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "PayRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockedByUserId",
                table: "PayRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockedByUserName",
                table: "PayRuns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparedAt",
                table: "PayRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreparedByUserId",
                table: "PayRuns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreparedByUserName",
                table: "PayRuns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "PayRunApprovals");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserName",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "LockedByUserId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "LockedByUserName",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "PreparedAt",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "PreparedByUserId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "PreparedByUserName",
                table: "PayRuns");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "PayRunApprovals",
                newName: "Comments");

            migrationBuilder.RenameColumn(
                name: "ActorUserName",
                table: "PayRunApprovals",
                newName: "ActionedBy");
        }
    }
}
