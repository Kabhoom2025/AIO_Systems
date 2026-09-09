using FluentValidation;

namespace FlowSphere.Application.Workflows.Commands.PublishWorkflowVersion;

public class PublishWorkflowVersionCommandValidator : AbstractValidator<PublishWorkflowVersionCommand>
{
    public PublishWorkflowVersionCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId).GreaterThan(0);
        RuleFor(x => x.VersionId).GreaterThan(0);
    }
}
