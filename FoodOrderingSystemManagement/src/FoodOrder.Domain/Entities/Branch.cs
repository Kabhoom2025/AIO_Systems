namespace FoodOrder.Domain.Entities;

/// <summary>
/// A physical location belonging to an Organization. Menu, tables, and orders all
/// belong to exactly one Branch; a Customer belongs to the Organization directly
/// so loyalty accounts carry across every branch of the same chain.
/// </summary>
public class Branch : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Marks the branch auto-created when the Organization was first provisioned.</summary>
    public bool IsDefault { get; set; }

    public Organization Organization { get; set; } = null!;
}
