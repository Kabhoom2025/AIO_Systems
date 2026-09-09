using ProjectFlowAI.Application.Features.Automation;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class WorkflowEngineConditionTests
{
    private static WorkItem MakeItem(WorkItemPriority priority = WorkItemPriority.Medium,
        WorkItemStatus status = WorkItemStatus.Backlog, Guid? assigneeUserId = null, int? storyPoints = null)
        => new()
        {
            Title = "Test item",
            Priority = priority,
            Status = status,
            AssigneeUserId = assigneeUserId,
            StoryPoints = storyPoints,
            ReporterUserId = Guid.NewGuid()
        };

    [Fact]
    public void Equals_Condition_Matches_When_Field_Value_Equals_Expected()
    {
        var item = MakeItem(priority: WorkItemPriority.High);
        var condition = new WorkflowCondition { FieldPath = "Priority", Operator = WorkflowConditionOperator.Equals, Value = "High" };

        Assert.True(WorkflowEngine.EvaluateCondition(condition, item));
    }

    [Fact]
    public void Equals_Condition_Does_Not_Match_When_Field_Value_Differs()
    {
        var item = MakeItem(priority: WorkItemPriority.Low);
        var condition = new WorkflowCondition { FieldPath = "Priority", Operator = WorkflowConditionOperator.Equals, Value = "High" };

        Assert.False(WorkflowEngine.EvaluateCondition(condition, item));
    }

    [Fact]
    public void NotEquals_Condition_Matches_When_Field_Value_Differs()
    {
        var item = MakeItem(status: WorkItemStatus.Blocked);
        var condition = new WorkflowCondition { FieldPath = "Status", Operator = WorkflowConditionOperator.NotEquals, Value = "Done" };

        Assert.True(WorkflowEngine.EvaluateCondition(condition, item));
    }

    [Fact]
    public void GreaterThan_Condition_Compares_Numeric_Values()
    {
        var item = MakeItem(storyPoints: 8);
        var condition = new WorkflowCondition { FieldPath = "StoryPoints", Operator = WorkflowConditionOperator.GreaterThan, Value = "5" };

        Assert.True(WorkflowEngine.EvaluateCondition(condition, item));
    }

    [Fact]
    public void LessThan_Condition_Compares_Numeric_Values()
    {
        var item = MakeItem(storyPoints: 2);
        var condition = new WorkflowCondition { FieldPath = "StoryPoints", Operator = WorkflowConditionOperator.LessThan, Value = "5" };

        Assert.True(WorkflowEngine.EvaluateCondition(condition, item));
    }

    [Fact]
    public void Contains_Condition_Is_Case_Insensitive_Substring_Match()
    {
        var item = MakeItem(status: WorkItemStatus.InProgress);
        var condition = new WorkflowCondition { FieldPath = "Status", Operator = WorkflowConditionOperator.Contains, Value = "progress" };

        Assert.True(WorkflowEngine.EvaluateCondition(condition, item));
    }

    [Fact]
    public void EvaluateConditions_Requires_ALL_Conditions_To_Pass_Ands_Them_Together()
    {
        var item = MakeItem(priority: WorkItemPriority.High, status: WorkItemStatus.ToDo);
        var conditions = new List<WorkflowCondition>
        {
            new() { FieldPath = "Priority", Operator = WorkflowConditionOperator.Equals, Value = "High" },
            new() { FieldPath = "Status", Operator = WorkflowConditionOperator.Equals, Value = "ToDo" }
        };

        Assert.True(WorkflowEngine.EvaluateConditions(conditions, item));
    }

    [Fact]
    public void EvaluateConditions_Fails_If_Any_Single_Condition_Fails()
    {
        var item = MakeItem(priority: WorkItemPriority.High, status: WorkItemStatus.Backlog);
        var conditions = new List<WorkflowCondition>
        {
            new() { FieldPath = "Priority", Operator = WorkflowConditionOperator.Equals, Value = "High" },
            // This one fails — status is Backlog, not ToDo — so the AND must fail overall.
            new() { FieldPath = "Status", Operator = WorkflowConditionOperator.Equals, Value = "ToDo" }
        };

        Assert.False(WorkflowEngine.EvaluateConditions(conditions, item));
    }

    [Fact]
    public void EvaluateConditions_With_No_Conditions_Passes_Vacuously()
    {
        var item = MakeItem();
        Assert.True(WorkflowEngine.EvaluateConditions(new List<WorkflowCondition>(), item));
    }

    [Fact]
    public void AssigneeUserId_Condition_Matches_By_Guid_String()
    {
        var userId = Guid.NewGuid();
        var item = MakeItem(assigneeUserId: userId);
        var condition = new WorkflowCondition { FieldPath = "AssigneeUserId", Operator = WorkflowConditionOperator.Equals, Value = userId.ToString() };

        Assert.True(WorkflowEngine.EvaluateCondition(condition, item));
    }
}
