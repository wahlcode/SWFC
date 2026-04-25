using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovements",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    QuantityDelta = table.Column<int>(type: "integer", nullable: false),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_Stocks_StockId",
                        column: x => x.StockId,
                        principalSchema: "core",
                        principalTable: "Stocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockId",
                schema: "core",
                table: "StockMovements",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements",
                schema: "core");
        }
    }
}

