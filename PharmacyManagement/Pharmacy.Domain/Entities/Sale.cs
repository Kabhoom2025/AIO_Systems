namespace Pharmacy.Domain.Entities;

public class Sale : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      BranchId       { get; set; }
    public int?     CustomerId     { get; set; }
    public int?     PatientId      { get; set; }
    public string   Channel        { get; set; } = "POS"; // POS | Online
    public string   InvoiceNumber  { get; set; } = string.Empty;
    public DateTime SaleDate       { get; set; } = DateTime.UtcNow;
    public decimal  Subtotal       { get; set; }
    public decimal  DiscountAmount { get; set; }
    public decimal  TaxAmount      { get; set; }
    public decimal  TotalAmount    { get; set; }
    public string   PaymentMethod  { get; set; } = "Cash"; // Cash | Card | UPI | Wallet | Credit
    public string   Status         { get; set; } = "Completed";

    public Organization           Organization { get; set; } = null!;
    public Branch                 Branch       { get; set; } = null!;
    public Customer?              Customer     { get; set; }
    public Patient?               Patient      { get; set; }
    public ICollection<SaleItem>  Items        { get; set; } = new List<SaleItem>();
}
