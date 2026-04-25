using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateM205EnergyMeterModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "core",
                table: "EnergyMeters");

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalImportEnabled",
                schema: "core",
                table: "EnergyMeters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualEntryEnabled",
                schema: "core",
                table: "EnergyMeters",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExternalImportEnabled",
                schema: "core",
                table: "EnergyMeters");

            migrationBuilder.DropColumn(
                name: "IsManualEntryEnabled",
                schema: "core",
                table: "EnergyMeters");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "core",
                table: "EnergyMeters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

