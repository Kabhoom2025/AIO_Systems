namespace HRMS.Application.DTOs;

public class ExpenseClaimDto
{
    public int       Id               { get; set; }
    public int       EmployeeId       { get; set; }
    public string    EmployeeName     { get; set; } = string.Empty;
    public string    EmployeeCode     { get; set; } = string.Empty;
    public string    Category         { get; set; } = string.Empty;
    public DateOnly  ClaimDate        { get; set; }
    public decimal   Amount           { get; set; }
    public string    Currency         { get; set; } = string.Empty;
    public string    Description      { get; set; } = string.Empty;
    public string?   ReceiptUrl       { get; set; }
    public string    Status           { get; set; } = string.Empty;
    public int?      ReviewedByUserId { get; set; }
    public string?   ReviewedByName   { get; set; }
    public DateTime? ReviewedAt       { get; set; }
    public string?   ReviewNotes      { get; set; }
    public DateTime? ReimbursedAt     { get; set; }
}

public class CreateExpenseClaimDto
{
    public string   Category    { get; set; } = "Travel";
    public DateOnly ClaimDate   { get; set; }
    public decimal  Amount      { get; set; }
    public string   Currency    { get; set; } = "INR";
    public string   Description { get; set; } = string.Empty;
    public string?  ReceiptUrl  { get; set; }
}

public class ReviewExpenseDto
{
    public string? Notes { get; set; }
}

public class ExpenseSummaryDto
{
    public int     Pending               { get; set; }
    public int     Approved              { get; set; }
    public int     Reimbursed            { get; set; }
    public int     Rejected              { get; set; }
    public decimal TotalPendingAmount    { get; set; }
    public decimal TotalReimbursedAmount { get; set; }
}
