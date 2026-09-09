using FluentValidation;

namespace FlowSphere.Application.Copilot.Commands.GenerateWorkflowGraph;

public class GenerateWorkflowGraphCommandValidator : AbstractValidator<GenerateWorkflowGraphCommand>
{
    public GenerateWorkflowGraphCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(2000);
    }
}
