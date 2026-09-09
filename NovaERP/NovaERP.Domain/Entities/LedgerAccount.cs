namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped chart-of-accounts entry — flat, no parent/sub-account hierarchy in
/// this pass. Not to be confused with the CRM Account (customer/prospect company).</summary>
public class LedgerAccount : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string Code           { get; set; } = string.Empty;
    public string Name           { get; set; } = string.Empty;
    public string Type           { get; set; } = string.Empty; // Asset | Liability | Equity | Revenue | Expense
    public bool   IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
