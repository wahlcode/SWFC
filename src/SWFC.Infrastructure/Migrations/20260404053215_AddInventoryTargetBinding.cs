using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTargetBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetReference",
                schema: "core",
                table: "StockReservations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                schema: "core",
                table: "StockReservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetReference",
                schema: "core",
                table: "StockMovements",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                schema: "core",
                table: "StockMovements",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetReference",
                schema: "core",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "TargetType",
                schema: "core",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "TargetReference",
                schema: "core",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "TargetType",
                schema: "core",
                table: "StockMovements");
        }
    }
}
