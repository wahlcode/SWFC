using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M806_PermissionsAndRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AuditInfo_CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedBy = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_LastModifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "core",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "core",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                schema: "core",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                schema: "core",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                schema: "core",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            var createdAtUtc = new DateTime(2026, 4, 19, 12, 59, 10, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                schema: "core",
                table: "Permissions",
                columns: new[]
                {
                    "Id",
                    "Code",
                    "Name",
                    "Description",
                    "Module",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy"
                },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "security.read", "Security Read", "Read security-related data.", "M800", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "security.write", "Security Write", "Change security-related data.", "M800", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000003"), "machine.read", "Machine Read", "Read machine data.", "M201", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "machine.create", "Machine Create", "Create machines.", "M201", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "machine.update", "Machine Update", "Update machines.", "M201", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "machine.delete", "Machine Delete", "Delete machines.", "M201", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000007"), "inventoryitem.read", "Inventory Item Read", "Read inventory items.", "M204", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "inventoryitem.create", "Inventory Item Create", "Create inventory items.", "M204", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "inventoryitem.update", "Inventory Item Update", "Update inventory items.", "M204", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "inventoryitem.delete", "Inventory Item Delete", "Delete inventory items.", "M204", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000011"), "stock.read", "Stock Read", "Read stock data.", "M204", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "stock.write", "Stock Write", "Change stock data.", "M204", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000013"), "stockmovement.read", "Stock Movement Read", "Read stock movements.", "M204", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000014"), "stockmovement.create", "Stock Movement Create", "Create stock movements.", "M204", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000015"), "stockreservation.read", "Stock Reservation Read", "Read stock reservations.", "M204", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000016"), "stockreservation.create", "Stock Reservation Create", "Create stock reservations.", "M204", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000017"), "stockreservation.release", "Stock Reservation Release", "Release stock reservations.", "M204", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000018"), "organization.read", "Organization Read", "Read organization data.", "M102", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000019"), "organization.write", "Organization Write", "Change organization data.", "M102", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000020"), "m202.maintenance-orders.read", "Maintenance Orders Read", "Read maintenance orders.", "M202", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000021"), "m202.maintenance-orders.create", "Maintenance Orders Create", "Create maintenance orders.", "M202", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000022"), "m202.maintenance-orders.update", "Maintenance Orders Update", "Update maintenance orders.", "M202", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000023"), "m202.maintenance-plans.read", "Maintenance Plans Read", "Read maintenance plans.", "M202", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000024"), "m202.maintenance-plans.create", "Maintenance Plans Create", "Create maintenance plans.", "M202", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000025"), "m202.maintenance-plans.update", "Maintenance Plans Update", "Update maintenance plans.", "M202", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000026"), "m205.energy-meters.read", "Energy Meters Read", "Read energy meters.", "M205", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000027"), "m205.energy-meters.create", "Energy Meters Create", "Create energy meters.", "M205", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000028"), "m205.energy-meters.update", "Energy Meters Update", "Update energy meters.", "M205", true, createdAtUtc, "migration", null, null },

                    { new Guid("10000000-0000-0000-0000-000000000029"), "m205.energy-readings.read", "Energy Readings Read", "Read energy readings.", "M205", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000030"), "m205.energy-readings.create", "Energy Readings Create", "Create energy readings.", "M205", true, createdAtUtc, "migration", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000031"), "m205.energy-readings.update", "Energy Readings Update", "Update energy readings.", "M205", true, createdAtUtc, "migration", null, null }
                });

            migrationBuilder.Sql("""
                INSERT INTO core."RolePermissions" (
                    "Id",
                    "RoleId",
                    "PermissionId",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy")
                SELECT gen_random_uuid(), r."Id", p."Id", true, NOW(), 'migration', NULL, NULL
                FROM core."Roles" r
                CROSS JOIN core."Permissions" p
                WHERE r."Name" = 'Admin';
                """);

            migrationBuilder.Sql("""
                INSERT INTO core."RolePermissions" (
                    "Id",
                    "RoleId",
                    "PermissionId",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy")
                SELECT gen_random_uuid(), r."Id", p."Id", true, NOW(), 'migration', NULL, NULL
                FROM core."Roles" r
                JOIN core."Permissions" p ON p."Code" IN (
                    'machine.read',
                    'inventoryitem.read',
                    'stock.read',
                    'stockmovement.read',
                    'stockreservation.read',
                    'organization.read'
                )
                WHERE r."Name" = 'Viewer';
                """);

            migrationBuilder.Sql("""
                INSERT INTO core."RolePermissions" (
                    "Id",
                    "RoleId",
                    "PermissionId",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy")
                SELECT gen_random_uuid(), r."Id", p."Id", true, NOW(), 'migration', NULL, NULL
                FROM core."Roles" r
                JOIN core."Permissions" p ON p."Code" IN (
                    'inventoryitem.read',
                    'inventoryitem.create',
                    'inventoryitem.update',
                    'inventoryitem.delete',
                    'stock.read',
                    'stock.write',
                    'stockmovement.read',
                    'stockmovement.create',
                    'stockreservation.read',
                    'stockreservation.create',
                    'stockreservation.release'
                )
                WHERE r."Name" = 'InventoryUser';
                """);

            migrationBuilder.Sql("""
                INSERT INTO core."RolePermissions" (
                    "Id",
                    "RoleId",
                    "PermissionId",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy")
                SELECT gen_random_uuid(), r."Id", p."Id", true, NOW(), 'migration', NULL, NULL
                FROM core."Roles" r
                JOIN core."Permissions" p ON p."Code" IN (
                    'machine.read',
                    'machine.create',
                    'machine.update'
                )
                WHERE r."Name" = 'MaintenanceUser';
                """);

            migrationBuilder.Sql("""
                INSERT INTO core."RolePermissions" (
                    "Id",
                    "RoleId",
                    "PermissionId",
                    "IsActive",
                    "AuditInfo_CreatedAtUtc",
                    "AuditInfo_CreatedBy",
                    "AuditInfo_LastModifiedAtUtc",
                    "AuditInfo_LastModifiedBy")
                SELECT gen_random_uuid(), r."Id", p."Id", true, NOW(), 'migration', NULL, NULL
                FROM core."Roles" r
                JOIN core."Permissions" p ON p."Code" IN (
                    'security.read',
                    'security.write',
                    'm202.maintenance-orders.read',
                    'm202.maintenance-orders.create',
                    'm202.maintenance-orders.update',
                    'm202.maintenance-plans.read',
                    'm202.maintenance-plans.create',
                    'm202.maintenance-plans.update',
                    'm205.energy-meters.read',
                    'm205.energy-meters.create',
                    'm205.energy-meters.update',
                    'm205.energy-readings.read',
                    'm205.energy-readings.create',
                    'm205.energy-readings.update'
                )
                WHERE r."Name" = 'Developer';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "core");
        }
    }
}
