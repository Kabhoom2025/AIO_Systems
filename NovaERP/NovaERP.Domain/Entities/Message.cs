namespace NovaERP.Domain.Entities;

/// <summary>One turn in a Conversation. "tool" rows capture the raw JSON result of a
/// get_widget_data call for transcript completeness — the Angular UI only renders user/
/// assistant bubbles, filtering tool rows out client-side. The PendingAction* columns are only
/// set on an assistant message that proposes a confirm-gated action (e.g. creating a Service
/// Ticket) — the action is never executed until the human confirms it via a separate endpoint.</summary>
public class Message : BaseEntity
{
    public int    ConversationId { get; set; }
    public string Role           { get; set; } = string.Empty; // user | assistant | tool
    public string Content        { get; set; } = string.Empty;

    public string? PendingActionType        { get; set; } // e.g. "CreateServiceTicket"
    public string? PendingActionPayloadJson { get; set; }
    public string? PendingActionStatus      { get; set; } // Pending | Confirmed | Cancelled

    public Conversation Conversation { get; set; } = null!;
}
