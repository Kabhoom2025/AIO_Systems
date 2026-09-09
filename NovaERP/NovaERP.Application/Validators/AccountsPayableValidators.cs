using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateVendorBillLineDtoValidator : AbstractValidator<CreateVendorBillLineDto>
{
    public CreateVendorBillLineDtoValidator()
    {
        RuleFor(x => x.LedgerAccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateVendorBillDtoValidator : AbstractValidator<CreateVendorBillDto>
{
    public CreateVendorBillDtoValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.PayableLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A vendor bill needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateVendorBillLineDtoValidator());
    }
}

public class UpdateVendorBillDtoValidator : AbstractValidator<UpdateVendorBillDto>
{
    public UpdateVendorBillDtoValidator()
    {
        RuleFor(x => x.PayableLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A vendor bill needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateVendorBillLineDtoValidator());
    }
}

public class PayVendorBillDtoValidator : AbstractValidator<PayVendorBillDto>
{
    public PayVendorBillDtoValidator()
    {
        RuleFor(x => x.PaymentLedgerAccountId).GreaterThan(0);
    }
}
