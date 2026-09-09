using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIO_Systems.Migrations
{
    /// <inheritdoc />
    public partial class AddDockerServiceNameToRegisteredService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DockerServiceName",
                table: "RegisteredServices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DockerServiceName",
                table: "RegisteredServices");
        }
    }
}
