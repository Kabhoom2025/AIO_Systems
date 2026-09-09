using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateCustomerInvoiceLineDtoValidator : AbstractValidator<CreateCustomerInvoiceLineDto>
{
    public CreateCustomerInvoiceLineDtoValidator()
    {
        RuleFor(x => x.LedgerAccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateCustomerInvoiceDtoValidator : AbstractValidator<CreateCustomerInvoiceDto>
{
    public CreateCustomerInvoiceDtoValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.ReceivableLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A customer invoice needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateCustomerInvoiceLineDtoValidator());
    }
}

public class UpdateCustomerInvoiceDtoValidator : AbstractValidator<UpdateCustomerInvoiceDto>
{
    public UpdateCustomerInvoiceDtoValidator()
    {
        RuleFor(x => x.ReceivableLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A customer invoice needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateCustomerInvoiceLineDtoValidator());
    }
}

public class ReceiveCustomerInvoicePaymentDtoValidator : AbstractValidator<ReceiveCustomerInvoicePaymentDto>
{
    public ReceiveCustomerInvoicePaymentDtoValidator()
    {
        RuleFor(x => x.PaymentLedgerAccountId).GreaterThan(0);
    }
}
