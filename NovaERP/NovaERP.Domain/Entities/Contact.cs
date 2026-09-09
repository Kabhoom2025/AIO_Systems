namespace NovaERP.Domain.Entities;

/// <summary>A person at an Account. AccountId is optional so a Contact can exist before being
/// linked to one (e.g. captured from a Lead conversion in the same transaction). UserId is
/// optional too — not every contact has a Customer Portal login; mirrors Employee.UserId.</summary>
public class Contact : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int?    AccountId      { get; set; }
    public int?    UserId         { get; set; }
    public string  FirstName      { get; set; } = string.Empty;
    public string  LastName       { get; set; } = string.Empty;
    public string? Email          { get; set; }
    public string? Phone          { get; set; }
    public string? Title          { get; set; }
    public int     OwnerId        { get; set; }

    public Organization Organization { get; set; } = null!;
    public Account? Account { get; set; }
    public User?    User    { get; set; }
    public User Owner { get; set; } = null!;
}
