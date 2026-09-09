namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped AI Assistant conversation owned by a single User (not a Role) — the
/// exact self-service shape as Dashboard, every authenticated user manages their own.</summary>
public class Conversation : BaseEntity
{
    public int    OrganizationId { get; set; }
    public int    UserId         { get; set; }
    public string Title          { get; set; } = "New Conversation";

    public Organization Organization { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
