namespace Pharmacy.Application.DTOs;

public class ExpenseDto
{
    public int      Id            { get; set; }
    public int      BranchId      { get; set; }
    public string   BranchName    { get; set; } = string.Empty;
    public string   Category      { get; set; } = string.Empty;
    public decimal  Amount        { get; set; }
    public DateTime ExpenseDate   { get; set; }
    public string   PaymentMethod { get; set; } = string.Empty;
    public string?  Notes         { get; set; }
}

public class CreateExpenseDto
{
    public int      BranchId      { get; set; }
    public string   Category      { get; set; } = "Other";
    public decimal  Amount        { get; set; }
    public DateTime ExpenseDate   { get; set; } = DateTime.UtcNow;
    public string   PaymentMethod { get; set; } = "Cash";
    public string?  Notes         { get; set; }
}

public class UpdateExpenseDto : CreateExpenseDto
{
}
