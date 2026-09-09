using FlowSphere.Application.Billing.Commands.ChangePlan;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Billing;

public class ChangePlanCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidPlan_UpdatesTierAndQuota()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 7 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ValidPlan_UpdatesTierAndQuota));
        db.Organizations.Add(new Organization { Id = 7, Name = "Acme Corp", PlanTier = "Free", MonthlyExecutionQuota = 100 });
        await db.SaveChangesAsync();

        var handler = new ChangePlanCommandHandler(db, currentUser);
        var result = await handler.Handle(new ChangePlanCommand("Pro"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var org = db.Organizations.Single();
        Assert.Equal("Pro", org.PlanTier);
        Assert.Equal(1000, org.MonthlyExecutionQuota);
    }

    [Fact]
    public async Task Handle_UnknownPlan_ReturnsNotFound()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 7 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_UnknownPlan_ReturnsNotFound));
        db.Organizations.Add(new Organization { Id = 7, Name = "Acme Corp" });
        await db.SaveChangesAsync();

        var handler = new ChangePlanCommandHandler(db, currentUser);
        var result = await handler.Handle(new ChangePlanCommand("Ultra"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
