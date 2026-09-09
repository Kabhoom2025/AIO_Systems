namespace HRMS.Domain.Entities;

public class ExpenseClaim : BaseEntity
{
    public int      OrganizationId   { get; set; }
    public int      EmployeeId       { get; set; }
    public string   Category         { get; set; } = "Travel"; // Travel | Food | Accommodation | Mileage | Other
    public DateOnly ClaimDate        { get; set; }
    public decimal  Amount           { get; set; }
    public string   Currency         { get; set; } = "INR";
    public string   Description      { get; set; } = string.Empty;
    public string?  ReceiptUrl       { get; set; }
    public string   Status           { get; set; } = "Pending"; // Pending | Approved | Rejected | Reimbursed
    public int?     ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt      { get; set; }
    public string?  ReviewNotes      { get; set; }
    public DateTime? ReimbursedAt    { get; set; }

    public Organization Organization   { get; set; } = null!;
    public Employee     Employee       { get; set; } = null!;
    public User?        ReviewedByUser { get; set; }
}
