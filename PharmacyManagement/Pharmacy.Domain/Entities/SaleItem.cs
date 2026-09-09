namespace Pharmacy.Domain.Entities;

public class SaleItem : BaseEntity
{
    public int     SaleId          { get; set; }
    public int     MedicineId      { get; set; }
    public int     MedicineBatchId { get; set; }
    public int     Quantity        { get; set; }
    public decimal UnitPrice       { get; set; }
    public decimal DiscountAmount  { get; set; }
    public decimal GstPercent      { get; set; }
    public decimal LineTotal       { get; set; }

    public Sale          Sale          { get; set; } = null!;
    public Medicine       Medicine     { get; set; } = null!;
    public MedicineBatch  MedicineBatch { get; set; } = null!;
}
