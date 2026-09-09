namespace Chatbot.Application.Messages;

public record MessageDto(Guid Id, Guid ConversationId, string Role, string Content, DateTime CreatedAt, int? TokenCount, string? Model);
