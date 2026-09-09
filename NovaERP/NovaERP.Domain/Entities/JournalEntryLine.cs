namespace NovaERP.Domain.Entities;

/// <summary>One debit-or-credit line within a JournalEntry.</summary>
public class JournalEntryLine : BaseEntity
{
    public int     JournalEntryId  { get; set; }
    public int     LedgerAccountId { get; set; }
    public decimal Debit           { get; set; }
    public decimal Credit          { get; set; }
    public string? Description     { get; set; }
    public int     DisplayOrder    { get; set; }

    public JournalEntry JournalEntry { get; set; } = null!;
    public LedgerAccount LedgerAccount { get; set; } = null!;
}
