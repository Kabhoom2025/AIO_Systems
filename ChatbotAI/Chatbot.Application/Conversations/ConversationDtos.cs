namespace Chatbot.Application.Conversations;

public record ConversationDto(Guid Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, bool IsArchived, int MessageCount);

public record ConversationDetailDto(Guid Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, bool IsArchived, List<Messages.MessageDto> Messages);

public record CreateConversationRequest(string? Title);

public record UpdateConversationRequest(string? Title, bool? IsArchived);
