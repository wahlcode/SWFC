using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M202_AddMaintenanceOrderOperationalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "core",
                table: "MaintenanceOrders",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "MaintenancePlanId",
                schema: "core",
                table: "MaintenanceOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueAtUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "MaintenancePlanId",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "PlannedStartUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "PlannedEndUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "DueAtUtc",
                schema: "core",
                table: "MaintenanceOrders");
        }
    }
}
