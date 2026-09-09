using FluentValidation;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateAutomationRuleDtoValidator : AbstractValidator<CreateAutomationRuleDto>
{
    public CreateAutomationRuleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TriggerEvent)
            .NotEmpty()
            .Must(e => AutomationEvents.All.Contains(e))
            .WithMessage($"TriggerEvent must be one of: {string.Join(", ", AutomationEvents.All)}");
        RuleFor(x => x.ActionType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NotifyMessageTemplate)
            .NotEmpty()
            .WithMessage("NotifyMessageTemplate is required when ActionType is 'Notify'.")
            .When(x => x.ActionType == "Notify");
    }
}

public class UpdateAutomationRuleDtoValidator : AbstractValidator<UpdateAutomationRuleDto>
{
    public UpdateAutomationRuleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TriggerEvent)
            .NotEmpty()
            .Must(e => AutomationEvents.All.Contains(e))
            .WithMessage($"TriggerEvent must be one of: {string.Join(", ", AutomationEvents.All)}");
        RuleFor(x => x.ActionType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NotifyMessageTemplate)
            .NotEmpty()
            .WithMessage("NotifyMessageTemplate is required when ActionType is 'Notify'.")
            .When(x => x.ActionType == "Notify");
    }
}
