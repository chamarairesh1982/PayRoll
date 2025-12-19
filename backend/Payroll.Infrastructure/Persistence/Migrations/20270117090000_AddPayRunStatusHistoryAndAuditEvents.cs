using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payroll.Infrastructure.Persistence.Migrations;

public partial class AddPayRunStatusHistoryAndAuditEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PayRunApprovals_PayRuns_PayRunId",
            table: "PayRunApprovals");

        migrationBuilder.RenameTable(
            name: "PayRunApprovals",
            newName: "PayRunStatusHistory");

        migrationBuilder.RenameIndex(
            name: "IX_PayRunApprovals_PayRunId",
            table: "PayRunStatusHistory",
            newName: "IX_PayRunStatusHistory_PayRunId");

        migrationBuilder.RenameColumn(
            name: "ActorUserName",
            table: "PayRunStatusHistory",
            newName: "ActorDisplayName");

        migrationBuilder.RenameColumn(
            name: "ActionedAt",
            table: "PayRunStatusHistory",
            newName: "TimestampUtc");

        migrationBuilder.AddColumn<string>(
            name: "PreviousHash",
            table: "PayRunStatusHistory",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Hash",
            table: "PayRunStatusHistory",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddForeignKey(
            name: "FK_PayRunStatusHistory_PayRuns_PayRunId",
            table: "PayRunStatusHistory",
            column: "PayRunId",
            principalTable: "PayRuns",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.RenameTable(
            name: "AuditLogs",
            newName: "AuditEvents");

        migrationBuilder.RenameColumn(
            name: "EntityName",
            table: "AuditEvents",
            newName: "EntityType");

        migrationBuilder.RenameColumn(
            name: "BeforeSnapshot",
            table: "AuditEvents",
            newName: "BeforeJson");

        migrationBuilder.RenameColumn(
            name: "AfterSnapshot",
            table: "AuditEvents",
            newName: "AfterJson");

        migrationBuilder.RenameColumn(
            name: "CreatedAt",
            table: "AuditEvents",
            newName: "TimestampUtc");

        migrationBuilder.RenameColumn(
            name: "CreatedBy",
            table: "AuditEvents",
            newName: "ActorDisplayName");

        migrationBuilder.DropIndex(
            name: "IX_AuditLogs_Action",
            table: "AuditEvents");

        migrationBuilder.DropIndex(
            name: "IX_AuditLogs_CreatedAt",
            table: "AuditEvents");

        migrationBuilder.DropIndex(
            name: "IX_AuditLogs_CreatedBy",
            table: "AuditEvents");

        migrationBuilder.DropIndex(
            name: "IX_AuditLogs_EntityName_EntityId",
            table: "AuditEvents");

        migrationBuilder.AlterColumn<string>(
            name: "EntityType",
            table: "AuditEvents",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "EntityId",
            table: "AuditEvents",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Action",
            table: "AuditEvents",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "ActorDisplayName",
            table: "AuditEvents",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddColumn<string>(
            name: "ActorUserId",
            table: "AuditEvents",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "system");

        migrationBuilder.AddColumn<string>(
            name: "CorrelationId",
            table: "AuditEvents",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PreviousHash",
            table: "AuditEvents",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Hash",
            table: "AuditEvents",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_Action",
            table: "AuditEvents",
            column: "Action");

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_TimestampUtc",
            table: "AuditEvents",
            column: "TimestampUtc");

        migrationBuilder.CreateIndex(
            name: "IX_AuditEvents_EntityType_EntityId",
            table: "AuditEvents",
            columns: new[] { "EntityType", "EntityId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AuditEvents_Action",
            table: "AuditEvents");

        migrationBuilder.DropIndex(
            name: "IX_AuditEvents_TimestampUtc",
            table: "AuditEvents");

        migrationBuilder.DropIndex(
            name: "IX_AuditEvents_EntityType_EntityId",
            table: "AuditEvents");

        migrationBuilder.DropColumn(
            name: "ActorUserId",
            table: "AuditEvents");

        migrationBuilder.DropColumn(
            name: "CorrelationId",
            table: "AuditEvents");

        migrationBuilder.DropColumn(
            name: "PreviousHash",
            table: "AuditEvents");

        migrationBuilder.DropColumn(
            name: "Hash",
            table: "AuditEvents");

        migrationBuilder.AlterColumn<string>(
            name: "EntityType",
            table: "AuditEvents",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "EntityId",
            table: "AuditEvents",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Action",
            table: "AuditEvents",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "ActorDisplayName",
            table: "AuditEvents",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldNullable: true);

        migrationBuilder.RenameColumn(
            name: "EntityType",
            table: "AuditEvents",
            newName: "EntityName");

        migrationBuilder.RenameColumn(
            name: "BeforeJson",
            table: "AuditEvents",
            newName: "BeforeSnapshot");

        migrationBuilder.RenameColumn(
            name: "AfterJson",
            table: "AuditEvents",
            newName: "AfterSnapshot");

        migrationBuilder.RenameColumn(
            name: "TimestampUtc",
            table: "AuditEvents",
            newName: "CreatedAt");

        migrationBuilder.RenameColumn(
            name: "ActorDisplayName",
            table: "AuditEvents",
            newName: "CreatedBy");

        migrationBuilder.RenameTable(
            name: "AuditEvents",
            newName: "AuditLogs");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Action",
            table: "AuditLogs",
            column: "Action");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_CreatedAt",
            table: "AuditLogs",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_CreatedBy",
            table: "AuditLogs",
            column: "CreatedBy");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_EntityName_EntityId",
            table: "AuditLogs",
            columns: new[] { "EntityName", "EntityId" });

        migrationBuilder.DropForeignKey(
            name: "FK_PayRunStatusHistory_PayRuns_PayRunId",
            table: "PayRunStatusHistory");

        migrationBuilder.DropColumn(
            name: "PreviousHash",
            table: "PayRunStatusHistory");

        migrationBuilder.DropColumn(
            name: "Hash",
            table: "PayRunStatusHistory");

        migrationBuilder.RenameColumn(
            name: "ActorDisplayName",
            table: "PayRunStatusHistory",
            newName: "ActorUserName");

        migrationBuilder.RenameColumn(
            name: "TimestampUtc",
            table: "PayRunStatusHistory",
            newName: "ActionedAt");

        migrationBuilder.RenameIndex(
            name: "IX_PayRunStatusHistory_PayRunId",
            table: "PayRunStatusHistory",
            newName: "IX_PayRunApprovals_PayRunId");

        migrationBuilder.RenameTable(
            name: "PayRunStatusHistory",
            newName: "PayRunApprovals");

        migrationBuilder.AddForeignKey(
            name: "FK_PayRunApprovals_PayRuns_PayRunId",
            table: "PayRunApprovals",
            column: "PayRunId",
            principalTable: "PayRuns",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
