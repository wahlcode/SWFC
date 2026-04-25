using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInventoryToV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var defaultLocationId = new Guid("11111111-1111-1111-1111-111111111111");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_InventoryItems_InventoryItemId",
                schema: "core",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_InventoryItemId",
                schema: "core",
                table: "Stocks");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedBy",
                schema: "core",
                table: "Stocks",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedAtUtc",
                schema: "core",
                table: "Stocks",
                newName: "LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "Stocks",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedAtUtc",
                schema: "core",
                table: "Stocks",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedBy",
                schema: "core",
                table: "StockReservations",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedAtUtc",
                schema: "core",
                table: "StockReservations",
                newName: "LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "StockReservations",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedAtUtc",
                schema: "core",
                table: "StockReservations",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedBy",
                schema: "core",
                table: "StockMovements",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedAtUtc",
                schema: "core",
                table: "StockMovements",
                newName: "LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "StockMovements",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedAtUtc",
                schema: "core",
                table: "StockMovements",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedBy",
                schema: "core",
                table: "InventoryItems",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedAtUtc",
                schema: "core",
                table: "InventoryItems",
                newName: "LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "InventoryItems",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedAtUtc",
                schema: "core",
                table: "InventoryItems",
                newName: "CreatedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "core",
                table: "Stocks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Bin",
                schema: "core",
                table: "Stocks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "core",
                table: "Stocks",
                type: "uuid",
                nullable: false,
                defaultValue: defaultLocationId);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "core",
                table: "StockReservations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "core",
                table: "StockMovements",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ArticleNumber",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "core",
                table: "InventoryItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerPartNumber",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "core",
                table: "InventoryItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Locations_ParentLocationId",
                        column: x => x.ParentLocationId,
                        principalSchema: "core",
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "core",
                table: "Locations",
                columns: new[]
                {
                    "Id",
                    "Name",
                    "Code",
                    "ParentLocationId",
                    "CreatedAtUtc",
                    "CreatedBy",
                    "LastModifiedAtUtc",
                    "LastModifiedBy"
                },
                values: new object[]
                {
                    defaultLocationId,
                    "Default Location",
                    "DEFAULT",
                    null,
                    new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                    "system",
                    null,
                    null
                });

            migrationBuilder.Sql($"""
                UPDATE core."Stocks"
                SET "LocationId" = '{defaultLocationId}'
                WHERE "LocationId" IS NULL
                   OR "LocationId" = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_InventoryItemId_LocationId_Bin",
                schema: "core",
                table: "Stocks",
                columns: new[] { "InventoryItemId", "LocationId", "Bin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_LocationId",
                schema: "core",
                table: "Stocks",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ParentLocationId",
                schema: "core",
                table: "Locations",
                column: "ParentLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_InventoryItems_InventoryItemId",
                schema: "core",
                table: "Stocks",
                column: "InventoryItemId",
                principalSchema: "core",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Locations_LocationId",
                schema: "core",
                table: "Stocks",
                column: "LocationId",
                principalSchema: "core",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_InventoryItems_InventoryItemId",
                schema: "core",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Locations_LocationId",
                schema: "core",
                table: "Stocks");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_InventoryItemId_LocationId_Bin",
                schema: "core",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_LocationId",
                schema: "core",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Bin",
                schema: "core",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "core",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "ArticleNumber",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Barcode",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ManufacturerPartNumber",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "core",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "core",
                table: "Stocks",
                newName: "AuditInfo_LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAtUtc",
                schema: "core",
                table: "Stocks",
                newName: "AuditInfo_LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "core",
                table: "Stocks",
                newName: "AuditInfo_CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "core",
                table: "Stocks",
                newName: "AuditInfo_CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "core",
                table: "StockReservations",
                newName: "AuditInfo_LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAtUtc",
                schema: "core",
                table: "StockReservations",
                newName: "AuditInfo_LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "core",
                table: "StockReservations",
                newName: "AuditInfo_CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "core",
                table: "StockReservations",
                newName: "AuditInfo_CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "core",
                table: "StockMovements",
                newName: "AuditInfo_LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAtUtc",
                schema: "core",
                table: "StockMovements",
                newName: "AuditInfo_LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "core",
                table: "StockMovements",
                newName: "AuditInfo_CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "core",
                table: "StockMovements",
                newName: "AuditInfo_CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "core",
                table: "InventoryItems",
                newName: "AuditInfo_LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAtUtc",
                schema: "core",
                table: "InventoryItems",
                newName: "AuditInfo_LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "core",
                table: "InventoryItems",
                newName: "AuditInfo_CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "core",
                table: "InventoryItems",
                newName: "AuditInfo_CreatedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "Stocks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "StockReservations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "StockMovements",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "InventoryItems",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_InventoryItemId",
                schema: "core",
                table: "Stocks",
                column: "InventoryItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_InventoryItems_InventoryItemId",
                schema: "core",
                table: "Stocks",
                column: "InventoryItemId",
                principalSchema: "core",
                principalTable: "InventoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

