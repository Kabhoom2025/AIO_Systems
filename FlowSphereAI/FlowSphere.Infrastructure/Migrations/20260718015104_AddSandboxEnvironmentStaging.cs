using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSandboxEnvironmentStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "workflow_executions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "table_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "app_records",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PromotedFromAppDefinitionId",
                table: "app_definitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceGroupId",
                table: "app_definitions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "app_definitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "workflow_stage_deployments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkflowDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    WorkflowVersionId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_stage_deployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_stage_deployments_workflow_definitions_WorkflowDef~",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_stage_deployments_workflow_versions_WorkflowVersio~",
                        column: x => x.WorkflowVersionId,
                        principalTable: "workflow_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill before the unique (SourceGroupId, Stage) index below - every existing
            // app_definitions row currently shares the same zero-Guid default, which would
            // violate that uniqueness constraint the moment a second row exists. Existing rows
            // (this week's dev/demo data) are treated as already-real, so they land on Live
            // rather than Dev - matches the equivalent backfill for the runtime-data tables.
            migrationBuilder.Sql("UPDATE app_definitions SET \"SourceGroupId\" = gen_random_uuid();");
            migrationBuilder.Sql("UPDATE app_definitions SET \"Stage\" = 3;");
            migrationBuilder.Sql("UPDATE app_records SET \"Stage\" = 3;");
            migrationBuilder.Sql("UPDATE table_records SET \"Stage\" = 3;");
            migrationBuilder.Sql("UPDATE workflow_executions SET \"Stage\" = 3;");

            migrationBuilder.CreateIndex(
                name: "IX_app_definitions_SourceGroupId_Stage",
                table: "app_definitions",
                columns: new[] { "SourceGroupId", "Stage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_definitions_WorkspaceId_Stage",
                table: "app_definitions",
                columns: new[] { "WorkspaceId", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_stage_deployments_WorkflowDefinitionId_Stage",
                table: "workflow_stage_deployments",
                columns: new[] { "WorkflowDefinitionId", "Stage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_stage_deployments_WorkflowVersionId",
                table: "workflow_stage_deployments",
                column: "WorkflowVersionId");

            // Without this, every already-published workflow (e.g. an app's linked approval
            // workflow that was working fine before this migration) would suddenly fail with
            // "no version deployed to the Live stage" the moment SubmitAppCommandHandler starts
            // resolving versions through WorkflowStageDeployment instead of the old global
            // VersionStatus.Published flag. Treat "was Published" as "is deployed to Live" for
            // every pre-existing version, preserving current behavior exactly.
            migrationBuilder.Sql(
                "INSERT INTO workflow_stage_deployments (\"WorkflowDefinitionId\", \"Stage\", \"WorkflowVersionId\", \"CreatedDate\", \"UpdatedDate\") " +
                "SELECT wv.\"WorkflowDefinitionId\", 3, wv.\"Id\", now(), now() FROM workflow_versions wv WHERE wv.\"Status\" = 'Published';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_stage_deployments");

            migrationBuilder.DropIndex(
                name: "IX_app_definitions_SourceGroupId_Stage",
                table: "app_definitions");

            migrationBuilder.DropIndex(
                name: "IX_app_definitions_WorkspaceId_Stage",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "workflow_executions");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "table_records");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "app_records");

            migrationBuilder.DropColumn(
                name: "PromotedFromAppDefinitionId",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "SourceGroupId",
                table: "app_definitions");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "app_definitions");
        }
    }
}
