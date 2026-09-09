using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppRulesIntegrationPublishLaunch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "table_definitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "table_definitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldMappingJson",
                table: "app_definitions",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "app_definitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LinkedTableId",
                table: "app_definitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinkedWorkflowDefinitionId",
                table: "app_definitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "app_definitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "app_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    DataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_app_records_app_definitions_AppDefinitionId",
                        column: x => x.AppDefinitionId,
                        principalTable: "app_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "table_records",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    DataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_table_records_table_definitions_TableDefinitionId",
                        column: x => x.TableDefinitionId,
                        principalTable: "table_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_records_AppDefinitionId",
                table: "app_records",
                column: "AppDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_table_records_TableDefinitionId",
                table: "table_records",
                column: "TableDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_records");

            migrationBuilder.DropTable(
                name: "table_records");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "table_definitions");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "table_definitions");

            migrationBuilder.DropColumn(
                name: "FieldMappingJson",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "LinkedTableId",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "LinkedWorkflowDefinitionId",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "app_definitions");
        }
    }
}
