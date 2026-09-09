using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateVendorDtoValidatorTests
{
    private readonly CreateVendorDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_Empty_Or_Owner_Missing()
    {
        var dto = new CreateVendorDto { Name = "", OwnerId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
    }

    [Fact]
    public void Passes_For_Valid_Vendor()
    {
        var dto = new CreateVendorDto { Name = "Bharat Steel Traders", OwnerId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateRfqRequestDtoValidatorTests
{
    private readonly CreateRfqRequestDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Items_And_InvitedVendors_Missing()
    {
        var dto = new CreateRfqRequestDto { Title = "Steel sheets", OwnerId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Items);
        result.ShouldHaveValidationErrorFor(x => x.InvitedVendorIds);
    }

    [Fact]
    public void Passes_For_Valid_Rfq()
    {
        var dto = new CreateRfqRequestDto
        {
            Title = "Steel sheets",
            OwnerId = 1,
            Items = { new CreateRfqItemDto { ItemName = "Cold-rolled sheet", Quantity = 500m, DisplayOrder = 1 } },
            InvitedVendorIds = { 1, 2 }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class RecordRfqQuoteDtoValidatorTests
{
    private readonly RecordRfqQuoteDtoValidator _validator = new();

    [Fact]
    public void Fails_When_VendorId_Zero_Or_Amount_Negative()
    {
        var dto = new RecordRfqQuoteDto { VendorId = 0, QuotedAmount = -5m };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.VendorId);
        result.ShouldHaveValidationErrorFor(x => x.QuotedAmount);
    }

    [Fact]
    public void Passes_For_Valid_Quote()
    {
        var dto = new RecordRfqQuoteDto { VendorId = 1, QuotedAmount = 425000m, Notes = "Includes freight" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
