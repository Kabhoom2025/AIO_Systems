using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppPublishSettingsAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublishedSnapshotJson",
                table: "app_definitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettingsJson",
                table: "app_definitions",
                type: "text",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "UserManualMarkdown",
                table: "app_definitions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "app_publish_history_entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    AppId = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    PublishedByUserId = table.Column<int>(type: "integer", nullable: false),
                    PublishedByUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_publish_history_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_publish_history_entries_OrganizationId_AppId",
                table: "app_publish_history_entries",
                columns: new[] { "OrganizationId", "AppId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_publish_history_entries");

            migrationBuilder.DropColumn(
                name: "PublishedSnapshotJson",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "SettingsJson",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "UserManualMarkdown",
                table: "app_definitions");
        }
    }
}
