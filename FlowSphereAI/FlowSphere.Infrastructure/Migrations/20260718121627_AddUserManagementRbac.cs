using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagementRbac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "users",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "workspace_roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Permissions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspace_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workspace_roles_workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workspace_role_assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WorkspaceRoleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspace_role_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workspace_role_assignments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workspace_role_assignments_workspace_roles_WorkspaceRoleId",
                        column: x => x.WorkspaceRoleId,
                        principalTable: "workspace_roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workspace_role_assignments_workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_ManagerId",
                table: "users",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_role_assignments_UserId",
                table: "workspace_role_assignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_role_assignments_WorkspaceId_UserId_WorkspaceRole~",
                table: "workspace_role_assignments",
                columns: new[] { "WorkspaceId", "UserId", "WorkspaceRoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workspace_role_assignments_WorkspaceRoleId",
                table: "workspace_role_assignments",
                column: "WorkspaceRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_roles_WorkspaceId_Name",
                table: "workspace_roles",
                columns: new[] { "WorkspaceId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_users_users_ManagerId",
                table: "users",
                column: "ManagerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_users_ManagerId",
                table: "users");

            migrationBuilder.DropTable(
                name: "workspace_role_assignments");

            migrationBuilder.DropTable(
                name: "workspace_roles");

            migrationBuilder.DropIndex(
                name: "IX_users_ManagerId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "users");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
