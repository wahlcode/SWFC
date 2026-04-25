using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M201_AddMachineFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "core",
                table: "Machines",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InventoryNumber",
                schema: "core",
                table: "Machines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "core",
                table: "Machines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                schema: "core",
                table: "Machines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                schema: "core",
                table: "Machines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                schema: "core",
                table: "Machines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "Machines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_InventoryNumber",
                schema: "core",
                table: "Machines",
                column: "InventoryNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Machines_InventoryNumber",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "InventoryNumber",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Model",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "Machines");
        }
    }
}

