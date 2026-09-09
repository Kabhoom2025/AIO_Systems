namespace Pharmacy.Domain.Entities;

public class Delivery : BaseEntity
{
    public int       OrganizationId  { get; set; }
    public int       BranchId        { get; set; }
    public int       SaleId          { get; set; }
    public int?      CustomerId      { get; set; }
    public int?      DeliveryStaffId { get; set; }
    public string    Address         { get; set; } = string.Empty;
    public DateTime  ScheduledDate   { get; set; }
    public decimal   DeliveryCharge  { get; set; }
    public string    Status          { get; set; } = "Pending"; // Pending | Assigned | OutForDelivery | Delivered | Cancelled
    public string    OtpCode         { get; set; } = string.Empty;
    public DateTime? OtpVerifiedDate { get; set; }
    public string?   Notes           { get; set; }

    public Organization Organization  { get; set; } = null!;
    public Branch        Branch        { get; set; } = null!;
    public Sale           Sale          { get; set; } = null!;
    public Customer?      Customer      { get; set; }
    public User?           DeliveryStaff { get; set; }
}
