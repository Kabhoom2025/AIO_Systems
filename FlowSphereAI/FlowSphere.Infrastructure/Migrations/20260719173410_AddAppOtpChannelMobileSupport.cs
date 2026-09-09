using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppOtpChannelMobileSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_app_otp_challenges_AppId_Email",
                table: "app_otp_challenges");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "app_otp_challenges",
                newName: "Recipient");

            migrationBuilder.AddColumn<int>(
                name: "Channel",
                table: "app_otp_challenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_app_otp_challenges_AppId_Channel_Recipient",
                table: "app_otp_challenges",
                columns: new[] { "AppId", "Channel", "Recipient" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_app_otp_challenges_AppId_Channel_Recipient",
                table: "app_otp_challenges");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "app_otp_challenges");

            migrationBuilder.RenameColumn(
                name: "Recipient",
                table: "app_otp_challenges",
                newName: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_app_otp_challenges_AppId_Email",
                table: "app_otp_challenges",
                columns: new[] { "AppId", "Email" });
        }
    }
}
