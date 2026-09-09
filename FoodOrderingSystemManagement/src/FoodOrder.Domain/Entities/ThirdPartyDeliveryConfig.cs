namespace FoodOrder.Domain.Entities;

public class ThirdPartyDeliveryConfig : BaseEntity
{
    public string  Provider     { get; set; } = string.Empty;
    public string  ApiKey       { get; set; } = string.Empty;
    public string? ApiSecret    { get; set; }
    public string? WebhookUrl   { get; set; }
    public bool    IsEnabled    { get; set; } = false;
    public int?    OrganizationId { get; set; }

    public Organization? Organization { get; set; }
}
