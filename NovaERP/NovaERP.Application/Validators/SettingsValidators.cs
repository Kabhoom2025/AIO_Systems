using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class UpdateOrganizationSettingsDtoValidator : AbstractValidator<UpdateOrganizationSettingsDto>
{
    public UpdateOrganizationSettingsDtoValidator()
    {
        RuleFor(x => x.DefaultLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(x => x.DefaultCurrencyCode).NotEmpty().Length(2, 3);
        RuleFor(x => x.DefaultTimezone).NotEmpty();
        RuleFor(x => x.DateFormat).NotEmpty();
        RuleFor(x => x.TimeFormat).NotEmpty();
        RuleFor(x => x.FiscalYearStartMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.InvoiceNumberPrefix).NotEmpty().MaximumLength(20);
    }
}
