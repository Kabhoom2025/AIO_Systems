using FlowSphere.Application.Workflows.Commands.CreateWorkflow;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Workflows;

public class CreateWorkflowCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesWorkflow_ScopedToCallerOrganization()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 42, UserId = 7 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_CreatesWorkflow_ScopedToCallerOrganization));
        var handler = new CreateWorkflowCommandHandler(db, currentUser);

        var result = await handler.Handle(new CreateWorkflowCommand("Invoice Approval", "desc"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.WorkflowDefinitions.FindAsync(result.Value);
        Assert.NotNull(saved);
        Assert.Equal(42, saved!.OrganizationId);
        Assert.Equal("Invoice Approval", saved.Name);
    }

    [Fact]
    public async Task Handle_DoesNotLeakWorkflows_AcrossOrganizations()
    {
        var orgA = new FakeCurrentUserContext { OrganizationId = 1 };
        var orgB = new FakeCurrentUserContext { OrganizationId = 2 };

        const string dbName = nameof(Handle_DoesNotLeakWorkflows_AcrossOrganizations);
        await using (var dbA = TestDbContextFactory.Create(orgA, dbName))
        {
            await new CreateWorkflowCommandHandler(dbA, orgA)
                .Handle(new CreateWorkflowCommand("Org A Workflow", null), CancellationToken.None);
        }

        await using var dbB = TestDbContextFactory.Create(orgB, dbName);
        var orgBWorkflows = await Task.FromResult(dbB.WorkflowDefinitions.ToList());

        Assert.Empty(orgBWorkflows);
    }
}
