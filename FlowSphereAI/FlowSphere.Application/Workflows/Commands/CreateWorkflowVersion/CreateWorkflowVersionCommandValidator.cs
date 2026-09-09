using System.Text.Json;
using FluentValidation;

namespace FlowSphere.Application.Workflows.Commands.CreateWorkflowVersion;

public class CreateWorkflowVersionCommandValidator : AbstractValidator<CreateWorkflowVersionCommand>
{
    public CreateWorkflowVersionCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId).GreaterThan(0);
        RuleFor(x => x.GraphJson)
            .NotEmpty()
            .Must(BeValidJson).WithMessage("GraphJson must be valid JSON.");
    }

    private static bool BeValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
