namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped bank reconciliation session for one cash/bank LedgerAccount —
/// proves the book balance matches the bank statement by matching each BankStatementLine to
/// a Posted JournalEntryLine on the same account. Lines are owned/replaced wholesale, same as
/// JournalEntry's Lines.</summary>
public class BankReconciliation : BaseEntity
{
    public int      OrganizationId          { get; set; }
    public int      LedgerAccountId         { get; set; }
    public DateTime StatementDate           { get; set; } = DateTime.UtcNow.Date;
    public decimal  StatementEndingBalance  { get; set; }
    public string   Status                  { get; set; } = "Draft"; // Draft | Completed
    public int      OwnerId                 { get; set; }

    public Organization Organization { get; set; } = null!;
    public LedgerAccount LedgerAccount { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<BankStatementLine> Lines { get; set; } = new List<BankStatementLine>();
}
