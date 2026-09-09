using Chatbot.Domain.Common;

namespace Chatbot.Domain.Entities;

public class UserSettings : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Language { get; set; } = "en-US";
    public string Voice { get; set; } = "default";
    public double SpeechRate { get; set; } = 1.0;
    public double Pitch { get; set; } = 1.0;
    public double Volume { get; set; } = 1.0;
    public bool AutoSpeak { get; set; } = false;
    public bool VoiceMode { get; set; } = false;
    public string Theme { get; set; } = "system";

    /// <summary>Per-user AI provider override (e.g. "Grok"). Null means "use the server default".</summary>
    public string? AiProvider { get; set; }

    /// <summary>The user's own AI provider API key, encrypted at rest via ASP.NET Core Data Protection.
    /// Never returned to the client — only a "is one set?" flag is exposed via the API.</summary>
    public string? AiApiKeyEncrypted { get; set; }

    /// <summary>Per-user AI model override (e.g. "openai/gpt-oss-120b" on Groq). Null means "use
    /// the server default model, or a built-in default for the overridden provider".</summary>
    public string? AiModel { get; set; }
}
