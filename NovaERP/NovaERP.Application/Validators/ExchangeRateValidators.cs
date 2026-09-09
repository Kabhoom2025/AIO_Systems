using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateExchangeRateDtoValidator : AbstractValidator<CreateExchangeRateDto>
{
    public CreateExchangeRateDtoValidator()
    {
        RuleFor(x => x.FromCurrencyCode).NotEmpty().Length(2, 3);
        RuleFor(x => x.ToCurrencyCode).NotEmpty().Length(2, 3);
        RuleFor(x => x.Rate).GreaterThan(0);
    }
}

public class UpdateExchangeRateDtoValidator : AbstractValidator<UpdateExchangeRateDto>
{
    public UpdateExchangeRateDtoValidator()
    {
        RuleFor(x => x.Rate).GreaterThan(0);
    }
}
