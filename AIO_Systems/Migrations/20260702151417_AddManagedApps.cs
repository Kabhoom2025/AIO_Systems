using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AIO_Systems.Migrations
{
    /// <inheritdoc />
    public partial class AddManagedApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagedApps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BackendWorkingDirectory = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BackendCommand = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BackendPort = table.Column<int>(type: "integer", nullable: false),
                    FrontendWorkingDirectory = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FrontendCommand = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FrontendPort = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagedApps");
        }
    }
}
