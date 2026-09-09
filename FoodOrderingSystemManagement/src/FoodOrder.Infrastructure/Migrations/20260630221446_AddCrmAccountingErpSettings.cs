using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmAccountingErpSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountingApiKey",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountingProvider",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountingWebhookUrl",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrmApiKey",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrmProvider",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrmWebhookUrl",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpApiKey",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpProvider",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpWebhookUrl",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountingApiKey", "AccountingProvider", "AccountingWebhookUrl", "CrmApiKey", "CrmProvider", "CrmWebhookUrl", "ErpApiKey", "ErpProvider", "ErpWebhookUrl" },
                values: new object[] { null, null, null, null, null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountingApiKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "AccountingProvider",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "AccountingWebhookUrl",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CrmApiKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CrmProvider",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CrmWebhookUrl",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ErpApiKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ErpProvider",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ErpWebhookUrl",
                table: "Settings");
        }
    }
}
