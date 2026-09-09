using FluentValidation;

namespace FlowSphere.Application.Copilot.Commands.GenerateApp;

public class GenerateAppCommandValidator : AbstractValidator<GenerateAppCommand>
{
    public GenerateAppCommandValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(2000);
    }
}
