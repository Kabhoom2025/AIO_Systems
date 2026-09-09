namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped double-entry journal entry. Lines are owned/replaced wholesale,
/// same as TaxCode's Components and BillOfMaterial's Components. Debits must equal credits
/// across the lines — enforced by the DTO validator (a pure sum of the submitted lines, no DB
/// access needed), so once persisted an entry is always balanced.</summary>
public class JournalEntry : BaseEntity
{
    public int      OrganizationId { get; set; }
    public string   EntryNumber    { get; set; } = string.Empty;
    public DateTime EntryDate      { get; set; } = DateTime.UtcNow.Date;
    public string?  Description    { get; set; }
    public string   Status         { get; set; } = "Draft"; // Draft | Posted | Voided
    public int      OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
