namespace NovaERP.Domain.Entities;

/// <summary>An unqualified prospect. Converting a Lead creates an <see cref="Account"/> +
/// <see cref="Contact"/> (and optionally an <see cref="Opportunity"/>), after which the Lead
/// itself becomes read-only history (Status = "Converted").</summary>
public class Lead : BaseEntity
{
    public int      OrganizationId      { get; set; }
    public string   Name                { get; set; } = string.Empty;
    public string?  CompanyName         { get; set; }
    public string?  Email               { get; set; }
    public string?  Phone               { get; set; }
    public string?  Source              { get; set; }
    public string   Status              { get; set; } = "New"; // New | Contacted | Qualified | Converted | Lost
    public int      OwnerId             { get; set; }
    public int?     ConvertedAccountId  { get; set; }
    public DateTime? ConvertedDate      { get; set; }

    public Organization Organization { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public Account? ConvertedAccount { get; set; }
}
