namespace Chatbot.Application.Common.Models;

/// <summary>
/// Configuration for how much conversation history is sent to the AI provider as context.
/// Bound from the "Chat" configuration section.
/// </summary>
public class ChatOptions
{
    public const string SectionName = "Chat";

    /// <summary>Maximum number of prior messages (user + assistant) included as context for a new turn.</summary>
    public int MaxHistoryMessages { get; set; } = 20;

    /// <summary>Optional system prompt prepended to every conversation.</summary>
    public string SystemPrompt { get; set; } = "You are a helpful, concise AI assistant.";
}
