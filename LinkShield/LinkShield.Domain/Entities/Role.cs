using LinkShield.Domain.Common;
using LinkShield.Domain.Enums;

namespace LinkShield.Domain.Entities;

public class Role : BaseEntity
{
    public SystemRole Name { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
