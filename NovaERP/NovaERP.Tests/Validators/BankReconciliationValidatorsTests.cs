using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateBankReconciliationDtoValidatorTests
{
    private readonly CreateBankReconciliationDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Required_Fields_Zero_And_No_Lines()
    {
        var dto = new CreateBankReconciliationDto { LedgerAccountId = 0, OwnerId = 0, Lines = new() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.LedgerAccountId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Fails_When_Line_Amount_Is_Zero()
    {
        var dto = new CreateBankReconciliationDto
        {
            LedgerAccountId = 1,
            OwnerId = 1,
            Lines = new List<CreateBankStatementLineDto> { new() { Amount = 0m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("cannot be zero"));
    }

    [Fact]
    public void Passes_For_Valid_Reconciliation()
    {
        var dto = new CreateBankReconciliationDto
        {
            LedgerAccountId = 1,
            OwnerId = 1,
            Lines = new List<CreateBankStatementLineDto> { new() { Amount = 100m, DisplayOrder = 1 } }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class MatchBankStatementLineDtoValidatorTests
{
    private readonly MatchBankStatementLineDtoValidator _validator = new();

    [Fact]
    public void Fails_When_JournalEntryLineId_Zero()
    {
        var dto = new MatchBankStatementLineDto { JournalEntryLineId = 0 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.JournalEntryLineId);
    }

    [Fact]
    public void Passes_For_Valid_Match()
    {
        var dto = new MatchBankStatementLineDto { JournalEntryLineId = 1 };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
