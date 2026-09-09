using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreatePurchaseOrderLineDtoValidator : AbstractValidator<CreatePurchaseOrderLineDto>
{
    public CreatePurchaseOrderLineDtoValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}

public class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A purchase order needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreatePurchaseOrderLineDtoValidator());
    }
}

public class UpdatePurchaseOrderDtoValidator : AbstractValidator<UpdatePurchaseOrderDto>
{
    public UpdatePurchaseOrderDtoValidator()
    {
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A purchase order needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreatePurchaseOrderLineDtoValidator());
    }
}
