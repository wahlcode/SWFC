using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'MachineComponents'
          AND column_name = 'AssetType'
    ) THEN
        ALTER TABLE core."MachineComponents" DROP COLUMN "AssetType";
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'core'
          AND table_name = 'MachineComponents'
          AND column_name = 'EnergyObjectId'
    ) THEN
        ALTER TABLE core."MachineComponents" DROP COLUMN "EnergyObjectId";
    END IF;
END $$;
""");

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

            migrationBuilder.AddColumn<Guid>(
                name: "MaintenancePlanId",
                schema: "core",
                table: "MaintenanceOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "core",
                table: "MaintenanceOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                schema: "core",
                table: "MaintenanceOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssetType",
                schema: "core",
                table: "Machines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EnergyObjectId",
                schema: "core",
                table: "Machines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "StandardUnitPrice",
                schema: "core",
                table: "InventoryItems",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "DueAtUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "MaintenancePlanId",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "PlannedEndUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "PlannedStartUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                schema: "core",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "AssetType",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "EnergyObjectId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "StandardUnitPrice",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.AddColumn<string>(
                name: "AssetType",
                schema: "core",
                table: "MachineComponents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EnergyObjectId",
                schema: "core",
                table: "MachineComponents",
                type: "uuid",
                nullable: true);
        }
    }
}
