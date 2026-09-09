using FlowSphere.Application.Observability.Queries.GetExecutionMetrics;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using FlowSphere.Tests.TestUtilities;

namespace FlowSphere.Tests.Application.Observability;

public class GetExecutionMetricsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesSuccessRateAndAverageDuration()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 6 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ComputesSuccessRateAndAverageDuration));

        var now = DateTime.UtcNow;
        db.WorkflowExecutions.AddRange(
            new WorkflowExecution
            {
                OrganizationId = 6, WorkflowDefinitionId = 1, WorkflowVersionId = 1,
                Status = ExecutionStatus.Succeeded, CreatedDate = now,
                StartedAt = now, CompletedAt = now.AddMilliseconds(100),
            },
            new WorkflowExecution
            {
                OrganizationId = 6, WorkflowDefinitionId = 1, WorkflowVersionId = 1,
                Status = ExecutionStatus.Succeeded, CreatedDate = now,
                StartedAt = now, CompletedAt = now.AddMilliseconds(300),
            },
            new WorkflowExecution
            {
                OrganizationId = 6, WorkflowDefinitionId = 1, WorkflowVersionId = 1,
                Status = ExecutionStatus.Failed, CreatedDate = now,
                StartedAt = now, CompletedAt = now.AddMilliseconds(200),
            },
            new WorkflowExecution
            {
                OrganizationId = 6, WorkflowDefinitionId = 1, WorkflowVersionId = 1,
                Status = ExecutionStatus.Running, CreatedDate = now,
            });
        await db.SaveChangesAsync();

        var handler = new GetExecutionMetricsQueryHandler(db);
        var result = await handler.Handle(new GetExecutionMetricsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.TotalLast24h);
        Assert.Equal(2, result.Value.SucceededLast24h);
        Assert.Equal(1, result.Value.FailedLast24h);
        Assert.Equal(1, result.Value.RunningOrQueuedNow);
        Assert.Equal(50.0, result.Value.SuccessRatePercent);
        Assert.Equal(200.0, result.Value.AverageDurationMs); // (100 + 300 + 200) / 3
    }

    [Fact]
    public async Task Handle_NoExecutions_ReturnsFullSuccessRateAndNullDuration()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 6 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_NoExecutions_ReturnsFullSuccessRateAndNullDuration));

        var handler = new GetExecutionMetricsQueryHandler(db);
        var result = await handler.Handle(new GetExecutionMetricsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalLast24h);
        Assert.Equal(100.0, result.Value.SuccessRatePercent);
        Assert.Null(result.Value.AverageDurationMs);
    }

    [Fact]
    public async Task Handle_ExcludesExecutionsOlderThan24Hours()
    {
        var currentUser = new FakeCurrentUserContext { OrganizationId = 6 };
        await using var db = TestDbContextFactory.Create(currentUser, nameof(Handle_ExcludesExecutionsOlderThan24Hours));

        db.WorkflowExecutions.Add(new WorkflowExecution
        {
            OrganizationId = 6,
            WorkflowDefinitionId = 1,
            WorkflowVersionId = 1,
            Status = ExecutionStatus.Succeeded,
            CreatedDate = DateTime.UtcNow.AddHours(-48),
        });
        await db.SaveChangesAsync();

        var handler = new GetExecutionMetricsQueryHandler(db);
        var result = await handler.Handle(new GetExecutionMetricsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalLast24h);
    }
}
