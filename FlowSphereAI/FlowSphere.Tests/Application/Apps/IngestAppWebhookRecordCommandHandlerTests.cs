using FlowSphere.Application.Apps.Commands.IngestAppWebhookRecord;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class IngestAppWebhookRecordCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, AppDefinition App)> SeedAsync(string dbName, string settingsJson)
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, dbName);
        db.Organizations.Add(new Organization { Id = 1, Name = "Acme Corp", MonthlyExecutionQuota = 100 });

        var workspace = Workspace.Create(1, "Integrations", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Order Intake", null, currentUser.UserId);
        app.UpdateFormSchema("""{"fields":[{"key":"orderId","type":"Text","label":"Order Id"}]}""");
        app.UpdateSettings(settingsJson);
        app.Publish();
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        return (db, app);
    }

    [Fact]
    public async Task Handle_WrongApiKey_ReturnsUnauthorized()
    {
        var settings = """{"shared":{"webhookEnabled":true,"webhookApiKey":"correct-key"}}""";
        var (db, app) = await SeedAsync(nameof(Handle_WrongApiKey_ReturnsUnauthorized), settings);
        await using var _ = db;

        var handler = new IngestAppWebhookRecordCommandHandler(db, new FakeCurrentEnvironmentContext(), new FakeAppNotificationDispatcher());
        var result = await handler.Handle(new IngestAppWebhookRecordCommand(app.Id, "wrong-key", """{"orderId":"1"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(db.AppRecords);
    }

    [Fact]
    public async Task Handle_ValidApiKey_CreatesWebhookSourcedRecord()
    {
        var settings = """{"shared":{"webhookEnabled":true,"webhookApiKey":"correct-key"}}""";
        var (db, app) = await SeedAsync(nameof(Handle_ValidApiKey_CreatesWebhookSourcedRecord), settings);
        await using var _ = db;

        var handler = new IngestAppWebhookRecordCommandHandler(db, new FakeCurrentEnvironmentContext(), new FakeAppNotificationDispatcher());
        var result = await handler.Handle(new IngestAppWebhookRecordCommand(app.Id, "correct-key", """{"orderId":"1"}"""), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var record = Assert.Single(db.AppRecords);
        Assert.Equal(AppRecordSource.Webhook, record.Source);
        Assert.Null(record.CreatedByUserId);
    }

    [Fact]
    public async Task Handle_WebhookDisabled_ReturnsUnauthorized()
    {
        var (db, app) = await SeedAsync(nameof(Handle_WebhookDisabled_ReturnsUnauthorized), "{}");
        await using var _ = db;

        var handler = new IngestAppWebhookRecordCommandHandler(db, new FakeCurrentEnvironmentContext(), new FakeAppNotificationDispatcher());
        var result = await handler.Handle(new IngestAppWebhookRecordCommand(app.Id, "any-key", """{"orderId":"1"}"""), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(db.AppRecords);
    }
}
