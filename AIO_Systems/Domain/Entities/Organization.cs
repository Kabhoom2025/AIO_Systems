namespace AIO_Systems.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Timezone { get; set; }
    public string? Currency { get; set; } = "INR";

    /// <summary>
    /// Stable URL-safe slug identifying the tenant externally (e.g. "acme-foods").
    /// Forward-looking only for this pass — not used in JWT claims or routing yet.
    /// </summary>
    public string TenantKey { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
