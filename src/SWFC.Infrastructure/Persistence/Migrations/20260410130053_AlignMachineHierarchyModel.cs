using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignMachineHierarchyModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Machines_MachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_MachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MachineId",
                schema: "core",
                table: "Machines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MachineId",
                schema: "core",
                table: "Machines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineId",
                schema: "core",
                table: "Machines",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Machines_MachineId",
                schema: "core",
                table: "Machines",
                column: "MachineId",
                principalSchema: "core",
                principalTable: "Machines",
                principalColumn: "Id");
        }
    }
}

