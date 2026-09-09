using FluentValidation.TestHelper;
using NovaERP.Application.DTOs;
using NovaERP.Application.Validators;
using Xunit;

namespace NovaERP.Tests.Validators;

public class CreateEmployeeCompensationDtoValidatorTests
{
    private readonly CreateEmployeeCompensationDtoValidator _validator = new();

    [Fact]
    public void Fails_When_EmployeeId_Zero_And_Amounts_Negative()
    {
        var dto = new CreateEmployeeCompensationDto
        {
            EmployeeId = 0, BasicSalary = -1, Hra = -1, OtherAllowances = -1, Deductions = -1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        result.ShouldHaveValidationErrorFor(x => x.BasicSalary);
        result.ShouldHaveValidationErrorFor(x => x.Hra);
        result.ShouldHaveValidationErrorFor(x => x.OtherAllowances);
        result.ShouldHaveValidationErrorFor(x => x.Deductions);
    }

    [Fact]
    public void Passes_For_Valid_Compensation()
    {
        var dto = new CreateEmployeeCompensationDto
        {
            EmployeeId = 1, BasicSalary = 50000, Hra = 20000, OtherAllowances = 5000, Deductions = 7500
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreatePayRunDtoValidatorTests
{
    private readonly CreatePayRunDtoValidator _validator = new();

    [Fact]
    public void Fails_When_Month_Out_Of_Range_And_Ids_Zero()
    {
        var dto = new CreatePayRunDto
        {
            PeriodMonth = 13, PeriodYear = 0, ExpenseLedgerAccountId = 0,
            DeductionsPayableLedgerAccountId = 0, OwnerId = 0
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PeriodMonth);
        result.ShouldHaveValidationErrorFor(x => x.PeriodYear);
        result.ShouldHaveValidationErrorFor(x => x.ExpenseLedgerAccountId);
        result.ShouldHaveValidationErrorFor(x => x.DeductionsPayableLedgerAccountId);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
    }

    [Fact]
    public void Passes_For_Valid_PayRun()
    {
        var dto = new CreatePayRunDto
        {
            PeriodMonth = 6, PeriodYear = 2026, ExpenseLedgerAccountId = 1,
            DeductionsPayableLedgerAccountId = 2, OwnerId = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
