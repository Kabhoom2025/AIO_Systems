namespace NovaERP.Domain.Entities;

/// <summary>One transaction line from a bank statement (deposits positive, withdrawals
/// negative). Matched against a Posted JournalEntryLine on the same LedgerAccount to prove
/// the books agree with the bank.</summary>
public class BankStatementLine : BaseEntity
{
    public int      BankReconciliationId    { get; set; }
    public DateTime TransactionDate         { get; set; } = DateTime.UtcNow.Date;
    public string?  Description             { get; set; }
    public decimal  Amount                  { get; set; }
    public int?     MatchedJournalEntryLineId { get; set; }
    public int      DisplayOrder            { get; set; }

    public BankReconciliation BankReconciliation { get; set; } = null!;
    public JournalEntryLine? MatchedJournalEntryLine { get; set; }
}
