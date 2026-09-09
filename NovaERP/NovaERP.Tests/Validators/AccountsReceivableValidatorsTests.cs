using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateCustomerInvoiceDtoValidatorTests
{
    private readonly CreateCustomerInvoiceDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Required_Fields_Zero_And_No_Lines()
    {
        var dto = new CreateCustomerInvoiceDto { AccountId = 0, ReceivableLedgerAccountId = 0, OwnerId = 0, Lines = new() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AccountId);
        result.ShouldHaveValidationErrorFor(x => x.ReceivableLedgerAccountId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Passes_For_Valid_Invoice()
    {
        var dto = new CreateCustomerInvoiceDto
        {
            AccountId = 1,
            ReceivableLedgerAccountId = 1,
            OwnerId = 1,
            Lines = new List<CreateCustomerInvoiceLineDto>
            {
                new() { LedgerAccountId = 2, Amount = 100m, DisplayOrder = 1 }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class ReceiveCustomerInvoicePaymentDtoValidatorTests
{
    private readonly ReceiveCustomerInvoicePaymentDtoValidator _validator = new();

    [Fact]
    public void Fails_When_PaymentLedgerAccountId_Zero()
    {
        var dto = new ReceiveCustomerInvoicePaymentDto { PaymentLedgerAccountId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PaymentLedgerAccountId);
    }

    [Fact]
    public void Passes_For_Valid_Payment()
    {
        var dto = new ReceiveCustomerInvoicePaymentDto { PaymentLedgerAccountId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
