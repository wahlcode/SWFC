using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M700SupportTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HistoryLog",
                schema: "core",
                table: "SupportCases",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModuleReference",
                schema: "core",
                table: "SupportCases",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectReference",
                schema: "core",
                table: "SupportCases",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "SupportCases",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HistoryLog",
                schema: "core",
                table: "ServiceLevels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModuleReference",
                schema: "core",
                table: "ServiceLevels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectReference",
                schema: "core",
                table: "ServiceLevels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "ServiceLevels",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HistoryLog",
                schema: "core",
                table: "KnowledgeEntries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModuleReference",
                schema: "core",
                table: "KnowledgeEntries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectReference",
                schema: "core",
                table: "KnowledgeEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "KnowledgeEntries",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HistoryLog",
                schema: "core",
                table: "Incidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModuleReference",
                schema: "core",
                table: "Incidents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectReference",
                schema: "core",
                table: "Incidents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "Incidents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HistoryLog",
                schema: "core",
                table: "ChangeRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModuleReference",
                schema: "core",
                table: "ChangeRequests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectReference",
                schema: "core",
                table: "ChangeRequests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "ChangeRequests",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HistoryLog",
                schema: "core",
                table: "Bugs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModuleReference",
                schema: "core",
                table: "Bugs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectReference",
                schema: "core",
                table: "Bugs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HistoryLog",
                schema: "core",
                table: "SupportCases");

            migrationBuilder.DropColumn(
                name: "ModuleReference",
                schema: "core",
                table: "SupportCases");

            migrationBuilder.DropColumn(
                name: "ObjectReference",
                schema: "core",
                table: "SupportCases");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "SupportCases");

            migrationBuilder.DropColumn(
                name: "HistoryLog",
                schema: "core",
                table: "ServiceLevels");

            migrationBuilder.DropColumn(
                name: "ModuleReference",
                schema: "core",
                table: "ServiceLevels");

            migrationBuilder.DropColumn(
                name: "ObjectReference",
                schema: "core",
                table: "ServiceLevels");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "ServiceLevels");

            migrationBuilder.DropColumn(
                name: "HistoryLog",
                schema: "core",
                table: "KnowledgeEntries");

            migrationBuilder.DropColumn(
                name: "ModuleReference",
                schema: "core",
                table: "KnowledgeEntries");

            migrationBuilder.DropColumn(
                name: "ObjectReference",
                schema: "core",
                table: "KnowledgeEntries");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "KnowledgeEntries");

            migrationBuilder.DropColumn(
                name: "HistoryLog",
                schema: "core",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ModuleReference",
                schema: "core",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ObjectReference",
                schema: "core",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "HistoryLog",
                schema: "core",
                table: "ChangeRequests");

            migrationBuilder.DropColumn(
                name: "ModuleReference",
                schema: "core",
                table: "ChangeRequests");

            migrationBuilder.DropColumn(
                name: "ObjectReference",
                schema: "core",
                table: "ChangeRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "ChangeRequests");

            migrationBuilder.DropColumn(
                name: "HistoryLog",
                schema: "core",
                table: "Bugs");

            migrationBuilder.DropColumn(
                name: "ModuleReference",
                schema: "core",
                table: "Bugs");

            migrationBuilder.DropColumn(
                name: "ObjectReference",
                schema: "core",
                table: "Bugs");
        }
    }
}
