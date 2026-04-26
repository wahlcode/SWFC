using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M203_M207_M208_OperationalControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionPlans",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    NextDueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityCases",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintenanceOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InspectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RootCause = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafetyAssessments",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Activity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Hazard = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Likelihood = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    RequiredMeasures = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QualityCaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyAssessments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inspections",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InspectionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InspectorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FollowUpReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PerformedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_InspectionPlans_InspectionPlanId",
                        column: x => x.InspectionPlanId,
                        principalSchema: "core",
                        principalTable: "InspectionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityActions",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QualityCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityActions_QualityCases_QualityCaseId",
                        column: x => x.QualityCaseId,
                        principalSchema: "core",
                        principalTable: "QualityCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SafetyPermits",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValidUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Restriction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyPermits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyPermits_SafetyAssessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "core",
                        principalTable: "SafetyAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_InspectionPlans_TargetType_TargetId", schema: "core", table: "InspectionPlans", columns: new[] { "TargetType", "TargetId" });
            migrationBuilder.CreateIndex(name: "IX_Inspections_InspectionPlanId", schema: "core", table: "Inspections", column: "InspectionPlanId");
            migrationBuilder.CreateIndex(name: "IX_Inspections_TargetType_TargetId", schema: "core", table: "Inspections", columns: new[] { "TargetType", "TargetId" });
            migrationBuilder.CreateIndex(name: "IX_QualityActions_QualityCaseId", schema: "core", table: "QualityActions", column: "QualityCaseId");
            migrationBuilder.CreateIndex(name: "IX_QualityCases_InspectionId", schema: "core", table: "QualityCases", column: "InspectionId");
            migrationBuilder.CreateIndex(name: "IX_QualityCases_Status", schema: "core", table: "QualityCases", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_SafetyAssessments_QualityCaseId", schema: "core", table: "SafetyAssessments", column: "QualityCaseId");
            migrationBuilder.CreateIndex(name: "IX_SafetyAssessments_TargetType_TargetId", schema: "core", table: "SafetyAssessments", columns: new[] { "TargetType", "TargetId" });
            migrationBuilder.CreateIndex(name: "IX_SafetyPermits_AssessmentId", schema: "core", table: "SafetyPermits", column: "AssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Inspections", schema: "core");
            migrationBuilder.DropTable(name: "QualityActions", schema: "core");
            migrationBuilder.DropTable(name: "SafetyPermits", schema: "core");
            migrationBuilder.DropTable(name: "InspectionPlans", schema: "core");
            migrationBuilder.DropTable(name: "QualityCases", schema: "core");
            migrationBuilder.DropTable(name: "SafetyAssessments", schema: "core");
        }
    }
}
