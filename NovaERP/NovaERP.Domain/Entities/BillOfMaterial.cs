namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped recipe for one finished-good Product, made up of one or more
/// BomComponent rows. One BOM per Product (enforced by a unique index) — no versioning, no
/// multi-level explosion (a component can't itself have a BOM in this pass).</summary>
public class BillOfMaterial : BaseEntity
{
    public int    OrganizationId { get; set; }
    public int    ProductId      { get; set; }
    public bool   IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<BomComponent> Components { get; set; } = new List<BomComponent>();
}
