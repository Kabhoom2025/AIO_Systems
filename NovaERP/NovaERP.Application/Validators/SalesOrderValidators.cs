using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateSalesOrderLineDtoValidator : AbstractValidator<CreateSalesOrderLineDto>
{
    public CreateSalesOrderLineDtoValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}

public class CreateSalesOrderDtoValidator : AbstractValidator<CreateSalesOrderDto>
{
    public CreateSalesOrderDtoValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A sales order needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateSalesOrderLineDtoValidator());
    }
}

public class UpdateSalesOrderDtoValidator : AbstractValidator<UpdateSalesOrderDto>
{
    public UpdateSalesOrderDtoValidator()
    {
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A sales order needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateSalesOrderLineDtoValidator());
    }
}
