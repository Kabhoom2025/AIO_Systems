using FluentValidation;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Validators;

public static class LedgerAccountTypes
{
    public static readonly string[] All = { "Asset", "Liability", "Equity", "Revenue", "Expense" };
}

public class CreateLedgerAccountDtoValidator : AbstractValidator<CreateLedgerAccountDto>
{
    public CreateLedgerAccountDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Type).Must(t => LedgerAccountTypes.All.Contains(t))
            .WithMessage($"Type must be one of: {string.Join(", ", LedgerAccountTypes.All)}");
    }
}

public class UpdateLedgerAccountDtoValidator : AbstractValidator<UpdateLedgerAccountDto>
{
    public UpdateLedgerAccountDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Type).Must(t => LedgerAccountTypes.All.Contains(t))
            .WithMessage($"Type must be one of: {string.Join(", ", LedgerAccountTypes.All)}");
    }
}

public class CreateJournalEntryLineDtoValidator : AbstractValidator<CreateJournalEntryLineDto>
{
    public CreateJournalEntryLineDtoValidator()
    {
        RuleFor(x => x.LedgerAccountId).GreaterThan(0);
        RuleFor(x => x.Debit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Credit).GreaterThanOrEqualTo(0);
    }
}

public class CreateJournalEntryDtoValidator : AbstractValidator<CreateJournalEntryDto>
{
    public CreateJournalEntryDtoValidator()
    {
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A journal entry needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateJournalEntryLineDtoValidator());
        RuleFor(x => x)
            .Must(x => x.Lines.Sum(l => l.Debit) == x.Lines.Sum(l => l.Credit))
            .WithMessage("Total debits must equal total credits.")
            .When(x => x.Lines.Count > 0);
    }
}

public class UpdateJournalEntryDtoValidator : AbstractValidator<UpdateJournalEntryDto>
{
    public UpdateJournalEntryDtoValidator()
    {
        RuleFor(x => x.OwnerId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A journal entry needs at least one line.");
        RuleForEach(x => x.Lines).SetValidator(new CreateJournalEntryLineDtoValidator());
        RuleFor(x => x)
            .Must(x => x.Lines.Sum(l => l.Debit) == x.Lines.Sum(l => l.Credit))
            .WithMessage("Total debits must equal total credits.")
            .When(x => x.Lines.Count > 0);
    }
}
