using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M206_AddOperationalReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryDocumentReference",
                schema: "core",
                table: "GoodsReceipts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpReference",
                schema: "core",
                table: "PurchaseOrders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderDocumentReference",
                schema: "core",
                table: "PurchaseOrders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferDocumentReference",
                schema: "core",
                table: "RequestForQuotations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDocumentReference",
                schema: "core",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ErpReference",
                schema: "core",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderDocumentReference",
                schema: "core",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OfferDocumentReference",
                schema: "core",
                table: "RequestForQuotations");
        }
    }
}
