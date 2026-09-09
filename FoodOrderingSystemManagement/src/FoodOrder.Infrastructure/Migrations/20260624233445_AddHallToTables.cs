using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHallToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_TableNumber",
                table: "Tables");

            migrationBuilder.AddColumn<string>(
                name: "Hall",
                table: "Tables",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "AC");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_Hall_TableNumber",
                table: "Tables",
                columns: new[] { "Hall", "TableNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_Hall_TableNumber",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Hall",
                table: "Tables");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_TableNumber",
                table: "Tables",
                column: "TableNumber",
                unique: true);
        }
    }
}
