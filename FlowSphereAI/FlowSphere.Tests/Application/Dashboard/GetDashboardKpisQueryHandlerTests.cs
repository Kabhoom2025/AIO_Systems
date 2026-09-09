using FlowSphere.Application.Dashboard.Queries.GetDashboardKpis;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Dashboard;

public class GetDashboardKpisQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesCountsScopedToCallerOrganization()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 4 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ComputesCountsScopedToCallerOrganization));

        db.Organizations.Add(new Organization { Id = 4, Name = "Acme Corp", PlanTier = "Free", MonthlyExecutionQuota = 100 });
        var workflow = WorkflowDefinition.Create(4, "WF1", null, 1);
        db.WorkflowDefinitions.Add(workflow);
        await db.SaveChangesAsync();

        db.WorkflowExecutions.AddRange(
            new WorkflowExecution { OrganizationId = 4, WorkflowDefinitionId = workflow.Id, WorkflowVersionId = 1, Status = ExecutionStatus.Succeeded, CreatedDate = DateTime.UtcNow },
            new WorkflowExecution { OrganizationId = 4, WorkflowDefinitionId = workflow.Id, WorkflowVersionId = 1, Status = ExecutionStatus.Succeeded, CreatedDate = DateTime.UtcNow },
            new WorkflowExecution { OrganizationId = 4, WorkflowDefinitionId = workflow.Id, WorkflowVersionId = 1, Status = ExecutionStatus.Failed, CreatedDate = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var handler = new GetDashboardKpisQueryHandler(db, currentUser, new FakeDistributedCache());
        var result = await handler.Handle(new GetDashboardKpisQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalWorkflows);
        Assert.Equal(2, result.Value.SucceededCount);
        Assert.Equal(1, result.Value.FailedCount);
        Assert.Equal(3, result.Value.ExecutionsThisMonth);
        Assert.Equal("Free", result.Value.PlanTier);
        Assert.Equal(100, result.Value.MonthlyExecutionQuota);
    }

    [Fact]
    public async Task Handle_DoesNotCountOtherOrganizationsExecutions()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 4 };
        const string dbName = nameof(Handle_DoesNotCountOtherOrganizationsExecutions);
        await using var db = TestDbContextFactory.Create(currentUser, dbName);

        db.Organizations.Add(new Organization { Id = 4, Name = "Acme Corp" });
        await db.SaveChangesAsync();

        // Seed an execution for a DIFFERENT organization using its own scoped context so the
        // tenant filter actually applies at write time too.
        var otherOrgUser = new FakeCurrentUserContext { OrganizationId = 99 };
        await using (var otherDb = TestDbContextFactory.Create(otherOrgUser, dbName))
        {
            otherDb.WorkflowExecutions.Add(new WorkflowExecution
            {
                OrganizationId = 99,
                WorkflowDefinitionId = 1,
                WorkflowVersionId = 1,
                Status = ExecutionStatus.Succeeded,
                CreatedDate = DateTime.UtcNow,
            });
            await otherDb.SaveChangesAsync();
        }

        var handler = new GetDashboardKpisQueryHandler(db, currentUser, new FakeDistributedCache());
        var result = await handler.Handle(new GetDashboardKpisQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.SucceededCount);
        Assert.Equal(0, result.Value.ExecutionsThisMonth);
    }
}
