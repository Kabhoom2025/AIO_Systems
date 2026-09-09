using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateEmployeeCompensationDtoValidator : AbstractValidator<CreateEmployeeCompensationDto>
{
    public CreateEmployeeCompensationDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.BasicSalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Hra).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OtherAllowances).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Deductions).GreaterThanOrEqualTo(0);
    }
}

public class UpdateEmployeeCompensationDtoValidator : AbstractValidator<UpdateEmployeeCompensationDto>
{
    public UpdateEmployeeCompensationDtoValidator()
    {
        RuleFor(x => x.BasicSalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Hra).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OtherAllowances).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Deductions).GreaterThanOrEqualTo(0);
    }
}

public class CreatePayRunDtoValidator : AbstractValidator<CreatePayRunDto>
{
    public CreatePayRunDtoValidator()
    {
        RuleFor(x => x.PeriodMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.PeriodYear).GreaterThan(2000);
        RuleFor(x => x.ExpenseLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.DeductionsPayableLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class UpdatePayRunDtoValidator : AbstractValidator<UpdatePayRunDto>
{
    public UpdatePayRunDtoValidator()
    {
        RuleFor(x => x.PeriodMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.PeriodYear).GreaterThan(2000);
        RuleFor(x => x.ExpenseLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.DeductionsPayableLedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
    }
}

public class PayPayRunDtoValidator : AbstractValidator<PayPayRunDto>
{
    public PayPayRunDtoValidator()
    {
        RuleFor(x => x.PaymentLedgerAccountId).GreaterThan(0);
    }
}
