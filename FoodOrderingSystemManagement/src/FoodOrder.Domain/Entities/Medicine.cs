namespace FoodOrder.Domain.Entities;

public class Medicine : BaseEntity
{
    public int     OrganizationId  { get; set; }
    public string  Name            { get; set; } = string.Empty;   // Brand name
    public string  GenericName     { get; set; } = string.Empty;
    public string  Category        { get; set; } = string.Empty;   // Antibiotic, Analgesic…
    public string  DrugSchedule    { get; set; } = "OTC";          // OTC | H | H1 | X
    public string  PackType        { get; set; } = string.Empty;   // Tablet, Syrup…
    public string  PackSize        { get; set; } = string.Empty;   // "10 Tablets", "100ml"
    public string  Unit            { get; set; } = string.Empty;   // Strip / Bottle / Vial
    public string  Manufacturer    { get; set; } = string.Empty;
    public string? HsnCode         { get; set; }
    public decimal MRP             { get; set; }
    public decimal PurchasePrice   { get; set; }
    public decimal GstPercent      { get; set; } = 12;
    public string? RackLocation    { get; set; }
    public int     ReorderLevel    { get; set; } = 10;
    public bool    IsActive        { get; set; } = true;

    public Organization           Organization { get; set; } = null!;
    public ICollection<MedicineBatch> Batches  { get; set; } = new List<MedicineBatch>();
}
