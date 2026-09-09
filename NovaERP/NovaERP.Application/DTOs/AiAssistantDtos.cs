namespace NovaERP.Application.DTOs;

public class MessageDto
{
    public int      Id          { get; set; }
    public string   Role        { get; set; } = string.Empty;
    public string   Content     { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    /// <summary>Null for ordinary messages; "Pending"/"Confirmed"/"Cancelled" for a message
    /// that proposed a confirm-gated action (e.g. creating a Service Ticket).</summary>
    public string? PendingActionStatus { get; set; }
}

public class ConversationSummaryDto
{
    public int      Id          { get; set; }
    public string   Title       { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class ConversationDto
{
    public int    Id    { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<MessageDto> Messages { get; set; } = new();
}

public class CreateConversationDto
{
    public string Title { get; set; } = "New Conversation";
}

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
}

/// <summary>Which confirm-gated actions the current caller is allowed to have proposed to them —
/// computed by the API layer from the caller's real permissions (see AiAssistantController) and
/// passed down as plain data so the Application/Infrastructure layers never touch ASP.NET Core
/// authorization types directly.</summary>
public class AiToolContext
{
    public bool CanCreateServiceTickets { get; set; }
}

/// <summary>Internal to the AI provider abstraction — not exposed via the controller.</summary>
public class AiChatTurn
{
    public string Role    { get; set; } = string.Empty; // user | assistant | tool | assistant_tool_use
    public string Content { get; set; } = string.Empty;

    /// <summary>Only set on "assistant_tool_use"/"tool" turns.</summary>
    public string? ToolCallId { get; set; }

    /// <summary>Only set on "assistant_tool_use" turns — which tool the model called.</summary>
    public string? ToolName { get; set; }
}

/// <summary>Either FinalText is set (the model is done) or ToolCallName/ToolCallArgumentsJson/
/// ToolCallId are (the model wants to call a tool).</summary>
public class AiCompletionResult
{
    public string? FinalText             { get; set; }
    public string? ToolCallName          { get; set; }
    public string? ToolCallArgumentsJson { get; set; }
    public string? ToolCallId            { get; set; }
}
