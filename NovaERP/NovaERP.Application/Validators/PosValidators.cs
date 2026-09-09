using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class PosSaleStatuses
{
    public static readonly string[] All = { "Draft", "Completed", "Refunded", "Cancelled" };
}

public class CreatePosSaleLineDtoValidator : AbstractValidator<CreatePosSaleLineDto>
{
    public CreatePosSaleLineDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}

public class CreatePosSaleDtoValidator : AbstractValidator<CreatePosSaleDto>
{
    public CreatePosSaleDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.RevenueLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A POS sale needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreatePosSaleLineDtoValidator());
    }
}

public class UpdatePosSaleDtoValidator : AbstractValidator<UpdatePosSaleDto>
{
    public UpdatePosSaleDtoValidator()
    {
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.RevenueLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A POS sale needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreatePosSaleLineDtoValidator());
    }
}

public class CompletePosSaleDtoValidator : AbstractValidator<CompletePosSaleDto>
{
    public CompletePosSaleDtoValidator()
    {
        RuleFor(x => x.PaymentLedgerAccountId).GreaterThan(0);
    }
}
