using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    public partial class M201_AddAssetTypeAndEnergyReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetType",
                schema: "core",
                table: "Machines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "EnergyObjectId",
                schema: "core",
                table: "Machines",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetType",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "EnergyObjectId",
                schema: "core",
                table: "Machines");
        }
    }
}
