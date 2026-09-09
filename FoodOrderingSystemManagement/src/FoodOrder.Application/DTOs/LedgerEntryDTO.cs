namespace FoodOrder.Application.DTOs;

public class LedgerEntryDTO
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public decimal RunningBalance { get; set; }
}

public class CreateLedgerEntryRequest
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = "Credit";
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class LedgerSummaryDTO
{
    public DateTime Date { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal Net { get; set; }
    public int EntryCount { get; set; }
}
