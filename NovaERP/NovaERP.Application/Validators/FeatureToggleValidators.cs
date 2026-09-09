using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class UpdateFeatureToggleDtoValidator : AbstractValidator<UpdateFeatureToggleDto>
{
    public UpdateFeatureToggleDtoValidator()
    {
        RuleFor(x => x.ModuleKey).NotEmpty().MaximumLength(50);
    }
}
