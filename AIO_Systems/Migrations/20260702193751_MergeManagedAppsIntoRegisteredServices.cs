using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AIO_Systems.Migrations
{
    /// <inheritdoc />
    public partial class MergeManagedAppsIntoRegisteredServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagedApps");

            migrationBuilder.AddColumn<string>(
                name: "BackendCommand",
                table: "RegisteredServices",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackendWorkingDirectory",
                table: "RegisteredServices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "RegisteredServices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FrontendCommand",
                table: "RegisteredServices",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontendUrl",
                table: "RegisteredServices",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontendWorkingDirectory",
                table: "RegisteredServices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "RegisteredServices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "RegisteredServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill the launch fields the two existing rows need, using the exact
            // values that used to live in Program.cs's ManagedApps seed (now removed —
            // Registered Services is the single source of truth for this going forward).
            migrationBuilder.Sql(@"
                UPDATE ""RegisteredServices""
                SET ""BackendWorkingDirectory"" = 'C:\dotnet_practice\FoodOrderingSystem\FoodOrderingSystemManagement\src\FoodOrder.API',
                    ""BackendCommand"" = 'dotnet run',
                    ""FrontendUrl"" = 'http://localhost:4200',
                    ""FrontendWorkingDirectory"" = 'C:\dotnet_practice\FoodOrderingSystem\FoodOrderingSystemManagement\client',
                    ""FrontendCommand"" = 'npm start',
                    ""Icon"" = 'restaurant',
                    ""Color"" = '#D97706',
                    ""SortOrder"" = 1
                WHERE ""RoutePrefix"" = 'restaurant';

                UPDATE ""RegisteredServices""
                SET ""BackendWorkingDirectory"" = 'C:\dotnet_practice\FoodOrderingSystem\PharmacyManagement\Pharmacy.API',
                    ""BackendCommand"" = 'dotnet run',
                    ""FrontendUrl"" = 'http://localhost:4201',
                    ""FrontendWorkingDirectory"" = 'C:\dotnet_practice\FoodOrderingSystem\PharmacyManagement\client',
                    ""FrontendCommand"" = 'npm start',
                    ""Icon"" = 'local_pharmacy',
                    ""Color"" = '#0F766E',
                    ""SortOrder"" = 2
                WHERE ""RoutePrefix"" = 'pharmacy';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackendCommand",
                table: "RegisteredServices");

            migrationBuilder.DropColumn(
                name: "BackendWorkingDirectory",
                table: "RegisteredServices");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "RegisteredServices");

            migrationBuilder.DropColumn(
                name: "FrontendCommand",
                table: "RegisteredServices");

            migrationBuilder.DropColumn(
                name: "FrontendUrl",
                table: "RegisteredServices");

            migrationBuilder.DropColumn(
                name: "FrontendWorkingDirectory",
                table: "RegisteredServices");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "RegisteredServices");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "RegisteredServices");

            migrationBuilder.CreateTable(
                name: "ManagedApps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BackendCommand = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BackendPort = table.Column<int>(type: "integer", nullable: false),
                    BackendWorkingDirectory = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FrontendCommand = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FrontendPort = table.Column<int>(type: "integer", nullable: true),
                    FrontendWorkingDirectory = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagedApps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagedApps_Name",
                table: "ManagedApps",
                column: "Name",
                unique: true);
        }
    }
}
