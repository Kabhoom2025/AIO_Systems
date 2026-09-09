namespace NovaERP.Application.DTOs;

public class LedgerAccountDto
{
    public int    Id       { get; set; }
    public string Code     { get; set; } = string.Empty;
    public string Name     { get; set; } = string.Empty;
    public string Type     { get; set; } = string.Empty;
    public bool   IsActive { get; set; }

    /// <summary>Sum of Debit - Credit across this account's Posted journal entry lines —
    /// computed on read, not stored.</summary>
    public decimal Balance { get; set; }
}

public class CreateLedgerAccountDto
{
    public string Code     { get; set; } = string.Empty;
    public string Name     { get; set; } = string.Empty;
    public string Type     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
}

/// <summary>Code is immutable after creation — same convention as Product.Sku/Warehouse.Code.</summary>
public class UpdateLedgerAccountDto
{
    public string Name     { get; set; } = string.Empty;
    public string Type     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
}

public class JournalEntryLineDto
{
    public int     Id                { get; set; }
    public int     LedgerAccountId   { get; set; }
    public string  LedgerAccountName { get; set; } = string.Empty;
    public string  LedgerAccountCode { get; set; } = string.Empty;
    public decimal Debit             { get; set; }
    public decimal Credit            { get; set; }
    public string? Description       { get; set; }
    public int     DisplayOrder      { get; set; }
}

public class CreateJournalEntryLineDto
{
    public int     LedgerAccountId { get; set; }
    public decimal Debit           { get; set; }
    public decimal Credit          { get; set; }
    public string? Description     { get; set; }
    public int     DisplayOrder    { get; set; }
}

public class JournalEntryDto
{
    public int      Id          { get; set; }
    public string   EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate   { get; set; }
    public string?  Description { get; set; }
    public string   Status      { get; set; } = string.Empty;
    public int      OwnerId     { get; set; }
    public string   OwnerName   { get; set; } = string.Empty;
    public List<JournalEntryLineDto> Lines { get; set; } = new();

    /// <summary>Both computed on read — always equal, since balance is enforced at save time.</summary>
    public decimal TotalDebit  { get; set; }
    public decimal TotalCredit { get; set; }
}

/// <summary>Lines are owned by the entry and replaced wholesale on update — same "replace-all"
/// approach as UpdateTaxCodeDto's Components/UpdateBillOfMaterialDto's Components.</summary>
public class CreateJournalEntryDto
{
    public DateTime EntryDate   { get; set; } = DateTime.UtcNow.Date;
    public string?  Description { get; set; }
    public int      OwnerId     { get; set; }
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

public class UpdateJournalEntryDto
{
    public DateTime EntryDate   { get; set; } = DateTime.UtcNow.Date;
    public string?  Description { get; set; }
    public int      OwnerId     { get; set; }
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}
