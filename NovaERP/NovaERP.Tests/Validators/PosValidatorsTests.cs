using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreatePosSaleDtoValidatorTests
{
    private readonly CreatePosSaleDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Warehouse_Revenue_Owner_Zero_And_No_Lines()
    {
        var dto = new CreatePosSaleDto { WarehouseId = 0, RevenueLedgerAccountId = 0, OwnerId = 0, Lines = new() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
        result.ShouldHaveValidationErrorFor(x => x.RevenueLedgerAccountId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Passes_For_Valid_Sale()
    {
        var dto = new CreatePosSaleDto
        {
            WarehouseId = 1, RevenueLedgerAccountId = 1, OwnerId = 1,
            Lines = new List<CreatePosSaleLineDto> { new() { ProductId = 1, Quantity = 2m, UnitPrice = 100m } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CompletePosSaleDtoValidatorTests
{
    private readonly CompletePosSaleDtoValidator _validator = new();

    [Fact]
    public void Fails_When_PaymentLedgerAccountId_Zero()
    {
        var dto = new CompletePosSaleDto { PaymentLedgerAccountId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PaymentLedgerAccountId);
    }

    [Fact]
    public void Passes_For_Valid_Dto()
    {
        var dto = new CompletePosSaleDto { PaymentLedgerAccountId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
