using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsExpanded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoBackupEnabled",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BackupEmail",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BackupFrequencyHours",
                table: "Settings",
                type: "integer",
                nullable: false,
                defaultValue: 24);

            migrationBuilder.AddColumn<bool>(
                name: "MenuShowDescriptions",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MenuShowImages",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MenuShowOutOfStock",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentCardEnabled",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentCashEnabled",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaymentOnlineEnabled",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PosAutoPrintBill",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PosDefaultOrderType",
                table: "Settings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DineIn");

            migrationBuilder.AddColumn<bool>(
                name: "PosEnableTips",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PosRequireCustomerPhone",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrinterAutoPrint",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PrinterEnabled",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrinterIp",
                table: "Settings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrinterPort",
                table: "Settings",
                type: "integer",
                nullable: false,
                defaultValue: 9100);

            migrationBuilder.AddColumn<string>(
                name: "PrinterType",
                table: "Settings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Thermal");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayKeyId",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazorpayKeySecret",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromEmail",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromName",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "Settings",
                type: "integer",
                nullable: false,
                defaultValue: 587);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpSsl",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "Settings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaxInclusive",
                table: "Settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxName",
                table: "Settings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "GST");

            migrationBuilder.AddColumn<string>(
                name: "ThemeMode",
                table: "Settings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "light");

            migrationBuilder.AddColumn<string>(
                name: "ThemePrimaryColor",
                table: "Settings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "#bf360c");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BackupEmail", "BackupFrequencyHours", "MenuShowDescriptions", "MenuShowImages", "MenuShowOutOfStock", "PaymentCardEnabled", "PaymentCashEnabled", "PosDefaultOrderType", "PrinterIp", "PrinterPort", "PrinterType", "RazorpayKeyId", "RazorpayKeySecret", "SmtpFromEmail", "SmtpFromName", "SmtpHost", "SmtpPassword", "SmtpPort", "SmtpSsl", "SmtpUsername", "TaxName", "ThemeMode", "ThemePrimaryColor" },
                values: new object[] { null, 24, true, true, true, true, true, "DineIn", null, 9100, "Thermal", null, null, null, null, null, null, 587, true, null, "GST", "light", "#bf360c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoBackupEnabled",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "BackupEmail",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "BackupFrequencyHours",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "MenuShowDescriptions",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "MenuShowImages",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "MenuShowOutOfStock",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PaymentCardEnabled",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PaymentCashEnabled",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PaymentOnlineEnabled",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PosAutoPrintBill",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PosDefaultOrderType",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PosEnableTips",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PosRequireCustomerPhone",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PrinterAutoPrint",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PrinterEnabled",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PrinterIp",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PrinterPort",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PrinterType",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "RazorpayKeyId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "RazorpayKeySecret",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SmtpFromEmail",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SmtpFromName",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SmtpSsl",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TaxInclusive",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TaxName",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ThemeMode",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "ThemePrimaryColor",
                table: "Settings");
        }
    }
}
