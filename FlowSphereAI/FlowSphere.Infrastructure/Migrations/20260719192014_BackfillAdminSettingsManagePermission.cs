using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillAdminSettingsManagePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Role.Permissions is a snapshot string captured when the role was created (see
            // Role.cs) - it does NOT automatically pick up new PermissionCatalog constants added
            // after the fact. Every existing "Admin" role needs "settings.manage" appended so
            // current admins retain access to the new platform Settings page, mirroring how
            // PermissionCatalog.All already includes it for newly-created orgs.
            migrationBuilder.Sql(@"
                UPDATE roles
                SET ""Permissions"" = ""Permissions"" || ',settings.manage'
                WHERE ""Name"" = 'Admin'
                  AND (',' || ""Permissions"" || ',') NOT LIKE '%,settings.manage,%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE roles
                SET ""Permissions"" = trim(both ',' from replace(',' || ""Permissions"" || ',', ',settings.manage,', ','))
                WHERE ""Name"" = 'Admin';
            ");
        }
    }
}
