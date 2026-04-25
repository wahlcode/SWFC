using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class M102ShiftModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShiftModels",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftModels_Code",
                schema: "core",
                table: "ShiftModels",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftModels",
                schema: "core");
        }
    }
}
