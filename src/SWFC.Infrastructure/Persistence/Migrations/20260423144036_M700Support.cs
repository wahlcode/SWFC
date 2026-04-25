using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M700Support : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bugs",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Reproducibility = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRequests",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RequirementReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RoadmapReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Escalation = table.Column<string>(type: "text", nullable: false),
                    ReactionControl = table.Column<string>(type: "text", nullable: false),
                    NotificationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RuntimeReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeEntries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceLevels",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Priority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResponseTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ProcessingTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    UseForSupport = table.Column<bool>(type: "boolean", nullable: false),
                    UseForIncidentManagement = table.Column<bool>(type: "boolean", nullable: false),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportCases",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserRequest = table.Column<string>(type: "text", nullable: false),
                    ProblemDescription = table.Column<string>(type: "text", nullable: false),
                    TriggeredBugId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriggeredIncidentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportCases_Bugs_TriggeredBugId",
                        column: x => x.TriggeredBugId,
                        principalSchema: "core",
                        principalTable: "Bugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportCases_Incidents_TriggeredIncidentId",
                        column: x => x.TriggeredIncidentId,
                        principalSchema: "core",
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportCases_TriggeredBugId",
                schema: "core",
                table: "SupportCases",
                column: "TriggeredBugId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportCases_TriggeredIncidentId",
                schema: "core",
                table: "SupportCases",
                column: "TriggeredIncidentId");

            var createdAtUtc = new DateTime(2026, 4, 23, 14, 40, 36, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                schema: "core",
                table: "Permissions",
                columns: new[]
                {
                    "Id",
                    "Code",
                    "Name",
                    "Description",
                    "Module",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy"
                },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000700"), "support.read", "Support Read", "Read support data.", "M700", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000701"), "support.write", "Support Write", "Change support data.", "M700", true, createdAtUtc, "migration", null, null }
                });

            migrationBuilder.Sql("""
                INSERT INTO core."RolePermissions" (
                    "Id",
                    "RoleId",
                    "PermissionId",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy")
                SELECT gen_random_uuid(), r."Id", p."Id", true, NOW(), 'migration', NULL, NULL
                FROM core."Roles" r
                JOIN core."Permissions" p ON p."Code" IN ('support.read', 'support.write')
                WHERE r."Name" IN ('Admin', 'SuperAdmin')
                ON CONFLICT ("RoleId", "PermissionId") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM core."RolePermissions" rp
                USING core."Permissions" p
                WHERE rp."PermissionId" = p."Id"
                  AND p."Code" IN ('support.read', 'support.write');
                """);

            migrationBuilder.DeleteData(
                schema: "core",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000700"));

            migrationBuilder.DeleteData(
                schema: "core",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000701"));

            migrationBuilder.DropTable(
                name: "ChangeRequests",
                schema: "core");

            migrationBuilder.DropTable(
                name: "KnowledgeEntries",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ServiceLevels",
                schema: "core");

            migrationBuilder.DropTable(
                name: "SupportCases",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Bugs",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Incidents",
                schema: "core");
        }
    }
}
