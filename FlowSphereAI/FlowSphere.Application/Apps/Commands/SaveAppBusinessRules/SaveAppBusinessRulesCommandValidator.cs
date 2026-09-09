using FluentValidation;

namespace FlowSphere.Application.Apps.Commands.SaveAppBusinessRules;

public class SaveAppBusinessRulesCommandValidator : AbstractValidator<SaveAppBusinessRulesCommand>
{
    public SaveAppBusinessRulesCommandValidator()
    {
        RuleFor(x => x.AppId).GreaterThan(0);
        RuleFor(x => x.BusinessRulesJson).NotEmpty();
    }
}
