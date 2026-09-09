namespace NovaERP.Domain.Entities;

/// <summary>A pending deal tied to an Account. Stage transitions into "Won"/"Lost" raise the
/// matching AutomationEvents so automation rules can react.</summary>
public class Opportunity : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      AccountId      { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public decimal  Amount         { get; set; }
    public string   Stage          { get; set; } = "Qualification"; // Qualification | Proposal | Negotiation | Won | Lost
    public DateTime? CloseDate     { get; set; }
    public int      OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public User Owner { get; set; } = null!;
}
