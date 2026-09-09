namespace NovaERP.Application.DTOs;

public class AiAssistantSettingsDto
{
    public int OrganizationId { get; set; }
    public string Provider { get; set; } = "Anthropic";
    public string? AnthropicModel { get; set; }
    public string? GroqModel { get; set; }
    public bool IsConfigured { get; set; }
}

public class UpdateAiAssistantSettingsDto
{
    public string Provider { get; set; } = "Anthropic";

    /// <summary>Blank means "keep the existing key unchanged" — the GET DTO never returns the
    /// raw key, so the UI always submits this blank unless the user is actively setting a new one.
    /// Each provider's key is tracked independently of which Provider is currently selected.</summary>
    public string? AnthropicApiKey { get; set; }
    public string? AnthropicModel { get; set; }

    public string? GroqApiKey { get; set; }
    public string? GroqModel { get; set; }
}
