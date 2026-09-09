namespace Pharmacy.Domain.Entities;

public class StockAdjustment : BaseEntity
{
    public int      OrganizationId  { get; set; }
    public int      MedicineId      { get; set; }
    public int      MedicineBatchId { get; set; }
    public string   AdjustmentType  { get; set; } = string.Empty; // Increase | Decrease
    public int      Quantity        { get; set; }
    public string   Reason          { get; set; } = string.Empty; // Damaged | Expired | Lost | Correction | Other
    public string?  Notes           { get; set; }
    public DateTime AdjustedDate    { get; set; } = DateTime.UtcNow;

    public Organization  Organization  { get; set; } = null!;
    public Medicine      Medicine      { get; set; } = null!;
    public MedicineBatch MedicineBatch { get; set; } = null!;
}
