namespace FoodOrder.Domain.Entities;

public class LedgerEntry : BaseEntity
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = "Credit"; // "Credit" | "Debit"
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
}
