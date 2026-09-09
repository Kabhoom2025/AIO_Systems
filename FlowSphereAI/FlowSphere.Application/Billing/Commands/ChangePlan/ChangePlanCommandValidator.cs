using FlowSphere.Domain.Common;
using FluentValidation;

namespace FlowSphere.Application.Billing.Commands.ChangePlan;

public class ChangePlanCommandValidator : AbstractValidator<ChangePlanCommand>
{
    public ChangePlanCommandValidator()
    {
        RuleFor(x => x.PlanName)
            .NotEmpty()
            .Must(name => PlanCatalog.Find(name) is not null)
            .WithMessage($"Plan must be one of: {string.Join(", ", PlanCatalog.All.Select(p => p.Name))}.");
    }
}
