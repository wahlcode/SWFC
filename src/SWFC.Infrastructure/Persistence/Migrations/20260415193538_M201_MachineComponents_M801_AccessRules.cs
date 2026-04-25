using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M201_MachineComponents_M801_AccessRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessRules",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubjectType = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineComponentAreas",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineComponentAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineComponents",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineComponentAreaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentMachineComponentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineComponents_MachineComponentAreas_MachineComponentAre~",
                        column: x => x.MachineComponentAreaId,
                        principalSchema: "core",
                        principalTable: "MachineComponentAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineComponents_MachineComponents_ParentMachineComponentId",
                        column: x => x.ParentMachineComponentId,
                        principalSchema: "core",
                        principalTable: "MachineComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineComponents_Machines_MachineId",
                        column: x => x.MachineId,
                        principalSchema: "core",
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessRules_TargetType_TargetId_SubjectType_SubjectId",
                schema: "core",
                table: "AccessRules",
                columns: new[] { "TargetType", "TargetId", "SubjectType", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponentAreas_Name",
                schema: "core",
                table: "MachineComponentAreas",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineComponentAreaId",
                schema: "core",
                table: "MachineComponents",
                column: "MachineComponentAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_MachineId_Name",
                schema: "core",
                table: "MachineComponents",
                columns: new[] { "MachineId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_MachineComponents_ParentMachineComponentId",
                schema: "core",
                table: "MachineComponents",
                column: "ParentMachineComponentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessRules",
                schema: "core");

            migrationBuilder.DropTable(
                name: "MachineComponents",
                schema: "core");

            migrationBuilder.DropTable(
                name: "MachineComponentAreas",
                schema: "core");
        }
    }
}

