using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRolesSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Roles table was seeded with explicit IDs 1-4 via HasData, but the
            // PostgreSQL sequence was never advanced, so the next INSERT tries ID 1 and
            // hits a duplicate PK. Advance the sequence to max(Id) so new roles get IDs 5+.
            migrationBuilder.Sql(@"SELECT setval('""Roles_Id_seq""', (SELECT MAX(""Id"") FROM ""Roles""));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
