using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M205_CompleteEnergyCapture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediumName",
                schema: "core",
                table: "EnergyMeters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Electricity");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentMeterId",
                schema: "core",
                table: "EnergyMeters",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CapturedByUserId",
                schema: "core",
                table: "EnergyReadings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaptureContext",
                schema: "core",
                table: "EnergyReadings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CapturedOfflineAtUtc",
                schema: "core",
                table: "EnergyReadings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfflineCaptureId",
                schema: "core",
                table: "EnergyReadings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlausibilityStatus",
                schema: "core",
                table: "EnergyReadings",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "PlausibilityNote",
                schema: "core",
                table: "EnergyReadings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RfidExceptionReason",
                schema: "core",
                table: "EnergyReadings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RfidTag",
                schema: "core",
                table: "EnergyReadings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedAtUtc",
                schema: "core",
                table: "EnergyReadings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnergyMeters_ParentMeterId",
                schema: "core",
                table: "EnergyMeters",
                column: "ParentMeterId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyReadings_OfflineCaptureId",
                schema: "core",
                table: "EnergyReadings",
                column: "OfflineCaptureId",
                unique: true,
                filter: "\"OfflineCaptureId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EnergyMeters_EnergyMeters_ParentMeterId",
                schema: "core",
                table: "EnergyMeters",
                column: "ParentMeterId",
                principalSchema: "core",
                principalTable: "EnergyMeters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnergyMeters_EnergyMeters_ParentMeterId",
                schema: "core",
                table: "EnergyMeters");

            migrationBuilder.DropIndex(
                name: "IX_EnergyMeters_ParentMeterId",
                schema: "core",
                table: "EnergyMeters");

            migrationBuilder.DropIndex(
                name: "IX_EnergyReadings_OfflineCaptureId",
                schema: "core",
                table: "EnergyReadings");

            migrationBuilder.DropColumn(name: "MediumName", schema: "core", table: "EnergyMeters");
            migrationBuilder.DropColumn(name: "ParentMeterId", schema: "core", table: "EnergyMeters");
            migrationBuilder.DropColumn(name: "CapturedByUserId", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "CaptureContext", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "CapturedOfflineAtUtc", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "OfflineCaptureId", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "PlausibilityStatus", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "PlausibilityNote", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "RfidExceptionReason", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "RfidTag", schema: "core", table: "EnergyReadings");
            migrationBuilder.DropColumn(name: "SyncedAtUtc", schema: "core", table: "EnergyReadings");
        }
    }
}
