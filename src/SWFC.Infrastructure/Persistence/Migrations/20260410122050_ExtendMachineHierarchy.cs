using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendMachineHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedBy",
                schema: "core",
                table: "Machines",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_LastModifiedAtUtc",
                schema: "core",
                table: "Machines",
                newName: "LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "Machines",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "AuditInfo_CreatedAtUtc",
                schema: "core",
                table: "Machines",
                newName: "CreatedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "core",
                table: "Machines",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "MachineId",
                schema: "core",
                table: "Machines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationUnitId",
                schema: "core",
                table: "Machines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentMachineId",
                schema: "core",
                table: "Machines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineId",
                schema: "core",
                table: "Machines",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_ParentMachineId",
                schema: "core",
                table: "Machines",
                column: "ParentMachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Machines_MachineId",
                schema: "core",
                table: "Machines",
                column: "MachineId",
                principalSchema: "core",
                principalTable: "Machines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Machines_ParentMachineId",
                schema: "core",
                table: "Machines",
                column: "ParentMachineId",
                principalSchema: "core",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Machines_MachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Machines_ParentMachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_MachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_ParentMachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "MachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                schema: "core",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ParentMachineId",
                schema: "core",
                table: "Machines");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "core",
                table: "Machines",
                newName: "AuditInfo_LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAtUtc",
                schema: "core",
                table: "Machines",
                newName: "AuditInfo_LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "core",
                table: "Machines",
                newName: "AuditInfo_CreatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "core",
                table: "Machines",
                newName: "AuditInfo_CreatedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_CreatedBy",
                schema: "core",
                table: "Machines",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}

