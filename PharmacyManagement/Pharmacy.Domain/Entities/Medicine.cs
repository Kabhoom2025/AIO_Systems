namespace Pharmacy.Domain.Entities;

public class Medicine : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string  GenericName    { get; set; } = string.Empty;
    public string  Category       { get; set; } = string.Empty;
    public string  DrugSchedule   { get; set; } = "OTC"; // OTC | H | H1 | X
    public string  PackType       { get; set; } = string.Empty;
    public string  PackSize       { get; set; } = string.Empty;
    public string  Unit           { get; set; } = string.Empty;
    public string  Manufacturer   { get; set; } = string.Empty;
    public string? HsnCode        { get; set; }
    public string  Sku            { get; set; } = string.Empty;
    public string  Barcode        { get; set; } = string.Empty;
    public decimal MRP            { get; set; }
    public decimal PurchasePrice  { get; set; }
    public decimal GstPercent     { get; set; } = 12;
    public string? RackLocation   { get; set; }
    public int     ReorderLevel   { get; set; } = 10;
    public bool    IsActive       { get; set; } = true;

    public Organization              Organization { get; set; } = null!;
    public ICollection<MedicineBatch> Batches     { get; set; } = new List<MedicineBatch>();
}
