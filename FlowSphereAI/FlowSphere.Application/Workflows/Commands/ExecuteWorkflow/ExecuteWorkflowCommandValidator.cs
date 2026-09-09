using FluentValidation;

namespace FlowSphere.Application.Workflows.Commands.ExecuteWorkflow;

public class ExecuteWorkflowCommandValidator : AbstractValidator<ExecuteWorkflowCommand>
{
    public ExecuteWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId).GreaterThan(0);
    }
}
