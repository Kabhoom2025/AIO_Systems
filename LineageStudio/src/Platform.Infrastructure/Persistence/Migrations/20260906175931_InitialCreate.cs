using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "platform");

            migrationBuilder.CreateTable(
                name: "applications",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "application_versions",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_versions_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "platform",
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screens",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Route = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_screens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_screens_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "platform",
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    SchemaName = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tables_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "platform",
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lineage_executions",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineage_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lineage_executions_application_versions_VersionId",
                        column: x => x.VersionId,
                        principalSchema: "platform",
                        principalTable: "application_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lineage_executions_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "platform",
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "components",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScreenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PositionX = table.Column<double>(type: "double precision", nullable: false),
                    PositionY = table.Column<double>(type: "double precision", nullable: false),
                    PropertiesJson = table.Column<string>(type: "jsonb", nullable: true),
                    ValidationJson = table.Column<string>(type: "jsonb", nullable: true),
                    DataBinding = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EventsJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_components_screens_ScreenId",
                        column: x => x.ScreenId,
                        principalSchema: "platform",
                        principalTable: "screens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "columns",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TableId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: true),
                    IsPrimaryKey = table.Column<bool>(type: "boolean", nullable: false),
                    IsForeignKey = table.Column<bool>(type: "boolean", nullable: false),
                    ReferencesTableId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferencesColumnId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsUnique = table.Column<bool>(type: "boolean", nullable: false),
                    IsIndexed = table.Column<bool>(type: "boolean", nullable: false),
                    IsNullable = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrdinalPosition = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_columns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_columns_tables_ReferencesTableId",
                        column: x => x.ReferencesTableId,
                        principalSchema: "platform",
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_columns_tables_TableId",
                        column: x => x.TableId,
                        principalSchema: "platform",
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TableId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_services_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "platform",
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_services_tables_TableId",
                        column: x => x.TableId,
                        principalSchema: "platform",
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "lineage_events",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    NodeId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NodeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EventType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineage_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lineage_events_lineage_events_ParentEventId",
                        column: x => x.ParentEventId,
                        principalSchema: "platform",
                        principalTable: "lineage_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lineage_events_lineage_executions_ExecutionId",
                        column: x => x.ExecutionId,
                        principalSchema: "platform",
                        principalTable: "lineage_executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apis",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequestSchemaJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResponseSchemaJson = table.Column<string>(type: "jsonb", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    TableId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_apis_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "platform",
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apis_services_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "platform",
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_apis_tables_TableId",
                        column: x => x.TableId,
                        principalSchema: "platform",
                        principalTable: "tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "mappings",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ColumnId = table.Column<Guid>(type: "uuid", nullable: false),
                    Transformation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TransformationConfigJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mappings_apis_ApiId",
                        column: x => x.ApiId,
                        principalSchema: "platform",
                        principalTable: "apis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mappings_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "platform",
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mappings_columns_ColumnId",
                        column: x => x.ColumnId,
                        principalSchema: "platform",
                        principalTable: "columns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mappings_components_SourceComponentId",
                        column: x => x.SourceComponentId,
                        principalSchema: "platform",
                        principalTable: "components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mappings_services_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "platform",
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apis_ApplicationId_Method_Path",
                schema: "platform",
                table: "apis",
                columns: new[] { "ApplicationId", "Method", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_apis_ServiceId",
                schema: "platform",
                table: "apis",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_apis_TableId",
                schema: "platform",
                table: "apis",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_application_versions_ApplicationId_VersionNumber",
                schema: "platform",
                table: "application_versions",
                columns: new[] { "ApplicationId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applications_Name",
                schema: "platform",
                table: "applications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_columns_ReferencesTableId",
                schema: "platform",
                table: "columns",
                column: "ReferencesTableId");

            migrationBuilder.CreateIndex(
                name: "IX_columns_TableId_Name",
                schema: "platform",
                table: "columns",
                columns: new[] { "TableId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_components_ScreenId",
                schema: "platform",
                table: "components",
                column: "ScreenId");

            migrationBuilder.CreateIndex(
                name: "IX_lineage_events_ExecutionId",
                schema: "platform",
                table: "lineage_events",
                column: "ExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_lineage_events_ParentEventId",
                schema: "platform",
                table: "lineage_events",
                column: "ParentEventId");

            migrationBuilder.CreateIndex(
                name: "IX_lineage_executions_ApplicationId",
                schema: "platform",
                table: "lineage_executions",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_lineage_executions_CorrelationId",
                schema: "platform",
                table: "lineage_executions",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_lineage_executions_StartedAt",
                schema: "platform",
                table: "lineage_executions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_lineage_executions_VersionId",
                schema: "platform",
                table: "lineage_executions",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_mappings_ApiId",
                schema: "platform",
                table: "mappings",
                column: "ApiId");

            migrationBuilder.CreateIndex(
                name: "IX_mappings_ApplicationId",
                schema: "platform",
                table: "mappings",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_mappings_ColumnId",
                schema: "platform",
                table: "mappings",
                column: "ColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_mappings_ServiceId",
                schema: "platform",
                table: "mappings",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_mappings_SourceComponentId",
                schema: "platform",
                table: "mappings",
                column: "SourceComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_screens_ApplicationId_Route",
                schema: "platform",
                table: "screens",
                columns: new[] { "ApplicationId", "Route" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_ApplicationId_Name",
                schema: "platform",
                table: "services",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_services_TableId",
                schema: "platform",
                table: "services",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_tables_ApplicationId_Name",
                schema: "platform",
                table: "tables",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lineage_events",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "mappings",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "lineage_executions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "apis",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "columns",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "components",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "application_versions",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "services",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "screens",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "tables",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "applications",
                schema: "platform");
        }
    }
}
