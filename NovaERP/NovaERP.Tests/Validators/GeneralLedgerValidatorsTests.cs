using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateLedgerAccountDtoValidatorTests
{
    private readonly CreateLedgerAccountDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Code_Name_Empty_And_Type_Invalid()
    {
        var dto = new CreateLedgerAccountDto { Code = "", Name = "", Type = "Bogus" };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Code);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Passes_For_Valid_Account()
    {
        var dto = new CreateLedgerAccountDto { Code = "1000", Name = "Cash", Type = "Asset" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateJournalEntryDtoValidatorTests
{
    private readonly CreateJournalEntryDtoValidator _validator = new();

    [Fact]
    public void Fails_When_No_Lines_Or_OwnerId_Zero()
    {
        var dto = new CreateJournalEntryDto { OwnerId = 0, Lines = new() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Lines);
    }

    [Fact]
    public void Fails_When_Debits_Do_Not_Equal_Credits()
    {
        var dto = new CreateJournalEntryDto
        {
            OwnerId = 1,
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { LedgerAccountId = 1, Debit = 100m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = 2, Debit = 0m, Credit = 50m, DisplayOrder = 2 }
            }
        };

        var result = _validator.TestValidate(dto);

        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("debits must equal"));
    }

    [Fact]
    public void Passes_For_Balanced_Entry()
    {
        var dto = new CreateJournalEntryDto
        {
            OwnerId = 1,
            Lines = new List<CreateJournalEntryLineDto>
            {
                new() { LedgerAccountId = 1, Debit = 100m, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = 2, Debit = 0m, Credit = 100m, DisplayOrder = 2 }
            }
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
