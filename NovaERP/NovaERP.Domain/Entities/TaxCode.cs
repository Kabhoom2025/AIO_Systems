namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped tax code (e.g. "GST18") made up of one or more <see cref="TaxComponent"/>
/// rows. A single component is a plain flat-rate tax; multiple components model a compound
/// tax (e.g. GST split into CGST + SGST). The effective total rate is the sum of its
/// components' rates, computed on read rather than stored.</summary>
public class TaxCode : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string Code           { get; set; } = string.Empty;
    public string Name           { get; set; } = string.Empty;
    public bool   IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<TaxComponent> Components { get; set; } = new List<TaxComponent>();
}
