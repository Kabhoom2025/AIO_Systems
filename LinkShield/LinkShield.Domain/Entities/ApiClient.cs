using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class ApiClient : BaseEntity
{
    public string OrganizationName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DailyQuota { get; set; } = 1000;
    public int RequestsPerMinute { get; set; } = 60;

    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
