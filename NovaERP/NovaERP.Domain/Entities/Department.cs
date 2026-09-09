namespace NovaERP.Domain.Entities;

public class Department : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int?    BranchId       { get; set; }
    public int?    ParentId       { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string  Code           { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public string? CostCenter     { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization           Organization { get; set; } = null!;
    public Branch?                Branch       { get; set; }
    public Department?            Parent       { get; set; }
    public ICollection<Department> Children    { get; set; } = new List<Department>();
}
