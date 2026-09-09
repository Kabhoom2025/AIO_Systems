using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureTogglesAndIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DunzoApiKey",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FeatDeliveryModule",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeatKitchenDisplay",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeatLoyaltyProgram",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeatOnlineOrdering",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeatPromotionsCoupons",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeatSmsNotifications",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeatTableReservations",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeatWhatsappAlerts",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoogleAnalyticsId",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailchimpApiKey",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlackWebhookUrl",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwiggyApiKey",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DunzoApiKey", "FeatDeliveryModule", "FeatKitchenDisplay", "FeatLoyaltyProgram", "FeatOnlineOrdering", "FeatPromotionsCoupons", "FeatSmsNotifications", "FeatTableReservations", "FeatWhatsappAlerts", "GoogleAnalyticsId", "MailchimpApiKey", "SlackWebhookUrl", "SwiggyApiKey" },
                values: new object[] { null, true, true, true, true, false, false, false, false, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DunzoApiKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatDeliveryModule",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatKitchenDisplay",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatLoyaltyProgram",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatOnlineOrdering",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatPromotionsCoupons",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatSmsNotifications",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatTableReservations",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "FeatWhatsappAlerts",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GoogleAnalyticsId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "MailchimpApiKey",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SlackWebhookUrl",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SwiggyApiKey",
                table: "Settings");
        }
    }
}
