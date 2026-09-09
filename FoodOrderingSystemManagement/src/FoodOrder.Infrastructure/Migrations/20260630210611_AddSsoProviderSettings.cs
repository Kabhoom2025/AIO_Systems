using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSsoProviderSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SsoFacebookAppId",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoFacebookAppSecret",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoGitHubClientId",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoGitHubClientSecret",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoGoogleClientId",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoGoogleClientSecret",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoMicrosoftClientId",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoMicrosoftClientSecret",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoMicrosoftTenantId",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SsoFacebookAppId", "SsoFacebookAppSecret", "SsoGitHubClientId", "SsoGitHubClientSecret", "SsoGoogleClientId", "SsoGoogleClientSecret", "SsoMicrosoftClientId", "SsoMicrosoftClientSecret", "SsoMicrosoftTenantId" },
                values: new object[] { null, null, null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SsoFacebookAppId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoFacebookAppSecret",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoGitHubClientId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoGitHubClientSecret",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoGoogleClientId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoGoogleClientSecret",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoMicrosoftClientId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoMicrosoftClientSecret",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SsoMicrosoftTenantId",
                table: "Settings");
        }
    }
}
