using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M805AuditMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                schema: "core",
                table: "AuditLogs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientInfo",
                schema: "core",
                table: "AuditLogs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientIp",
                schema: "core",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Module",
                schema: "core",
                table: "AuditLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "core",
                table: "AuditLogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUserId",
                schema: "core",
                table: "AuditLogs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                schema: "core",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TargetUserId",
                schema: "core",
                table: "AuditLogs",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TimestampUtc",
                schema: "core",
                table: "AuditLogs",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "core",
                table: "AuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TargetUserId",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TimestampUtc",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ClientInfo",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ClientIp",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Module",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "core",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                schema: "core",
                table: "AuditLogs");
        }
    }
}
