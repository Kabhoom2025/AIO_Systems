using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Handle role updates using raw SQL to bypass FK constraints
            migrationBuilder.Sql(@"
                -- Temporarily disable foreign key checks
                SET session_replication_role = 'replica';

                -- Delete existing roles
                DELETE FROM ""Roles"";

                -- Re-insert roles with new structure including SuperAdmin
                INSERT INTO ""Roles"" (""Id"", ""RoleName"")
                VALUES
                    (1, 'SuperAdmin'),
                    (2, 'Admin'),
                    (3, 'Cashier'),
                    (4, 'Waiter'),
                    (5, 'InventoryManager');

                -- Reset sequence
                SELECT setval('""Roles_Id_seq""', (SELECT MAX(""Id"") FROM ""Roles""));

                -- Re-enable foreign key checks
                SET session_replication_role = 'origin';
            ");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            // On a brand-new database no Organization exists yet, so seed a
            // default one at Id 1 — later migrations backfill Branches/Settings/
            // Suppliers etc. against this org and fail their FK checks otherwise.
            migrationBuilder.Sql(@"
                INSERT INTO ""Organizations"" (""Id"", ""Name"", ""IsActive"", ""CreatedDate"")
                SELECT 1, 'Default Organization', true, NOW()
                WHERE NOT EXISTS (SELECT 1 FROM ""Organizations"" WHERE ""Id"" = 1);

                SELECT setval('""Organizations_Id_seq""', (SELECT COALESCE(MAX(""Id""), 1) FROM ""Organizations""));
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            // Restore the old roles
            migrationBuilder.Sql(@"
                -- Temporarily disable foreign key checks
                SET session_replication_role = 'replica';

                -- Delete the new roles
                DELETE FROM ""Roles"";

                -- Restore the old roles
                INSERT INTO ""Roles"" (""Id"", ""RoleName"")
                VALUES
                    (1, 'Admin'),
                    (2, 'Cashier'),
                    (3, 'Waiter'),
                    (4, 'InventoryManager');

                -- Reset sequence
                SELECT setval('""Roles_Id_seq""', (SELECT MAX(""Id"") FROM ""Roles""));

                -- Re-enable foreign key checks
                SET session_replication_role = 'origin';
            ");
        }
    }
}
