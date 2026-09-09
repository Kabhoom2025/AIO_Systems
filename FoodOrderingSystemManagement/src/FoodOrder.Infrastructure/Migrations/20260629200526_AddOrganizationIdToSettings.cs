using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Settings",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrganizationId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_OrganizationId",
                table: "Settings",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_Organizations_OrganizationId",
                table: "Settings",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Settings_Organizations_OrganizationId",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Settings_OrganizationId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Settings");
        }
    }
}
