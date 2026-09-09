namespace Pharmacy.Application.DTOs;

public class StockAdjustmentDto
{
    public int      Id              { get; set; }
    public int      MedicineId      { get; set; }
    public string   MedicineName    { get; set; } = string.Empty;
    public int      MedicineBatchId { get; set; }
    public string   BatchNumber     { get; set; } = string.Empty;
    public string   AdjustmentType  { get; set; } = string.Empty;
    public int      Quantity        { get; set; }
    public string   Reason          { get; set; } = string.Empty;
    public string?  Notes           { get; set; }
    public DateTime AdjustedDate    { get; set; }
}

public class CreateStockAdjustmentDto
{
    public int     MedicineId      { get; set; }
    public int     MedicineBatchId { get; set; }
    public string  AdjustmentType  { get; set; } = string.Empty;
    public int     Quantity        { get; set; }
    public string  Reason          { get; set; } = string.Empty;
    public string? Notes           { get; set; }
}
