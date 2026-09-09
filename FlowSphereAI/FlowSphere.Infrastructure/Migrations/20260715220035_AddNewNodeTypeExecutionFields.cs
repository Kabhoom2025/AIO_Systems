using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewNodeTypeExecutionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingNodeKey",
                table: "workflow_executions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingResumeHandle",
                table: "workflow_executions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingResumeInputJson",
                table: "workflow_executions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingNodeKey",
                table: "workflow_executions");

            migrationBuilder.DropColumn(
                name: "PendingResumeHandle",
                table: "workflow_executions");

            migrationBuilder.DropColumn(
                name: "PendingResumeInputJson",
                table: "workflow_executions");
        }
    }
}
