using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateBranchDtoValidatorTests
{
    private readonly CreateBranchDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Name_And_Code_Are_Empty()
    {
        var dto = new CreateBranchDto { Name = "", Code = "" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Passes_For_Valid_Branch()
    {
        var dto = new CreateBranchDto { Name = "Head Office", Code = "HO", Email = "ho@example.com" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_When_Email_Is_Invalid()
    {
        var dto = new CreateBranchDto { Name = "Head Office", Code = "HO", Email = "not-an-email" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
