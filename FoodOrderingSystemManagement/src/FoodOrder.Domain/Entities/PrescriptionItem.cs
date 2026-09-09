namespace FoodOrder.Domain.Entities;

public class PrescriptionItem
{
    public int    Id             { get; set; }
    public int    PrescriptionId { get; set; }
    public string MedicineName   { get; set; } = string.Empty;
    public string Dosage         { get; set; } = string.Empty;  // "1-0-1"
    public string Duration       { get; set; } = string.Empty;  // "5 days"
    public int    Quantity       { get; set; }
    public bool   IsDispensed    { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
