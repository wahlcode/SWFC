using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalCredentials_Users_UserId",
                schema: "core",
                table: "LocalCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Stocks_StockId",
                schema: "core",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                schema: "core",
                table: "StockReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganizationUnits_Users_UserId",
                schema: "core",
                table: "UserOrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                schema: "core",
                table: "UserRoles");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "core",
                table: "UserRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "core",
                table: "UserOrganizationUnits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "core",
                table: "Machines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RfidTag",
                schema: "core",
                table: "EnergyMeters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SupportsOfflineCapture",
                schema: "core",
                table: "EnergyMeters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "core",
                table: "AccessRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_LocalCredentials_Users_UserId",
                schema: "core",
                table: "LocalCredentials",
                column: "UserId",
                principalSchema: "core",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Stocks_StockId",
                schema: "core",
                table: "StockMovements",
                column: "StockId",
                principalSchema: "core",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                schema: "core",
                table: "StockReservations",
                column: "StockId",
                principalSchema: "core",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserOrganizationUnits_Users_UserId",
                schema: "core",
                table: "UserOrganizationUnits",
                column: "UserId",
                principalSchema: "core",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                schema: "core",
                table: "UserRoles",
                column: "UserId",
                principalSchema: "core",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalCredentials_Users_UserId",
                schema: "core",
                table: "LocalCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Stocks_StockId",
                schema: "core",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                schema: "core",
                table: "StockReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganizationUnits_Users_UserId",
                schema: "core",
                table: "UserOrganizationUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                schema: "core",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "core",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "core",
                table: "UserOrganizationUnits");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "RfidTag",
                schema: "core",
                table: "EnergyMeters");

            migrationBuilder.DropColumn(
                name: "SupportsOfflineCapture",
                schema: "core",
                table: "EnergyMeters");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "core",
                table: "AccessRules");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalCredentials_Users_UserId",
                schema: "core",
                table: "LocalCredentials",
                column: "UserId",
                principalSchema: "core",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Stocks_StockId",
                schema: "core",
                table: "StockMovements",
                column: "StockId",
                principalSchema: "core",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockReservations_Stocks_StockId",
                schema: "core",
                table: "StockReservations",
                column: "StockId",
                principalSchema: "core",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserOrganizationUnits_Users_UserId",
                schema: "core",
                table: "UserOrganizationUnits",
                column: "UserId",
                principalSchema: "core",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                schema: "core",
                table: "UserRoles",
                column: "UserId",
                principalSchema: "core",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
