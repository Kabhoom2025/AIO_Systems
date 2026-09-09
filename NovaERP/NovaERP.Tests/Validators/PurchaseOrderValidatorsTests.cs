using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreatePurchaseOrderDtoValidatorTests
{
    private readonly CreatePurchaseOrderDtoValidator _validator = new();

    [Fact]
    public void Fails_When_VendorId_Owner_Or_Lines_Missing()
    {
        var dto = new CreatePurchaseOrderDto { VendorId = 0, OwnerId = 0, Lines = new List<CreatePurchaseOrderLineDto>() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.VendorId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Fails_When_Line_Quantity_Zero()
    {
        var dto = new CreatePurchaseOrderDto
        {
            VendorId = 1, OwnerId = 1,
            Lines = { new CreatePurchaseOrderLineDto { ItemName = "Steel sheet", Quantity = 0, UnitPrice = 10m } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Lines[0].Quantity");
    }

    [Fact]
    public void Passes_For_Valid_Order()
    {
        var dto = new CreatePurchaseOrderDto
        {
            VendorId = 1, OwnerId = 1,
            Lines = { new CreatePurchaseOrderLineDto { ItemName = "Steel sheet", Quantity = 5m, UnitPrice = 10m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdatePurchaseOrderDtoValidatorTests
{
    private readonly UpdatePurchaseOrderDtoValidator _validator = new();

    [Fact]
    public void Fails_When_No_Lines()
    {
        var dto = new UpdatePurchaseOrderDto { OwnerId = 1, Lines = new List<CreatePurchaseOrderLineDto>() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Passes_For_Valid_Update()
    {
        var dto = new UpdatePurchaseOrderDto
        {
            OwnerId = 1,
            Lines = { new CreatePurchaseOrderLineDto { ItemName = "Steel sheet", Quantity = 5m, UnitPrice = 10m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
