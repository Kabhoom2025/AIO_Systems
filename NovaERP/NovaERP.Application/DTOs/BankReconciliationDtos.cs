namespace NovaERP.Application.DTOs;

public class BankStatementLineDto
{
    public int      Id                        { get; set; }
    public DateTime TransactionDate           { get; set; }
    public string?  Description               { get; set; }
    public decimal  Amount                    { get; set; }
    public int?     MatchedJournalEntryLineId { get; set; }
    public int      DisplayOrder              { get; set; }

    /// <summary>Computed on read — true when MatchedJournalEntryLineId is set.</summary>
    public bool IsMatched { get; set; }
}

public class CreateBankStatementLineDto
{
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow.Date;
    public string?  Description     { get; set; }
    public decimal  Amount          { get; set; }
    public int      DisplayOrder    { get; set; }
}

public class BankReconciliationDto
{
    public int      Id                      { get; set; }
    public int      LedgerAccountId         { get; set; }
    public string   LedgerAccountName       { get; set; } = string.Empty;
    public DateTime StatementDate           { get; set; }
    public decimal  StatementEndingBalance  { get; set; }
    public string   Status                  { get; set; } = string.Empty;
    public int      OwnerId                 { get; set; }
    public string   OwnerName               { get; set; } = string.Empty;
    public List<BankStatementLineDto> Lines { get; set; } = new();

    /// <summary>Both computed on read — BookBalance is the LedgerAccount's live balance,
    /// Difference = StatementEndingBalance - BookBalance, UnmatchedCount = lines still needing
    /// a match.</summary>
    public decimal BookBalance    { get; set; }
    public decimal Difference     { get; set; }
    public int     UnmatchedCount { get; set; }
}

public class CreateBankReconciliationDto
{
    public int      LedgerAccountId        { get; set; }
    public DateTime StatementDate          { get; set; } = DateTime.UtcNow.Date;
    public decimal  StatementEndingBalance { get; set; }
    public int      OwnerId                { get; set; }
    public List<CreateBankStatementLineDto> Lines { get; set; } = new();
}

public class UpdateBankReconciliationDto
{
    public DateTime StatementDate          { get; set; } = DateTime.UtcNow.Date;
    public decimal  StatementEndingBalance { get; set; }
    public int      OwnerId                { get; set; }
    public List<CreateBankStatementLineDto> Lines { get; set; } = new();
}

public class MatchBankStatementLineDto
{
    public int JournalEntryLineId { get; set; }
}

/// <summary>A candidate JournalEntryLine for matching — Posted, on the target LedgerAccount,
/// not yet matched by any BankStatementLine.</summary>
public class MatchCandidateDto
{
    public int      JournalEntryLineId { get; set; }
    public string   JournalEntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate          { get; set; }
    public string?  Description        { get; set; }
    public decimal  Amount             { get; set; }
}
