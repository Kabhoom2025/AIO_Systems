using FluentValidation;

namespace FlowSphere.Application.Apps.Commands.UpdateAppDetails;

public class UpdateAppDetailsCommandValidator : AbstractValidator<UpdateAppDetailsCommand>
{
    public UpdateAppDetailsCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Icon).MaximumLength(50);
    }
}
