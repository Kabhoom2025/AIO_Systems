namespace Pharmacy.Domain.Entities;

public class Expense : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      BranchId       { get; set; }
    public string   Category       { get; set; } = "Other"; // Rent | Utilities | Salaries | Marketing | Maintenance | Other
    public decimal  Amount         { get; set; }
    public DateTime ExpenseDate    { get; set; } = DateTime.UtcNow;
    public string   PaymentMethod  { get; set; } = "Cash"; // Cash | Card | UPI | Bank Transfer
    public string?  Notes          { get; set; }

    public Organization Organization { get; set; } = null!;
    public Branch        Branch      { get; set; } = null!;
}
