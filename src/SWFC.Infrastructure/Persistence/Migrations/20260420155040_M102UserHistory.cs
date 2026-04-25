using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M102UserHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserHistoryEntries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SnapshotJson = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHistoryEntries_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "core",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserHistoryEntries_ChangedAtUtc",
                schema: "core",
                table: "UserHistoryEntries",
                column: "ChangedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistoryEntries_UserId",
                schema: "core",
                table: "UserHistoryEntries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHistoryEntries",
                schema: "core");
        }
    }
}
