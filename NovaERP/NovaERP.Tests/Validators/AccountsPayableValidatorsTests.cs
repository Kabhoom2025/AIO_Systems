using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateVendorBillDtoValidatorTests
{
    private readonly CreateVendorBillDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Required_Fields_Zero_And_No_Lines()
    {
        var dto = new CreateVendorBillDto { VendorId = 0, PayableLedgerAccountId = 0, OwnerId = 0, Lines = new() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.VendorId);
        result.ShouldHaveValidationErrorFor(x => x.PayableLedgerAccountId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Passes_For_Valid_Bill()
    {
        var dto = new CreateVendorBillDto
        {
            VendorId = 1,
            PayableLedgerAccountId = 1,
            OwnerId = 1,
            Lines = new List<CreateVendorBillLineDto>
            {
                new() { LedgerAccountId = 2, Amount = 100m, DisplayOrder = 1 }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class PayVendorBillDtoValidatorTests
{
    private readonly PayVendorBillDtoValidator _validator = new();

    [Fact]
    public void Fails_When_PaymentLedgerAccountId_Zero()
    {
        var dto = new PayVendorBillDto { PaymentLedgerAccountId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PaymentLedgerAccountId);
    }

    [Fact]
    public void Passes_For_Valid_Payment()
    {
        var dto = new PayVendorBillDto { PaymentLedgerAccountId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
