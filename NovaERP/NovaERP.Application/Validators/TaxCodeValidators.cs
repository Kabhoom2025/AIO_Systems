using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateTaxComponentDtoValidator : AbstractValidator<CreateTaxComponentDto>
{
    public CreateTaxComponentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RatePercent).GreaterThanOrEqualTo(0);
    }
}

public class CreateTaxCodeDtoValidator : AbstractValidator<CreateTaxCodeDto>
{
    public CreateTaxCodeDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Components).NotEmpty().WithMessage("A tax code needs at least one rate component.");
        RuleForEach(x => x.Components).SetValidator(new CreateTaxComponentDtoValidator());
    }
}

public class UpdateTaxCodeDtoValidator : AbstractValidator<UpdateTaxCodeDto>
{
    public UpdateTaxCodeDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Components).NotEmpty().WithMessage("A tax code needs at least one rate component.");
        RuleForEach(x => x.Components).SetValidator(new CreateTaxComponentDtoValidator());
    }
}
