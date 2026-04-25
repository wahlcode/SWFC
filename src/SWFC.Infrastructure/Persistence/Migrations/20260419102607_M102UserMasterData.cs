using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWFC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M102UserMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessEmail",
                schema: "core",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessPhone",
                schema: "core",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CostCenter",
                schema: "core",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumber",
                schema: "core",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "core",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobFunction",
                schema: "core",
                table: "Users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "core",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "core",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                schema: "core",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
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

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "core",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Inactive");

            migrationBuilder.AddColumn<string>(
                name: "Team",
                schema: "core",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                schema: "core",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Internal");

            migrationBuilder.Sql(
                """
                UPDATE core."Users"
                SET "FirstName" = CASE WHEN NULLIF("FirstName", '') IS NULL THEN COALESCE(NULLIF("DisplayName", ''), 'Unknown') ELSE "FirstName" END,
                    "LastName" = CASE WHEN NULLIF("LastName", '') IS NULL THEN COALESCE(NULLIF("DisplayName", ''), 'Unknown') ELSE "LastName" END,
                    "EmployeeNumber" = CASE WHEN NULLIF("EmployeeNumber", '') IS NULL THEN COALESCE(NULLIF("IdentityKey", ''), "Id"::text) ELSE "EmployeeNumber" END,
                    "BusinessEmail" = CASE WHEN NULLIF("BusinessEmail", '') IS NULL THEN CONCAT(COALESCE(NULLIF("Username", ''), 'user'), '@local.swfc') ELSE "BusinessEmail" END,
                    "BusinessPhone" = CASE WHEN NULLIF("BusinessPhone", '') IS NULL THEN 'n/a' ELSE "BusinessPhone" END,
                    "Plant" = CASE WHEN NULLIF("Plant", '') IS NULL THEN 'Default Plant' ELSE "Plant" END,
                    "Location" = CASE WHEN NULLIF("Location", '') IS NULL THEN 'Default Location' ELSE "Location" END,
                    "Team" = CASE WHEN NULLIF("Team", '') IS NULL THEN 'Default Team' ELSE "Team" END,
                    "CostCenter" = CASE WHEN NULLIF("CostCenter", '') IS NULL THEN '0000' ELSE "CostCenter" END,
                    "Shift" = CASE WHEN NULLIF("Shift", '') IS NULL THEN 'Day' ELSE "Shift" END,
                    "JobFunction" = CASE WHEN NULLIF("JobFunction", '') IS NULL THEN 'Unassigned' ELSE "JobFunction" END,
                    "Status" = CASE WHEN "IsActive" THEN 'Active' ELSE 'Inactive' END,
                    "UserType" = CASE WHEN NULLIF("UserType", '') IS NULL THEN 'Internal' ELSE "UserType" END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessEmail",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessPhone",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CostCenter",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmployeeNumber",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobFunction",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Plant",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Shift",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Team",
                schema: "core",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserType",
                schema: "core",
                table: "Users");
        }
    }
}
