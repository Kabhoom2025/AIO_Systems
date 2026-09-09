using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppBusinessRulesAndAccessPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessPermissionsJson",
                table: "app_definitions",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessRulesJson",
                table: "app_definitions",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessPermissionsJson",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "BusinessRulesJson",
                table: "app_definitions");
        }
    }
}
