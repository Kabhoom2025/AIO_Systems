using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class UpdateOrganizationLanguagesDtoValidator : AbstractValidator<UpdateOrganizationLanguagesDto>
{
    public UpdateOrganizationLanguagesDtoValidator()
    {
        RuleFor(x => x.LanguageCodes).NotEmpty();
        RuleFor(x => x.DefaultLanguageCode).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.LanguageCodes.Contains(x.DefaultLanguageCode))
            .WithMessage("DefaultLanguageCode must be one of the selected LanguageCodes.")
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultLanguageCode));
    }
}
