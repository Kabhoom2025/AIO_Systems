namespace Pharmacy.Domain.Entities;

public class Branch : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string  Code           { get; set; } = string.Empty;
    public string  Type           { get; set; } = "Retail"; // Retail | Warehouse
    public string? Address        { get; set; }
    public string? Phone          { get; set; }
    public string? Email          { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization                Organization    { get; set; } = null!;
    public ICollection<User>           Users           { get; set; } = new List<User>();
    public ICollection<MedicineBatch>  MedicineBatches { get; set; } = new List<MedicineBatch>();
}
