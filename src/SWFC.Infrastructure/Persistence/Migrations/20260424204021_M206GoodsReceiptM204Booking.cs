using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M206GoodsReceiptM204Booking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredItem",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InventoryBookingRequested",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                schema: "core",
                table: "GoodsReceipts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "Bin",
                schema: "core",
                table: "GoodsReceipts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InventoryBookingMessage",
                schema: "core",
                table: "GoodsReceipts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InventoryBookingStatus",
                schema: "core",
                table: "GoodsReceipts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryItemId",
                schema: "core",
                table: "GoodsReceipts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryStockMovementId",
                schema: "core",
                table: "GoodsReceipts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "core",
                table: "GoodsReceipts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_InventoryBookingStatus",
                schema: "core",
                table: "GoodsReceipts",
                column: "InventoryBookingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_InventoryItemId",
                schema: "core",
                table: "GoodsReceipts",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_InventoryStockMovementId",
                schema: "core",
                table: "GoodsReceipts",
                column: "InventoryStockMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_LocationId",
                schema: "core",
                table: "GoodsReceipts",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_InventoryItems_InventoryItemId",
                schema: "core",
                table: "GoodsReceipts",
                column: "InventoryItemId",
                principalSchema: "core",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_Locations_LocationId",
                schema: "core",
                table: "GoodsReceipts",
                column: "LocationId",
                principalSchema: "core",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceipts_StockMovements_InventoryStockMovementId",
                schema: "core",
                table: "GoodsReceipts",
                column: "InventoryStockMovementId",
                principalSchema: "core",
                principalTable: "StockMovements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_InventoryItems_InventoryItemId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_Locations_LocationId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceipts_StockMovements_InventoryStockMovementId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_InventoryBookingStatus",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_InventoryItemId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_InventoryStockMovementId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_LocationId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "Bin",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InventoryBookingMessage",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InventoryBookingStatus",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InventoryItemId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InventoryStockMovementId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "core",
                table: "GoodsReceipts",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "DeliveredItem",
                schema: "core",
                table: "GoodsReceipts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "InventoryBookingRequested",
                schema: "core",
                table: "GoodsReceipts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
