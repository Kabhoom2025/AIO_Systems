namespace Pharmacy.Domain.Entities;

public class Supplier : BaseEntity
{
    public int      OrganizationId { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string?  ContactPerson  { get; set; }
    public string?  Phone          { get; set; }
    public string?  Email          { get; set; }
    public string?  Address        { get; set; }
    public string?  GstNumber      { get; set; }
    public string?  PaymentTerms   { get; set; }
    public decimal  CreditLimit    { get; set; }
    public bool     IsActive       { get; set; } = true;

    public Organization                Organization   { get; set; } = null!;
    public ICollection<PurchaseOrder>  PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
