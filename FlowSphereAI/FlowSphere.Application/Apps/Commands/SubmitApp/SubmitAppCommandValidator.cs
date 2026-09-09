using FluentValidation;

namespace FlowSphere.Application.Apps.Commands.SubmitApp;

public class SubmitAppCommandValidator : AbstractValidator<SubmitAppCommand>
{
    public SubmitAppCommandValidator()
    {
        RuleFor(x => x.AppId).GreaterThan(0);
        RuleFor(x => x.DataJson).NotEmpty();
    }
}
