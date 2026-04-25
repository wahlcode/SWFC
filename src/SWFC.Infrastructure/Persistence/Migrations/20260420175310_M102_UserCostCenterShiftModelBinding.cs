using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M102_UserCostCenterShiftModelBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostCenter",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Shift",
                schema: "core",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                schema: "core",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftModelId",
                schema: "core",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CostCenterId",
                schema: "core",
                table: "Users",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ShiftModelId",
                schema: "core",
                table: "Users",
                column: "ShiftModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CostCenters_CostCenterId",
                schema: "core",
                table: "Users",
                column: "CostCenterId",
                principalSchema: "core",
                principalTable: "CostCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ShiftModels_ShiftModelId",
                schema: "core",
                table: "Users",
                column: "ShiftModelId",
                principalSchema: "core",
                principalTable: "ShiftModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_CostCenters_CostCenterId",
                schema: "core",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_ShiftModels_ShiftModelId",
                schema: "core",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CostCenterId",
                schema: "core",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ShiftModelId",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShiftModelId",
                schema: "core",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "CostCenter",
                schema: "core",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Shift",
                schema: "core",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
