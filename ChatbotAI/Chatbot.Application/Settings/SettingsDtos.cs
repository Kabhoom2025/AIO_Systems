namespace Chatbot.Application.Settings;

public record UserSettingsDto(
    string Language,
    string Voice,
    double SpeechRate,
    double Pitch,
    double Volume,
    bool AutoSpeak,
    bool VoiceMode,
    string Theme,
    string? AiProvider,
    bool HasCustomAiApiKey,
    string? AiModel);

public record UpdateUserSettingsRequest(
    string? Language,
    string? Voice,
    double? SpeechRate,
    double? Pitch,
    double? Volume,
    bool? AutoSpeak,
    bool? VoiceMode,
    string? Theme,
    /// <summary>Null = leave unchanged. Empty string = clear (revert to server default provider/key).</summary>
    string? AiProvider,
    /// <summary>Null = leave unchanged. Empty string = clear the stored key. Non-empty = set/replace it.</summary>
    string? AiApiKey,
    /// <summary>Null = leave unchanged. Empty string = clear (fall back to a built-in default model for the provider).</summary>
    string? AiModel);
