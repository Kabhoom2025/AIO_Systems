using FlowSphere.Application.Apps.Commands.GenerateAppShareToken;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Apps;

public class GenerateAppShareTokenCommandHandlerTests
{
    private static async Task<(FlowSphere.Infrastructure.Data.FlowSphereDbContext Db, FakeCurrentUserContext CurrentUser, AppDefinition App)> SeedAsync(string dbName)
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 1 };
        var db = TestDbContextFactory.Create(currentUser, dbName);
        db.Organizations.Add(new Organization { Id = 1, Name = "Acme Corp", MonthlyExecutionQuota = 100 });

        var workspace = Workspace.Create(1, "Sales Ops", null, currentUser.UserId);
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync();

        var app = AppDefinition.Create(workspace.Id, "Lead Intake", null, currentUser.UserId);
        db.AppDefinitions.Add(app);
        await db.SaveChangesAsync();

        return (db, currentUser, app);
    }

    [Fact]
    public async Task Handle_Guest_GeneratesTokenAndEnablesGuestLink()
    {
        var (db, currentUser, app) = await SeedAsync(nameof(Handle_Guest_GeneratesTokenAndEnablesGuestLink));
        await using var _ = db;

        var handler = new GenerateAppShareTokenCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new GenerateAppShareTokenCommand(app.Id, "Guest"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!.Token);
        Assert.Contains(result.Value.Token, result.Value.SettingsJson);
        Assert.Contains("\"guestLinkEnabled\":true", result.Value.SettingsJson);
    }

    [Fact]
    public async Task Handle_Webhook_GeneratesDifferentTokenAndEnablesWebhook()
    {
        var (db, currentUser, app) = await SeedAsync(nameof(Handle_Webhook_GeneratesDifferentTokenAndEnablesWebhook));
        await using var _ = db;

        var handler = new GenerateAppShareTokenCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new GenerateAppShareTokenCommand(app.Id, "Webhook"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("\"webhookEnabled\":true", result.Value!.SettingsJson);
    }

    [Fact]
    public async Task Handle_InvalidKind_ReturnsValidationFailure()
    {
        var (db, currentUser, app) = await SeedAsync(nameof(Handle_InvalidKind_ReturnsValidationFailure));
        await using var _ = db;

        var handler = new GenerateAppShareTokenCommandHandler(db, currentUser, new FakeCurrentEnvironmentContext());
        var result = await handler.Handle(new GenerateAppShareTokenCommand(app.Id, "Nonsense"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
