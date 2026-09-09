using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Infrastructure.Notifications;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class AppTriggerDispatcherTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, AppDefinition Source, AppDefinition Destination)> SeedAsync(string dbName)
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, dbName);
        db.Organizations.Add(new Organization { Id = 1, Name = "Acme Corp", MonthlyExecutionQuota = 100 });

        var workspace = Workspace.Create(1, "HR", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var source = AppDefinition.Create(workspace.Id, "Employee Details", null, currentUser.UserId);
        source.UpdateFormSchema("""{"fields":[{"key":"empName","type":"Text","label":"Name"}]}""");
        db.AppDefinitions.Add(source);

        var destination = AppDefinition.Create(workspace.Id, "Salary Details", null, currentUser.UserId);
        destination.UpdateFormSchema("""{"fields":[{"key":"fullName","type":"Text","label":"Full Name"}]}""");
        db.AppDefinitions.Add(destination);

        await db.SaveChangesAsync();
        return (db, source, destination);
    }

    [Fact]
    public async Task Dispatch_SubmitAddRecordTrigger_CreatesMappedRecordInDestination()
    {
        var (db, source, destination) = await SeedAsync(nameof(Dispatch_SubmitAddRecordTrigger_CreatesMappedRecordInDestination));
        await using var _ = db;

        db.AppTriggers.Add(new AppTrigger
        {
            OrganizationId = 1,
            Name = "Employee to Salary",
            SourceAppId = source.Id,
            DestinationAppId = destination.Id,
            ActionType = AppTriggerActionType.SubmitAddRecord,
            FieldMappingJson = """{"empName":"fullName"}""",
            IsEnabled = true,
        });
        await db.SaveChangesAsync();

        var dispatcher = new AppTriggerDispatcher(db, new FakeExecutionQueue());
        await dispatcher.DispatchRecordSubmittedAsync(1, source.Id, EnvironmentStage.Dev, """{"empName":"Alice"}""", CancellationToken.None);

        var record = Assert.Single(db.AppRecords.Where(r => r.AppDefinitionId == destination.Id));
        Assert.Equal(AppRecordSource.Trigger, record.Source);
        Assert.Contains("\"fullName\":\"Alice\"", record.DataJson);
    }

    [Fact]
    public async Task Dispatch_DisabledTrigger_DoesNotFire()
    {
        var (db, source, destination) = await SeedAsync(nameof(Dispatch_DisabledTrigger_DoesNotFire));
        await using var _ = db;

        db.AppTriggers.Add(new AppTrigger
        {
            OrganizationId = 1,
            Name = "Disabled",
            SourceAppId = source.Id,
            DestinationAppId = destination.Id,
            ActionType = AppTriggerActionType.SubmitAddRecord,
            FieldMappingJson = """{"empName":"fullName"}""",
            IsEnabled = false,
        });
        await db.SaveChangesAsync();

        var dispatcher = new AppTriggerDispatcher(db, new FakeExecutionQueue());
        await dispatcher.DispatchRecordSubmittedAsync(1, source.Id, EnvironmentStage.Dev, """{"empName":"Alice"}""", CancellationToken.None);

        Assert.Empty(db.AppRecords.Where(r => r.AppDefinitionId == destination.Id));
    }

    [Fact]
    public async Task Dispatch_CreateTaskWithNoLinkedWorkflow_OnlyCreatesRecord()
    {
        var (db, source, destination) = await SeedAsync(nameof(Dispatch_CreateTaskWithNoLinkedWorkflow_OnlyCreatesRecord));
        await using var _ = db;

        db.AppTriggers.Add(new AppTrigger
        {
            OrganizationId = 1,
            Name = "Create Task",
            SourceAppId = source.Id,
            DestinationAppId = destination.Id,
            ActionType = AppTriggerActionType.CreateTask,
            FieldMappingJson = """{"empName":"fullName"}""",
            IsEnabled = true,
        });
        await db.SaveChangesAsync();

        var queue = new FakeExecutionQueue();
        var dispatcher = new AppTriggerDispatcher(db, queue);
        await dispatcher.DispatchRecordSubmittedAsync(1, source.Id, EnvironmentStage.Dev, """{"empName":"Alice"}""", CancellationToken.None);

        Assert.Single(db.AppRecords.Where(r => r.AppDefinitionId == destination.Id));
        Assert.Empty(queue.Enqueued);
        Assert.Empty(db.WorkflowExecutions);
    }
}
