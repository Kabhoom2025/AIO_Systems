using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public class CreateBankStatementLineDtoValidator : AbstractValidator<CreateBankStatementLineDto>
{
    public CreateBankStatementLineDtoValidator()
    {
        RuleFor(x => x.Amount).NotEqual(0).WithMessage("Amount cannot be zero.");
    }
}

public class CreateBankReconciliationDtoValidator : AbstractValidator<CreateBankReconciliationDto>
{
    public CreateBankReconciliationDtoValidator()
    {
        RuleFor(x => x.LedgerAccountId).GreaterThan(0);
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A bank reconciliation needs at least one statement line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateBankStatementLineDtoValidator());
    }
}

public class UpdateBankReconciliationDtoValidator : AbstractValidator<UpdateBankReconciliationDto>
{
    public UpdateBankReconciliationDtoValidator()
    {
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A bank reconciliation needs at least one statement line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateBankStatementLineDtoValidator());
    }
}

public class MatchBankStatementLineDtoValidator : AbstractValidator<MatchBankStatementLineDto>
{
    public MatchBankStatementLineDtoValidator()
    {
        RuleFor(x => x.JournalEntryLineId).GreaterThan(0);
    }
}
