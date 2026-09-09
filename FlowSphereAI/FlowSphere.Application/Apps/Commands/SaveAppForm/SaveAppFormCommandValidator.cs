using FluentValidation;

namespace FlowSphere.Application.Apps.Commands.SaveAppForm;

public class SaveAppFormCommandValidator : AbstractValidator<SaveAppFormCommand>
{
    public SaveAppFormCommandValidator()
    {
        RuleFor(x => x.AppId).GreaterThan(0);
        RuleFor(x => x.FormSchemaJson).NotEmpty();
    }
}
