namespace NovaERP.Domain.Entities;

/// <summary>One row per Organization holding the AI Assistant's provider credentials. Provider
/// selects which of AnthropicApiKey/GroqApiKey is currently active (see IAiProvider.IsConfiguredAsync,
/// which checks only the selected provider's key). Both providers' keys/models are stored
/// independently so switching Provider never clobbers the other provider's saved credential.</summary>
public class AiAssistantSettings : BaseEntity
{
    public int OrganizationId { get; set; }

    public string Provider { get; set; } = "Anthropic";

    public string? AnthropicApiKey { get; set; }
    public string? AnthropicModel { get; set; }

    public string? GroqApiKey { get; set; }
    public string? GroqModel { get; set; }

    public Organization Organization { get; set; } = null!;
}
