using FlowSphere.Application.Apps.Commands.SubmitApp;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class SubmitAppCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, Workspace Workspace, AppDefinition App)> SeedAsync(
        FakeCurrentUserContext currentUser, string dbName)
    {
        var db = TestDbContextFactory.Create(currentUser, dbName);
        db.Organizations.Add(new Organization { Id = currentUser.OrganizationId, Name = "Acme Corp", MonthlyExecutionQuota = 100 });

        var workspace = Workspace.Create(currentUser.OrganizationId, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, currentUser.UserId);
        app.UpdateFormSchema("""{"fields":[{"key":"name","type":"Text","label":"Name","required":true}]}""");
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        return (db, workspace, app);
    }

    [Fact]
    public async Task Handle_TestSubmit_DoesNotPersistRecord()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_TestSubmit_DoesNotPersistRecord));
        await using var _ = db;

        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), new FakeExecutionQueue(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, """{"name":"Alice"}""", true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.RecordId);
        Assert.Empty(db.AppRecords);
    }

    [Fact]
    public async Task Handle_LaunchSubmit_UnpublishedApp_ReturnsValidationFailure()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_LaunchSubmit_UnpublishedApp_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), new FakeExecutionQueue(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, """{"name":"Alice"}""", false), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Empty(db.AppRecords);
    }

    [Fact]
    public async Task Handle_LaunchSubmit_PublishedApp_PersistsAppRecord()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1, UserId = 5 };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_LaunchSubmit_PublishedApp_PersistsAppRecord));
        await using var _ = db;
        app.Publish();
        await db.SaveChangesAsync();

        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), new FakeExecutionQueue(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, """{"name":"Alice"}""", false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.RecordId);
        var record = Assert.Single(db.AppRecords);
        Assert.Equal("""{"name":"Alice"}""", record.DataJson);
        Assert.Equal(5, record.CreatedByUserId);
    }

    [Fact]
    public async Task Handle_LaunchSubmit_WithLinkedTable_WritesMappedTableRecord()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace, app) = await SeedAsync(currentUser, nameof(Handle_LaunchSubmit_WithLinkedTable_WritesMappedTableRecord));
        await using var _ = db;

        var table = TableDefinition.Create(workspace.Id, "Leads", null, currentUser.UserId);
        db.TableDefinitions.Add(table);
        await db.SaveChangesAsync();

        app.LinkTable(table.Id, """{"name":"fullName"}""");
        app.Publish();
        await db.SaveChangesAsync();

        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), new FakeExecutionQueue(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, """{"name":"Alice"}""", false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tableRecord = Assert.Single(db.TableRecords);
        Assert.Equal(table.Id, tableRecord.TableDefinitionId);
        Assert.Contains("\"fullName\":\"Alice\"", tableRecord.DataJson);
    }

    [Fact]
    public async Task Handle_LaunchSubmit_WithLinkedWorkflow_EnqueuesExecution()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_LaunchSubmit_WithLinkedWorkflow_EnqueuesExecution));
        await using var _ = db;

        var workflow = WorkflowDefinition.Create(currentUser.OrganizationId, "On Submit", null, currentUser.UserId);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();
        var version = new FlowSphere.Domain.Entities.WorkflowVersion
        {
            WorkflowDefinitionId = workflow.Id,
            VersionNumber = 1,
            Status = VersionStatus.Published,
            PublishedAt = DateTime.UtcNow,
        };
        db.WorkflowVersions.Add(version);
        await db.SaveChangesAsync();
        db.WorkflowStageDeployments.Add(new WorkflowStageDeployment
        {
            WorkflowDefinitionId = workflow.Id,
            Stage = EnvironmentStage.Dev,
            WorkflowVersionId = version.Id,
        });
        await db.SaveChangesAsync();

        app.LinkWorkflow(workflow.Id);
        app.Publish();
        await db.SaveChangesAsync();

        var queue = new FakeExecutionQueue();
        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), queue, new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, """{"name":"Alice"}""", false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.WorkflowExecutionId);
        Assert.Single(queue.Enqueued);
    }

    [Fact]
    public async Task Handle_SectionHasOwnWorkflow_UsesSectionWorkflowNotAppLevel()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, _, app) = await SeedAsync(currentUser, nameof(Handle_SectionHasOwnWorkflow_UsesSectionWorkflowNotAppLevel));
        await using var _ = db;

        var sectionWorkflow = WorkflowDefinition.Create(currentUser.OrganizationId, "Section Workflow", null, currentUser.UserId);
        db.WorkflowDefinitions.Add(sectionWorkflow);
        await db.SaveChangesAsync();
        var sectionVersion = new FlowSphere.Domain.Entities.WorkflowVersion
        {
            WorkflowDefinitionId = sectionWorkflow.Id,
            VersionNumber = 1,
            Status = VersionStatus.Published,
            PublishedAt = DateTime.UtcNow,
        };
        db.WorkflowVersions.Add(sectionVersion);
        await db.SaveChangesAsync();
        db.WorkflowStageDeployments.Add(new WorkflowStageDeployment
        {
            WorkflowDefinitionId = sectionWorkflow.Id,
            Stage = EnvironmentStage.Dev,
            WorkflowVersionId = sectionVersion.Id,
        });
        await db.SaveChangesAsync();

        // No app-level workflow linked at all - only the module (section) has its own.
        app.UpdateFormSchema($$"""
            {"sections":[{"id":"leave","title":"Leave Request","fields":[],"workflowDefinitionId":{{sectionWorkflow.Id}}}]}
            """);
        app.Publish();
        await db.SaveChangesAsync();

        var queue = new FakeExecutionQueue();
        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), queue, new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, "{}", false, "leave"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.WorkflowExecutionId);
        Assert.Single(queue.Enqueued);
    }

    [Fact]
    public async Task Handle_SectionHasOwnTable_WritesToSectionTableNotAppLevel()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace, app) = await SeedAsync(currentUser, nameof(Handle_SectionHasOwnTable_WritesToSectionTableNotAppLevel));
        await using var _ = db;

        var table = TableDefinition.Create(workspace.Id, "Employees", null, currentUser.UserId);
        db.TableDefinitions.Add(table);
        await db.SaveChangesAsync();

        app.UpdateFormSchema($$"""
            {"sections":[{"id":"onboarding","title":"Onboarding","fields":[],"linkedTableId":{{table.Id}},"fieldMappingJson":"{\"name\":\"fullName\"}"}]}
            """);
        app.Publish();
        await db.SaveChangesAsync();

        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), new FakeExecutionQueue(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, """{"name":"Priya"}""", false, "onboarding"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tableRecord = Assert.Single(db.TableRecords);
        Assert.Equal(table.Id, tableRecord.TableDefinitionId);
        Assert.Contains("\"fullName\":\"Priya\"", tableRecord.DataJson);
    }

    [Fact]
    public async Task Handle_NoSectionIdProvided_FallsBackToAppLevelLinks()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var (db, workspace, app) = await SeedAsync(currentUser, nameof(Handle_NoSectionIdProvided_FallsBackToAppLevelLinks));
        await using var _ = db;

        var table = TableDefinition.Create(workspace.Id, "Leads", null, currentUser.UserId);
        db.TableDefinitions.Add(table);
        await db.SaveChangesAsync();

        app.LinkTable(table.Id, """{"name":"fullName"}""");
        app.Publish();
        await db.SaveChangesAsync();

        var handler = new SubmitAppCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext(), new FakeExecutionQueue(), new FakeCaptchaService(), new FakePasswordHasher(), new FakeAppNotificationDispatcher(), new FakeAppTriggerDispatcher());
        var result = await handler.Handle(new SubmitAppCommand(app.Id, """{"name":"Alice"}""", false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tableRecord = Assert.Single(db.TableRecords);
        Assert.Equal(table.Id, tableRecord.TableDefinitionId);
    }
}
